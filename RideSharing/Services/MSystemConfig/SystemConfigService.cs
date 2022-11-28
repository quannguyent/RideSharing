using TrueSight.Common;
using RideSharing.Handlers.Configuration;
using RideSharing.Common;
using RideSharing.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using RideSharing.Repositories;
using RideSharing.Entities;
using RideSharing.Enums;

namespace RideSharing.Services.MSystemConfig
{
    public interface ISystemConfigService :  IServiceScoped
    {
        Task<int> Count(SystemConfigFilter SystemConfigFilter);
        Task<List<SystemConfig>> List(SystemConfigFilter SystemConfigFilter);
        Task<SystemConfig> Get(long Id);
        Task<SystemConfig> Create(SystemConfig SystemConfig);
        Task<SystemConfig> Update(SystemConfig SystemConfig);
        Task<SystemConfig> Delete(SystemConfig SystemConfig);
        Task<List<SystemConfig>> BulkDelete(List<SystemConfig> SystemConfigs);
        Task<List<SystemConfig>> BulkMerge(List<SystemConfig> SystemConfigs);
        Task<SystemConfigFilter> ToFilter(SystemConfigFilter SystemConfigFilter);
    }

    public class SystemConfigService : BaseService, ISystemConfigService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ILogging Logging;
        private ICurrentContext CurrentContext;
        
        private ISystemConfigValidator SystemConfigValidator;

        public SystemConfigService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            ISystemConfigValidator SystemConfigValidator,
            ILogging Logging
        )
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.Logging = Logging;
           
            this.SystemConfigValidator = SystemConfigValidator;
        }

        public async Task<int> Count(SystemConfigFilter SystemConfigFilter)
        {
            try
            {
                int result = await UOW.SystemConfigRepository.Count(SystemConfigFilter);
                return result;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(SystemConfigService));
            }
            return 0;
        }

        public async Task<List<SystemConfig>> List(SystemConfigFilter SystemConfigFilter)
        {
            try
            {
                List<SystemConfig> SystemConfigs = await UOW.SystemConfigRepository.List(SystemConfigFilter);
                return SystemConfigs;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(SystemConfigService));
            }
            return null;
        }

        public async Task<SystemConfig> Get(long Id)
        {
            SystemConfig SystemConfig = await UOW.SystemConfigRepository.Get(Id);
            if (SystemConfig == null)
                return null;
            await SystemConfigValidator.Get(SystemConfig);
            return SystemConfig;
        }
        
        public async Task<SystemConfig> Create(SystemConfig SystemConfig)
        {
            if (!await SystemConfigValidator.Create(SystemConfig))
                return SystemConfig;

            try
            {
                await UOW.SystemConfigRepository.Create(SystemConfig);
                SystemConfig = await UOW.SystemConfigRepository.Get(SystemConfig.Id);
                Sync(new List<SystemConfig> { SystemConfig });
                return SystemConfig;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(SystemConfigService));
            }
            return null;
        }

        public async Task<SystemConfig> Update(SystemConfig SystemConfig)
        {
            if (!await SystemConfigValidator.Update(SystemConfig))
                return SystemConfig;
            try
            {
                var oldData = await UOW.SystemConfigRepository.Get(SystemConfig.Id);

                await UOW.SystemConfigRepository.Update(SystemConfig);

                SystemConfig = await UOW.SystemConfigRepository.Get(SystemConfig.Id);
                Sync(new List<SystemConfig> { SystemConfig });
                return SystemConfig;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(SystemConfigService));
            }
            return null;
        }

        public async Task<SystemConfig> Delete(SystemConfig SystemConfig)
        {
            if (!await SystemConfigValidator.Delete(SystemConfig))
                return SystemConfig;

            try
            {
                await UOW.SystemConfigRepository.Delete(SystemConfig);
                return SystemConfig;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(SystemConfigService));
            }
            return null;
        }

        public async Task<List<SystemConfig>> BulkDelete(List<SystemConfig> SystemConfigs)
        {
            if (!await SystemConfigValidator.BulkDelete(SystemConfigs))
                return SystemConfigs;

            try
            {
                await UOW.SystemConfigRepository.BulkDelete(SystemConfigs);
                return SystemConfigs;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(SystemConfigService));
            }
            return null;
        }

        public async Task<List<SystemConfig>> BulkMerge(List<SystemConfig> SystemConfigs)
        {
            if (!await SystemConfigValidator.Import(SystemConfigs))
                return SystemConfigs;
            try
            {
                var Ids = await UOW.SystemConfigRepository.BulkMerge(SystemConfigs);
                SystemConfigs = await UOW.SystemConfigRepository.List(Ids);
                Sync(SystemConfigs);
                return SystemConfigs;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(SystemConfigService));
            }
            return null;
        }     
        
        public async Task<SystemConfigFilter> ToFilter(SystemConfigFilter filter)
        {
            if (filter.OrFilter == null) filter.OrFilter = new List<SystemConfigFilter>();
            if (CurrentContext.Filters == null || CurrentContext.Filters.Count == 0) return filter;
            foreach (var currentFilter in CurrentContext.Filters)
            {
                SystemConfigFilter subFilter = new SystemConfigFilter();
                filter.OrFilter.Add(subFilter);
                List<FilterPermissionDefinition> FilterPermissionDefinitions = currentFilter.Value;
                foreach (FilterPermissionDefinition FilterPermissionDefinition in FilterPermissionDefinitions)
                {
                    if (FilterPermissionDefinition.Name == "SystemConfigId")
                        subFilter.Id = FilterBuilder.Merge(subFilter.Id, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Code))
                        subFilter.Code = FilterBuilder.Merge(subFilter.Code, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.FreighterQuotientCost))
                        subFilter.FreighterQuotientCost = FilterBuilder.Merge(subFilter.FreighterQuotientCost, FilterPermissionDefinition.DecimalFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.DeliveryRadius))
                        subFilter.DeliveryRadius = FilterBuilder.Merge(subFilter.DeliveryRadius, FilterPermissionDefinition.DecimalFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.DeliveryServiceDuration))
                        subFilter.DeliveryServiceDuration = FilterBuilder.Merge(subFilter.DeliveryServiceDuration, FilterPermissionDefinition.LongFilter);
                    if (FilterPermissionDefinition.Name == nameof(CurrentContext.UserId) && FilterPermissionDefinition.IdFilter != null)
                    {
                        if (FilterPermissionDefinition.IdFilter.Equal.HasValue && FilterPermissionDefinition.IdFilter.Equal.Value == CurrentUserEnum.IS.Id)
                        {
                        }
                        if (FilterPermissionDefinition.IdFilter.Equal.HasValue && FilterPermissionDefinition.IdFilter.Equal.Value == CurrentUserEnum.ISNT.Id)
                        {
                        }
                    }
                }
            }
            return filter;
        }

        private void Sync(List<SystemConfig> SystemConfigs)
        {


            
        }
    }
}
