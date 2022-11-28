using TrueSight.Common;
using System.Reflection;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Thinktecture;
using Z.EntityFramework.Extensions;
using RideSharing.Models;
using RideSharing.Rpc;
using RideSharing.Handlers.Configuration;
using RideSharing.Services;
using RideSharing.Repositories;

namespace RideSharing
{
    public class MyDesignTimeService : DesignTimeService { }
    public class Startup
    {
        public Startup(IHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

            Configuration = builder.Build();
            License.Activate(Configuration["Config:TrueSightLicense"]);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            LicenseManager.AddLicense("2456;100-FPT", "3f0586d1-0216-5005-8b7a-9080b0bedb5e");
            string licenseErrorMessage;
            if (!LicenseManager.ValidateLicense(out licenseErrorMessage))
                throw new Exception(licenseErrorMessage);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            _ = DataEntity.InformationResource;
            _ = DataEntity.WarningResource;
            _ = DataEntity.ErrorResource;

            services.AddControllers().AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK";
                });

            services.AddSingleton<IRedisStore, RedisStore>();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<IPooledObjectPolicy<IModel>, RabbitModelPooledObjectPolicy>();
            services.AddSingleton<IRabbitManager, RabbitManager>();
            services.AddHostedService<ConsumeRabbitMQHostedService>();

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DataContext"), sqlOptions =>
                {
                    sqlOptions.AddTempTableSupport();
                });
                options.AddInterceptors(new HintCommandInterceptor());
            });
            EntityFrameworkManager.ContextFactory = context =>
            {
               var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DataContext"), sqlOptions =>
                {
                    sqlOptions.AddTempTableSupport();
                });
                DataContext DataContext = new DataContext(optionsBuilder.Options);
                return DataContext;
            };

            Assembly[] assemblies = new[] { Assembly.GetAssembly(typeof(IServiceScoped)),
                Assembly.GetAssembly(typeof(Startup)) };
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<IServiceScoped>())
                     .AsImplementedInterfaces()
                     .WithScopedLifetime());
            
            services.AddHangfire(configuration => configuration
             .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
             .UseSimpleAssemblyNameTypeSerializer()
             .UseRecommendedSerializerSettings()
             .UseSqlServerStorage(Configuration.GetConnectionString("DataContext"), new SqlServerStorageOptions
             {
                 SlidingInvisibilityTimeout = TimeSpan.FromMinutes(2),
                 QueuePollInterval = TimeSpan.FromSeconds(10),
                 CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                 UseRecommendedIsolationLevel = true,
                 UsePageLocksOnDequeue = true,
                 DisableGlobalLocks = true
             }));
            services.AddHangfireServer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["Token"];
                        return Task.CompletedTask;
                    }
                };
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKeyResolver = (token, secutiryToken, kid, validationParameters) =>
                    {
                        string PublicRSAKeyBase64 = Configuration["Config:PublicRSAKey"];
                        byte[] PublicRSAKeyBytes = Convert.FromBase64String(PublicRSAKeyBase64);
                        string PublicRSAKey = Encoding.Default.GetString(PublicRSAKeyBytes);

                        RSAParameters rsaParams;
                        using (var tr = new StringReader(PublicRSAKey))
                        {
                            var pemReader = new PemReader(tr);
                            var publicRsaParams = pemReader.ReadObject() as RsaKeyParameters;
                            if (publicRsaParams == null)
                            {
                                throw new Exception("Could not read RSA public key");
                            }
                            rsaParams = DotNetUtilities.ToRSAParameters(publicRsaParams);
                        }

                        RSA rsa = RSA.Create();
                        rsa.ImportParameters(rsaParams);

                        SecurityKey RSASecurityKey = new RsaSecurityKey(rsa);
                        return new List<SecurityKey> { RSASecurityKey };
                    }
                };
            });

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Permission", policy =>
                    policy.Requirements.Add(new PermissionRequirement()));
            });

            services.AddScoped<IAuthorizationHandler, SimpleHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Simple", policy =>
                    policy.Requirements.Add(new SimpleRequirement()));
            });

            Action onChange = () =>
            {
                JobStorage.Current = new SqlServerStorage(Configuration.GetConnectionString("DataContext"));
                using (var connection = JobStorage.Current.GetConnection())
                {
                    foreach (var recurringJob in connection.GetRecurringJobs())
                    {
                        RecurringJob.RemoveIfExists(recurringJob.Id);
                    }
                }

                RecurringJob.AddOrUpdate<MaintenanceService>("CleanHangfire", x => x.CleanHangfire(), Cron.Daily);
            };
            onChange();
            ChangeToken.OnChange(() => Configuration.GetReloadToken(), onChange);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "rpc/rideSharing/swagger/{documentname}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/rpc/rideSharing/swagger/v1/swagger.json", "rideSharing API");
                c.RoutePrefix = "rpc/rideSharing/swagger";
            });
            app.UseDeveloperExceptionPage();
            app.UseHangfireDashboard("/rpc/rideSharing/hangfire");
            
        }
    }
}
