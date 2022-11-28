using TrueSight.Common;
using RideSharing.Common;
using RideSharing.Models;
using RideSharing.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideSharing.Services
{
    public interface IMaintenanceService : IServiceScoped
    {
        Task CleanHangfire();
    }
    public class MaintenanceService : IMaintenanceService
    {
        private DataContext DataContext;
        public MaintenanceService(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        public async Task CleanHangfire()
        {
            var commandText = @"
                TRUNCATE TABLE [HangFire].[AggregatedCounter]
                TRUNCATE TABLE[HangFire].[Counter]
                TRUNCATE TABLE[HangFire].[JobParameter]
                TRUNCATE TABLE[HangFire].[JobQueue]
                TRUNCATE TABLE[HangFire].[List]
                TRUNCATE TABLE[HangFire].[State]
                DELETE FROM[HangFire].[Job]
                DBCC CHECKIDENT('[HangFire].[Job]', reseed, 0)
                UPDATE[HangFire].[Hash] SET Value = 1 WHERE Field = 'LastJobId'";
            var result = await DataContext.Database.ExecuteSqlRawAsync(commandText);
        }
    }
}
