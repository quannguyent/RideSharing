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

namespace RideSharing.Services.MBusStop
{
    public interface IBusStopService :  IServiceScoped
    {
        Task<int> Count(BusStopFilter BusStopFilter);
        Task<List<BusStop>> List(BusStopFilter BusStopFilter);
        Task<BusStop> Get(long Id);
        Task<BusStop> Create(BusStop BusStop);
        Task<BusStop> Update(BusStop BusStop);
        Task<BusStop> Delete(BusStop BusStop);
        Task<List<BusStop>> BulkDelete(List<BusStop> BusStops);
        Task<List<BusStop>> BulkMerge(List<BusStop> BusStops);
        Task<BusStopFilter> ToFilter(BusStopFilter BusStopFilter);
    }

    public class BusStopService : BaseService, IBusStopService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ILogging Logging;
        private ICurrentContext CurrentContext;
        
        private IBusStopValidator BusStopValidator;

        public BusStopService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            IBusStopValidator BusStopValidator,
            ILogging Logging
        )
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.Logging = Logging;
           
            this.BusStopValidator = BusStopValidator;
        }

        public async Task<int> Count(BusStopFilter BusStopFilter)
        {
            try
            {
                int result = await UOW.BusStopRepository.Count(BusStopFilter);
                return result;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(BusStopService));
            }
            return 0;
        }

        public async Task<List<BusStop>> List(BusStopFilter BusStopFilter)
        {
            try
            {
                List<BusStop> BusStops = await UOW.BusStopRepository.List(BusStopFilter);
                return BusStops;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(BusStopService));
            }
            return null;
        }

        public async Task<BusStop> Get(long Id)
        {
            BusStop BusStop = await UOW.BusStopRepository.Get(Id);
            if (BusStop == null)
                return null;
            await BusStopValidator.Get(BusStop);
            return BusStop;
        }
        
        public async Task<BusStop> Create(BusStop BusStop)
        {
            if (!await BusStopValidator.Create(BusStop))
                return BusStop;

            try
            {
                await UOW.BusStopRepository.Create(BusStop);
                BusStop = await UOW.BusStopRepository.Get(BusStop.Id);
                Sync(new List<BusStop> { BusStop });
                return BusStop;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(BusStopService));
            }
            return null;
        }

        public async Task<BusStop> Update(BusStop BusStop)
        {
            if (!await BusStopValidator.Update(BusStop))
                return BusStop;
            try
            {
                var oldData = await UOW.BusStopRepository.Get(BusStop.Id);

                await UOW.BusStopRepository.Update(BusStop);

                BusStop = await UOW.BusStopRepository.Get(BusStop.Id);
                Sync(new List<BusStop> { BusStop });
                return BusStop;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(BusStopService));
            }
            return null;
        }

        public async Task<BusStop> Delete(BusStop BusStop)
        {
            if (!await BusStopValidator.Delete(BusStop))
                return BusStop;

            try
            {
                await UOW.BusStopRepository.Delete(BusStop);
                return BusStop;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(BusStopService));
            }
            return null;
        }

        public async Task<List<BusStop>> BulkDelete(List<BusStop> BusStops)
        {
            if (!await BusStopValidator.BulkDelete(BusStops))
                return BusStops;

            try
            {
                await UOW.BusStopRepository.BulkDelete(BusStops);
                return BusStops;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(BusStopService));
            }
            return null;
        }

        public async Task<List<BusStop>> BulkMerge(List<BusStop> BusStops)
        {
            if (!await BusStopValidator.Import(BusStops))
                return BusStops;
            try
            {
                var Ids = await UOW.BusStopRepository.BulkMerge(BusStops);
                BusStops = await UOW.BusStopRepository.List(Ids);
                Sync(BusStops);
                return BusStops;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(BusStopService));
            }
            return null;
        }     
        
        public async Task<BusStopFilter> ToFilter(BusStopFilter filter)
        {
            if (filter.OrFilter == null) filter.OrFilter = new List<BusStopFilter>();
            if (CurrentContext.Filters == null || CurrentContext.Filters.Count == 0) return filter;
            foreach (var currentFilter in CurrentContext.Filters)
            {
                BusStopFilter subFilter = new BusStopFilter();
                filter.OrFilter.Add(subFilter);
                List<FilterPermissionDefinition> FilterPermissionDefinitions = currentFilter.Value;
                foreach (FilterPermissionDefinition FilterPermissionDefinition in FilterPermissionDefinitions)
                {
                    if (FilterPermissionDefinition.Name == "BusStopId")
                        subFilter.Id = FilterBuilder.Merge(subFilter.Id, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Name))
                        subFilter.Name = FilterBuilder.Merge(subFilter.Name, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.NodeId))
                        subFilter.NodeId = FilterBuilder.Merge(subFilter.NodeId, FilterPermissionDefinition.IdFilter);
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

        private void Sync(List<BusStop> BusStops)
        {


            
        }
    }
}
