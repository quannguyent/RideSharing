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

namespace RideSharing.Services.MDeliveryRoute
{
    public interface IDeliveryRouteService :  IServiceScoped
    {
        Task<int> Count(DeliveryRouteFilter DeliveryRouteFilter);
        Task<List<DeliveryRoute>> List(DeliveryRouteFilter DeliveryRouteFilter);
        Task<DeliveryRoute> Get(long Id);
        Task<DeliveryRoute> Create(DeliveryRoute DeliveryRoute);
        Task<DeliveryRoute> Update(DeliveryRoute DeliveryRoute);
        Task<DeliveryRoute> Delete(DeliveryRoute DeliveryRoute);
        Task<List<DeliveryRoute>> BulkDelete(List<DeliveryRoute> DeliveryRoutes);
        Task<List<DeliveryRoute>> BulkMerge(List<DeliveryRoute> DeliveryRoutes);
        Task<DeliveryRouteFilter> ToFilter(DeliveryRouteFilter DeliveryRouteFilter);
    }

    public class DeliveryRouteService : BaseService, IDeliveryRouteService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ILogging Logging;
        private ICurrentContext CurrentContext;
        
        private IDeliveryRouteValidator DeliveryRouteValidator;

        public DeliveryRouteService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            IDeliveryRouteValidator DeliveryRouteValidator,
            ILogging Logging
        )
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.Logging = Logging;
           
            this.DeliveryRouteValidator = DeliveryRouteValidator;
        }

        public async Task<int> Count(DeliveryRouteFilter DeliveryRouteFilter)
        {
            try
            {
                int result = await UOW.DeliveryRouteRepository.Count(DeliveryRouteFilter);
                return result;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryRouteService));
            }
            return 0;
        }

        public async Task<List<DeliveryRoute>> List(DeliveryRouteFilter DeliveryRouteFilter)
        {
            try
            {
                List<DeliveryRoute> DeliveryRoutes = await UOW.DeliveryRouteRepository.List(DeliveryRouteFilter);
                return DeliveryRoutes;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryRouteService));
            }
            return null;
        }

        public async Task<DeliveryRoute> Get(long Id)
        {
            DeliveryRoute DeliveryRoute = await UOW.DeliveryRouteRepository.Get(Id);
            if (DeliveryRoute == null)
                return null;
            await DeliveryRouteValidator.Get(DeliveryRoute);
            return DeliveryRoute;
        }
        
        public async Task<DeliveryRoute> Create(DeliveryRoute DeliveryRoute)
        {
            if (!await DeliveryRouteValidator.Create(DeliveryRoute))
                return DeliveryRoute;

            try
            {
                await UOW.DeliveryRouteRepository.Create(DeliveryRoute);
                DeliveryRoute = await UOW.DeliveryRouteRepository.Get(DeliveryRoute.Id);
                Sync(new List<DeliveryRoute> { DeliveryRoute });
                return DeliveryRoute;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryRouteService));
            }
            return null;
        }

        public async Task<DeliveryRoute> Update(DeliveryRoute DeliveryRoute)
        {
            if (!await DeliveryRouteValidator.Update(DeliveryRoute))
                return DeliveryRoute;
            try
            {
                var oldData = await UOW.DeliveryRouteRepository.Get(DeliveryRoute.Id);

                await UOW.DeliveryRouteRepository.Update(DeliveryRoute);

                DeliveryRoute = await UOW.DeliveryRouteRepository.Get(DeliveryRoute.Id);
                Sync(new List<DeliveryRoute> { DeliveryRoute });
                return DeliveryRoute;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryRouteService));
            }
            return null;
        }

        public async Task<DeliveryRoute> Delete(DeliveryRoute DeliveryRoute)
        {
            if (!await DeliveryRouteValidator.Delete(DeliveryRoute))
                return DeliveryRoute;

            try
            {
                await UOW.DeliveryRouteRepository.Delete(DeliveryRoute);
                return DeliveryRoute;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryRouteService));
            }
            return null;
        }

        public async Task<List<DeliveryRoute>> BulkDelete(List<DeliveryRoute> DeliveryRoutes)
        {
            if (!await DeliveryRouteValidator.BulkDelete(DeliveryRoutes))
                return DeliveryRoutes;

            try
            {
                await UOW.DeliveryRouteRepository.BulkDelete(DeliveryRoutes);
                return DeliveryRoutes;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryRouteService));
            }
            return null;
        }

        public async Task<List<DeliveryRoute>> BulkMerge(List<DeliveryRoute> DeliveryRoutes)
        {
            if (!await DeliveryRouteValidator.Import(DeliveryRoutes))
                return DeliveryRoutes;
            try
            {
                var Ids = await UOW.DeliveryRouteRepository.BulkMerge(DeliveryRoutes);
                DeliveryRoutes = await UOW.DeliveryRouteRepository.List(Ids);
                Sync(DeliveryRoutes);
                return DeliveryRoutes;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryRouteService));
            }
            return null;
        }     
        
        public async Task<DeliveryRouteFilter> ToFilter(DeliveryRouteFilter filter)
        {
            if (filter.OrFilter == null) filter.OrFilter = new List<DeliveryRouteFilter>();
            if (CurrentContext.Filters == null || CurrentContext.Filters.Count == 0) return filter;
            foreach (var currentFilter in CurrentContext.Filters)
            {
                DeliveryRouteFilter subFilter = new DeliveryRouteFilter();
                filter.OrFilter.Add(subFilter);
                List<FilterPermissionDefinition> FilterPermissionDefinitions = currentFilter.Value;
                foreach (FilterPermissionDefinition FilterPermissionDefinition in FilterPermissionDefinitions)
                {
                    if (FilterPermissionDefinition.Name == "DeliveryRouteId")
                        subFilter.Id = FilterBuilder.Merge(subFilter.Id, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Path))
                        subFilter.Path = FilterBuilder.Merge(subFilter.Path, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.CityFreighterId))
                        subFilter.CityFreighterId = FilterBuilder.Merge(subFilter.CityFreighterId, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.TotalTravelDistance))
                        subFilter.TotalTravelDistance = FilterBuilder.Merge(subFilter.TotalTravelDistance, FilterPermissionDefinition.DecimalFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.TotalEmptyRunDistance))
                        subFilter.TotalEmptyRunDistance = FilterBuilder.Merge(subFilter.TotalEmptyRunDistance, FilterPermissionDefinition.DecimalFilter);
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

        private void Sync(List<DeliveryRoute> DeliveryRoutes)
        {


            
        }
    }
}
