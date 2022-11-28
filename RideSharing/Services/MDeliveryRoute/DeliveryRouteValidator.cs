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

namespace RideSharing.Services.MDeliveryRoute
{
    public interface IDeliveryRouteValidator : IServiceScoped
    {
        Task Get(DeliveryRoute DeliveryRoute);
        Task<bool> Create(DeliveryRoute DeliveryRoute);
        Task<bool> Update(DeliveryRoute DeliveryRoute);
        Task<bool> Delete(DeliveryRoute DeliveryRoute);
        Task<bool> BulkDelete(List<DeliveryRoute> DeliveryRoutes);
        Task<bool> Import(List<DeliveryRoute> DeliveryRoutes);
    }

    public class DeliveryRouteValidator : IDeliveryRouteValidator
    {
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private DeliveryRouteMessage DeliveryRouteMessage;

        public DeliveryRouteValidator(IUOW UOW, ICurrentContext CurrentContext)
        {
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.DeliveryRouteMessage = new DeliveryRouteMessage();
        }

        public async Task Get(DeliveryRoute DeliveryRoute)
        {
        }

        public async Task<bool> Create(DeliveryRoute DeliveryRoute)
        {
            await ValidatePath(DeliveryRoute);
            await ValidateTotalTravelDistance(DeliveryRoute);
            await ValidateTotalEmptyRunDistance(DeliveryRoute);
            await ValidateCityFreighter(DeliveryRoute);
            return DeliveryRoute.IsValidated;
        }

        public async Task<bool> Update(DeliveryRoute DeliveryRoute)
        {
            if (await ValidateId(DeliveryRoute))
            {
                await ValidatePath(DeliveryRoute);
                await ValidateTotalTravelDistance(DeliveryRoute);
                await ValidateTotalEmptyRunDistance(DeliveryRoute);
                await ValidateCityFreighter(DeliveryRoute);
            }
            return DeliveryRoute.IsValidated;
        }

        public async Task<bool> Delete(DeliveryRoute DeliveryRoute)
        {
            var oldData = await UOW.DeliveryRouteRepository.Get(DeliveryRoute.Id);
            if (oldData != null)
            {
            }
            else
            {
                DeliveryRoute.AddError(nameof(DeliveryRouteValidator), nameof(DeliveryRoute.Id), DeliveryRouteMessage.Error.IdNotExisted, DeliveryRouteMessage);
            }
            return DeliveryRoute.IsValidated;
        }
        
        public async Task<bool> BulkDelete(List<DeliveryRoute> DeliveryRoutes)
        {
            return DeliveryRoutes.All(x => x.IsValidated);
        }

        public async Task<bool> Import(List<DeliveryRoute> DeliveryRoutes)
        {
            return true;
        }
        
        private async Task<bool> ValidateId(DeliveryRoute DeliveryRoute)
        {
            DeliveryRouteFilter DeliveryRouteFilter = new DeliveryRouteFilter
            {
                Skip = 0,
                Take = 10,
                Id = new IdFilter { Equal = DeliveryRoute.Id },
                Selects = DeliveryRouteSelect.Id
            };

            int count = await UOW.DeliveryRouteRepository.CountAll(DeliveryRouteFilter);
            if (count == 0)
                DeliveryRoute.AddError(nameof(DeliveryRouteValidator), nameof(DeliveryRoute.Id), DeliveryRouteMessage.Error.IdNotExisted, DeliveryRouteMessage);
            return DeliveryRoute.IsValidated;
        }

        private async Task<bool> ValidatePath(DeliveryRoute DeliveryRoute)
        {
            if(string.IsNullOrEmpty(DeliveryRoute.Path))
            {
                DeliveryRoute.AddError(nameof(DeliveryRouteValidator), nameof(DeliveryRoute.Path), DeliveryRouteMessage.Error.PathEmpty, DeliveryRouteMessage);
            }
            else if(DeliveryRoute.Path.Count() > 500)
            {
                DeliveryRoute.AddError(nameof(DeliveryRouteValidator), nameof(DeliveryRoute.Path), DeliveryRouteMessage.Error.PathOverLength, DeliveryRouteMessage);
            }
            return DeliveryRoute.IsValidated;
        }
        private async Task<bool> ValidateTotalTravelDistance(DeliveryRoute DeliveryRoute)
        {   
            return true;
        }
        private async Task<bool> ValidateTotalEmptyRunDistance(DeliveryRoute DeliveryRoute)
        {   
            return true;
        }
        private async Task<bool> ValidateCityFreighter(DeliveryRoute DeliveryRoute)
        {       
            if(DeliveryRoute.CityFreighterId == 0)
            {
                DeliveryRoute.AddError(nameof(DeliveryRouteValidator), nameof(DeliveryRoute.CityFreighter), DeliveryRouteMessage.Error.CityFreighterEmpty, DeliveryRouteMessage);
            }
            else
            {
                int count = await UOW.CityFreighterRepository.CountAll(new CityFreighterFilter
                {
                    Id = new IdFilter{ Equal =  DeliveryRoute.CityFreighterId },
                });
                if(count == 0)
                {
                    DeliveryRoute.AddError(nameof(DeliveryRouteValidator), nameof(DeliveryRoute.CityFreighter), DeliveryRouteMessage.Error.CityFreighterNotExisted, DeliveryRouteMessage);
                }
            }
            return true;
        }
    }
}
