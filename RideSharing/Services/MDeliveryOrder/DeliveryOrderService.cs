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

namespace RideSharing.Services.MDeliveryOrder
{
    public interface IDeliveryOrderService :  IServiceScoped
    {
        Task<int> Count(DeliveryOrderFilter DeliveryOrderFilter);
        Task<List<DeliveryOrder>> List(DeliveryOrderFilter DeliveryOrderFilter);
        Task<DeliveryOrder> Get(long Id);
        Task<DeliveryOrder> Create(DeliveryOrder DeliveryOrder);
        Task<DeliveryOrder> Update(DeliveryOrder DeliveryOrder);
        Task<DeliveryOrder> Delete(DeliveryOrder DeliveryOrder);
        Task<List<DeliveryOrder>> BulkDelete(List<DeliveryOrder> DeliveryOrders);
        Task<List<DeliveryOrder>> BulkMerge(List<DeliveryOrder> DeliveryOrders);
        Task<DeliveryOrderFilter> ToFilter(DeliveryOrderFilter DeliveryOrderFilter);
    }

    public class DeliveryOrderService : BaseService, IDeliveryOrderService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ILogging Logging;
        private ICurrentContext CurrentContext;
        
        private IDeliveryOrderValidator DeliveryOrderValidator;

        public DeliveryOrderService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            IDeliveryOrderValidator DeliveryOrderValidator,
            ILogging Logging
        )
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.Logging = Logging;
           
            this.DeliveryOrderValidator = DeliveryOrderValidator;
        }

        public async Task<int> Count(DeliveryOrderFilter DeliveryOrderFilter)
        {
            try
            {
                int result = await UOW.DeliveryOrderRepository.Count(DeliveryOrderFilter);
                return result;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryOrderService));
            }
            return 0;
        }

        public async Task<List<DeliveryOrder>> List(DeliveryOrderFilter DeliveryOrderFilter)
        {
            try
            {
                List<DeliveryOrder> DeliveryOrders = await UOW.DeliveryOrderRepository.List(DeliveryOrderFilter);
                return DeliveryOrders;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryOrderService));
            }
            return null;
        }

        public async Task<DeliveryOrder> Get(long Id)
        {
            DeliveryOrder DeliveryOrder = await UOW.DeliveryOrderRepository.Get(Id);
            if (DeliveryOrder == null)
                return null;
            await DeliveryOrderValidator.Get(DeliveryOrder);
            return DeliveryOrder;
        }
        
        public async Task<DeliveryOrder> Create(DeliveryOrder DeliveryOrder)
        {
            if (!await DeliveryOrderValidator.Create(DeliveryOrder))
                return DeliveryOrder;

            try
            {
                await UOW.DeliveryOrderRepository.Create(DeliveryOrder);
                DeliveryOrder = await UOW.DeliveryOrderRepository.Get(DeliveryOrder.Id);
                Sync(new List<DeliveryOrder> { DeliveryOrder });
                return DeliveryOrder;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryOrderService));
            }
            return null;
        }

        public async Task<DeliveryOrder> Update(DeliveryOrder DeliveryOrder)
        {
            if (!await DeliveryOrderValidator.Update(DeliveryOrder))
                return DeliveryOrder;
            try
            {
                var oldData = await UOW.DeliveryOrderRepository.Get(DeliveryOrder.Id);

                await UOW.DeliveryOrderRepository.Update(DeliveryOrder);

                DeliveryOrder = await UOW.DeliveryOrderRepository.Get(DeliveryOrder.Id);
                Sync(new List<DeliveryOrder> { DeliveryOrder });
                return DeliveryOrder;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryOrderService));
            }
            return null;
        }

        public async Task<DeliveryOrder> Delete(DeliveryOrder DeliveryOrder)
        {
            if (!await DeliveryOrderValidator.Delete(DeliveryOrder))
                return DeliveryOrder;

            try
            {
                await UOW.DeliveryOrderRepository.Delete(DeliveryOrder);
                return DeliveryOrder;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryOrderService));
            }
            return null;
        }

        public async Task<List<DeliveryOrder>> BulkDelete(List<DeliveryOrder> DeliveryOrders)
        {
            if (!await DeliveryOrderValidator.BulkDelete(DeliveryOrders))
                return DeliveryOrders;

            try
            {
                await UOW.DeliveryOrderRepository.BulkDelete(DeliveryOrders);
                return DeliveryOrders;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryOrderService));
            }
            return null;
        }

        public async Task<List<DeliveryOrder>> BulkMerge(List<DeliveryOrder> DeliveryOrders)
        {
            if (!await DeliveryOrderValidator.Import(DeliveryOrders))
                return DeliveryOrders;
            try
            {
                var Ids = await UOW.DeliveryOrderRepository.BulkMerge(DeliveryOrders);
                DeliveryOrders = await UOW.DeliveryOrderRepository.List(Ids);
                Sync(DeliveryOrders);
                return DeliveryOrders;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(DeliveryOrderService));
            }
            return null;
        }     
        
        public async Task<DeliveryOrderFilter> ToFilter(DeliveryOrderFilter filter)
        {
            if (filter.OrFilter == null) filter.OrFilter = new List<DeliveryOrderFilter>();
            if (CurrentContext.Filters == null || CurrentContext.Filters.Count == 0) return filter;
            foreach (var currentFilter in CurrentContext.Filters)
            {
                DeliveryOrderFilter subFilter = new DeliveryOrderFilter();
                filter.OrFilter.Add(subFilter);
                List<FilterPermissionDefinition> FilterPermissionDefinitions = currentFilter.Value;
                foreach (FilterPermissionDefinition FilterPermissionDefinition in FilterPermissionDefinitions)
                {
                    if (FilterPermissionDefinition.Name == "DeliveryOrderId")
                        subFilter.Id = FilterBuilder.Merge(subFilter.Id, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Code))
                        subFilter.Code = FilterBuilder.Merge(subFilter.Code, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Weight))
                        subFilter.Weight = FilterBuilder.Merge(subFilter.Weight, FilterPermissionDefinition.DecimalFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.CustomerId))
                        subFilter.CustomerId = FilterBuilder.Merge(subFilter.CustomerId, FilterPermissionDefinition.IdFilter);
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

        private void Sync(List<DeliveryOrder> DeliveryOrders)
        {


            
        }
    }
}
