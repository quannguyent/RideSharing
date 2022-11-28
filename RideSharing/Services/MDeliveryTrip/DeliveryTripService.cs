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

namespace RideSharing.Services.MDeliveryTrip
{
    public interface IDeliveryTripService :  IServiceScoped
    {
        Task<int> Count(DeliveryTripFilter DeliveryTripFilter);
        Task<List<DeliveryTrip>> List(DeliveryTripFilter DeliveryTripFilter);
        Task<DeliveryTrip> Get(long Id);
        Task<DeliveryTrip> Create(DeliveryTrip DeliveryTrip);
        Task<DeliveryTrip> Update(DeliveryTrip DeliveryTrip);
        Task<DeliveryTrip> Delete(DeliveryTrip DeliveryTrip);
        Task<List<DeliveryTrip>> BulkDelete(List<DeliveryTrip> DeliveryTrips);
        Task<List<DeliveryTrip>> BulkMerge(List<DeliveryTrip> DeliveryTrips);
        Task<DeliveryTripFilter> ToFilter(DeliveryTripFilter DeliveryTripFilter);
    }

    public class DeliveryTripService : BaseService, IDeliveryTripService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ILogging Logging;
        private ICurrentContext CurrentContext;
        
        private IDeliveryTripValidator DeliveryTripValidator;

        public DeliveryTripService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            IDeliveryTripValidator DeliveryTripValidator,
            ILogging Logging
        )
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.Logging = Logging;
           
            this.DeliveryTripValidator = DeliveryTripValidator;
        }

        public async Task<int> Count(DeliveryTripFilter DeliveryTripFilter)
        {
            try
            {
                int result = await UOW.DeliveryTripRepository.Count(DeliveryTripFilter);
                return result;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryTripService));
            }
            return 0;
        }

        public async Task<List<DeliveryTrip>> List(DeliveryTripFilter DeliveryTripFilter)
        {
            try
            {
                List<DeliveryTrip> DeliveryTrips = await UOW.DeliveryTripRepository.List(DeliveryTripFilter);
                return DeliveryTrips;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryTripService));
            }
            return null;
        }

        public async Task<DeliveryTrip> Get(long Id)
        {
            DeliveryTrip DeliveryTrip = await UOW.DeliveryTripRepository.Get(Id);
            if (DeliveryTrip == null)
                return null;
            await DeliveryTripValidator.Get(DeliveryTrip);
            return DeliveryTrip;
        }
        
        public async Task<DeliveryTrip> Create(DeliveryTrip DeliveryTrip)
        {
            if (!await DeliveryTripValidator.Create(DeliveryTrip))
                return DeliveryTrip;

            try
            {
                await UOW.DeliveryTripRepository.Create(DeliveryTrip);
                DeliveryTrip = await UOW.DeliveryTripRepository.Get(DeliveryTrip.Id);
                Sync(new List<DeliveryTrip> { DeliveryTrip });
                return DeliveryTrip;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryTripService));
            }
            return null;
        }

        public async Task<DeliveryTrip> Update(DeliveryTrip DeliveryTrip)
        {
            if (!await DeliveryTripValidator.Update(DeliveryTrip))
                return DeliveryTrip;
            try
            {
                var oldData = await UOW.DeliveryTripRepository.Get(DeliveryTrip.Id);

                await UOW.DeliveryTripRepository.Update(DeliveryTrip);

                DeliveryTrip = await UOW.DeliveryTripRepository.Get(DeliveryTrip.Id);
                Sync(new List<DeliveryTrip> { DeliveryTrip });
                return DeliveryTrip;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryTripService));
            }
            return null;
        }

        public async Task<DeliveryTrip> Delete(DeliveryTrip DeliveryTrip)
        {
            if (!await DeliveryTripValidator.Delete(DeliveryTrip))
                return DeliveryTrip;

            try
            {
                await UOW.DeliveryTripRepository.Delete(DeliveryTrip);
                return DeliveryTrip;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryTripService));
            }
            return null;
        }

        public async Task<List<DeliveryTrip>> BulkDelete(List<DeliveryTrip> DeliveryTrips)
        {
            if (!await DeliveryTripValidator.BulkDelete(DeliveryTrips))
                return DeliveryTrips;

            try
            {
                await UOW.DeliveryTripRepository.BulkDelete(DeliveryTrips);
                return DeliveryTrips;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryTripService));
            }
            return null;
        }

        public async Task<List<DeliveryTrip>> BulkMerge(List<DeliveryTrip> DeliveryTrips)
        {
            if (!await DeliveryTripValidator.Import(DeliveryTrips))
                return DeliveryTrips;
            try
            {
                var Ids = await UOW.DeliveryTripRepository.BulkMerge(DeliveryTrips);
                DeliveryTrips = await UOW.DeliveryTripRepository.List(Ids);
                Sync(DeliveryTrips);
                return DeliveryTrips;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryTripService));
            }
            return null;
        }     
        
        public async Task<DeliveryTripFilter> ToFilter(DeliveryTripFilter filter)
        {
            if (filter.OrFilter == null) filter.OrFilter = new List<DeliveryTripFilter>();
            if (CurrentContext.Filters == null || CurrentContext.Filters.Count == 0) return filter;
            foreach (var currentFilter in CurrentContext.Filters)
            {
                DeliveryTripFilter subFilter = new DeliveryTripFilter();
                filter.OrFilter.Add(subFilter);
                List<FilterPermissionDefinition> FilterPermissionDefinitions = currentFilter.Value;
                foreach (FilterPermissionDefinition FilterPermissionDefinition in FilterPermissionDefinitions)
                {
                    if (FilterPermissionDefinition.Name == "DeliveryTripId")
                        subFilter.Id = FilterBuilder.Merge(subFilter.Id, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Path))
                        subFilter.Path = FilterBuilder.Merge(subFilter.Path, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.CityFreighterId))
                        subFilter.CityFreighterId = FilterBuilder.Merge(subFilter.CityFreighterId, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.BusStopId))
                        subFilter.BusStopId = FilterBuilder.Merge(subFilter.BusStopId, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.TravelDistance))
                        subFilter.TravelDistance = FilterBuilder.Merge(subFilter.TravelDistance, FilterPermissionDefinition.DecimalFilter);
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

        private void Sync(List<DeliveryTrip> DeliveryTrips)
        {


            
        }
    }
}
