using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RideSharing;
using RideSharing.Common;
using RideSharing.Enums;
using RideSharing.Entities;
using RideSharing.Repositories;

namespace RideSharing.Services.MDeliveryOrder
{
    public interface IDeliveryOrderValidator : IServiceScoped
    {
        Task Get(DeliveryOrder DeliveryOrder);
        Task<bool> Create(DeliveryOrder DeliveryOrder);
        Task<bool> Update(DeliveryOrder DeliveryOrder);
        Task<bool> Delete(DeliveryOrder DeliveryOrder);
        Task<bool> BulkDelete(List<DeliveryOrder> DeliveryOrders);
        Task<bool> Import(List<DeliveryOrder> DeliveryOrders);
    }

    public class DeliveryOrderValidator : IDeliveryOrderValidator
    {
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private DeliveryOrderMessage DeliveryOrderMessage;

        public DeliveryOrderValidator(IUOW UOW, ICurrentContext CurrentContext)
        {
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.DeliveryOrderMessage = new DeliveryOrderMessage();
        }

        public async Task Get(DeliveryOrder DeliveryOrder)
        {
        }

        public async Task<bool> Create(DeliveryOrder DeliveryOrder)
        {
            await ValidateCode(DeliveryOrder);
            await ValidateWeight(DeliveryOrder);
            await ValidateCustomer(DeliveryOrder);
            return DeliveryOrder.IsValidated;
        }

        public async Task<bool> Update(DeliveryOrder DeliveryOrder)
        {
            if (await ValidateId(DeliveryOrder))
            {
                await ValidateCode(DeliveryOrder);
                await ValidateWeight(DeliveryOrder);
                await ValidateCustomer(DeliveryOrder);
            }
            return DeliveryOrder.IsValidated;
        }

        public async Task<bool> Delete(DeliveryOrder DeliveryOrder)
        {
            var oldData = await UOW.DeliveryOrderRepository.Get(DeliveryOrder.Id);
            if (oldData != null)
            {
            }
            else
            {
                DeliveryOrder.AddError(nameof(DeliveryOrderValidator), nameof(DeliveryOrder.Id), DeliveryOrderMessage.Error.IdNotExisted, DeliveryOrderMessage);
            }
            return DeliveryOrder.IsValidated;
        }
        
        public async Task<bool> BulkDelete(List<DeliveryOrder> DeliveryOrders)
        {
            return DeliveryOrders.All(x => x.IsValidated);
        }

        public async Task<bool> Import(List<DeliveryOrder> DeliveryOrders)
        {
            return true;
        }
        
        private async Task<bool> ValidateId(DeliveryOrder DeliveryOrder)
        {
            DeliveryOrderFilter DeliveryOrderFilter = new DeliveryOrderFilter
            {
                Skip = 0,
                Take = 10,
                Id = new IdFilter { Equal = DeliveryOrder.Id },
                Selects = DeliveryOrderSelect.Id
            };

            int count = await UOW.DeliveryOrderRepository.CountAll(DeliveryOrderFilter);
            if (count == 0)
                DeliveryOrder.AddError(nameof(DeliveryOrderValidator), nameof(DeliveryOrder.Id), DeliveryOrderMessage.Error.IdNotExisted, DeliveryOrderMessage);
            return DeliveryOrder.IsValidated;
        }

        private async Task<bool> ValidateCode(DeliveryOrder DeliveryOrder)
        {
            if(string.IsNullOrEmpty(DeliveryOrder.Code))
            {
                DeliveryOrder.AddError(nameof(DeliveryOrderValidator), nameof(DeliveryOrder.Code), DeliveryOrderMessage.Error.CodeEmpty, DeliveryOrderMessage);
            }
            else if(DeliveryOrder.Code.Count() > 500)
            {
                DeliveryOrder.AddError(nameof(DeliveryOrderValidator), nameof(DeliveryOrder.Code), DeliveryOrderMessage.Error.CodeOverLength, DeliveryOrderMessage);
            }
            return DeliveryOrder.IsValidated;
        }
        private async Task<bool> ValidateWeight(DeliveryOrder DeliveryOrder)
        {   
            return true;
        }
        private async Task<bool> ValidateCustomer(DeliveryOrder DeliveryOrder)
        {       
            if(DeliveryOrder.CustomerId == 0)
            {
                DeliveryOrder.AddError(nameof(DeliveryOrderValidator), nameof(DeliveryOrder.Customer), DeliveryOrderMessage.Error.CustomerEmpty, DeliveryOrderMessage);
            }
            else
            {
                int count = await UOW.CustomerRepository.CountAll(new CustomerFilter
                {
                    Id = new IdFilter{ Equal =  DeliveryOrder.CustomerId },
                });
                if(count == 0)
                {
                    DeliveryOrder.AddError(nameof(DeliveryOrderValidator), nameof(DeliveryOrder.Customer), DeliveryOrderMessage.Error.CustomerNotExisted, DeliveryOrderMessage);
                }
            }
            return true;
        }
    }
}
