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

namespace RideSharing.Services.MDeliveryTrip
{
    public interface IDeliveryTripValidator : IServiceScoped
    {
        Task Get(DeliveryTrip DeliveryTrip);
        Task<bool> Create(DeliveryTrip DeliveryTrip);
        Task<bool> Update(DeliveryTrip DeliveryTrip);
        Task<bool> Delete(DeliveryTrip DeliveryTrip);
        Task<bool> BulkDelete(List<DeliveryTrip> DeliveryTrips);
        Task<bool> Import(List<DeliveryTrip> DeliveryTrips);
    }

    public class DeliveryTripValidator : IDeliveryTripValidator
    {
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private DeliveryTripMessage DeliveryTripMessage;

        public DeliveryTripValidator(IUOW UOW, ICurrentContext CurrentContext)
        {
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.DeliveryTripMessage = new DeliveryTripMessage();
        }

        public async Task Get(DeliveryTrip DeliveryTrip)
        {
        }

        public async Task<bool> Create(DeliveryTrip DeliveryTrip)
        {
            await ValidatePath(DeliveryTrip);
            await ValidateTravelDistance(DeliveryTrip);
            await ValidateBusStop(DeliveryTrip);
            await ValidateCityFreighter(DeliveryTrip);
            return DeliveryTrip.IsValidated;
        }

        public async Task<bool> Update(DeliveryTrip DeliveryTrip)
        {
            if (await ValidateId(DeliveryTrip))
            {
                await ValidatePath(DeliveryTrip);
                await ValidateTravelDistance(DeliveryTrip);
                await ValidateBusStop(DeliveryTrip);
                await ValidateCityFreighter(DeliveryTrip);
            }
            return DeliveryTrip.IsValidated;
        }

        public async Task<bool> Delete(DeliveryTrip DeliveryTrip)
        {
            var oldData = await UOW.DeliveryTripRepository.Get(DeliveryTrip.Id);
            if (oldData != null)
            {
            }
            else
            {
                DeliveryTrip.AddError(nameof(DeliveryTripValidator), nameof(DeliveryTrip.Id), DeliveryTripMessage.Error.IdNotExisted, DeliveryTripMessage);
            }
            return DeliveryTrip.IsValidated;
        }
        
        public async Task<bool> BulkDelete(List<DeliveryTrip> DeliveryTrips)
        {
            return DeliveryTrips.All(x => x.IsValidated);
        }

        public async Task<bool> Import(List<DeliveryTrip> DeliveryTrips)
        {
            return true;
        }
        
        private async Task<bool> ValidateId(DeliveryTrip DeliveryTrip)
        {
            DeliveryTripFilter DeliveryTripFilter = new DeliveryTripFilter
            {
                Skip = 0,
                Take = 10,
                Id = new IdFilter { Equal = DeliveryTrip.Id },
                Selects = DeliveryTripSelect.Id
            };

            int count = await UOW.DeliveryTripRepository.CountAll(DeliveryTripFilter);
            if (count == 0)
                DeliveryTrip.AddError(nameof(DeliveryTripValidator), nameof(DeliveryTrip.Id), DeliveryTripMessage.Error.IdNotExisted, DeliveryTripMessage);
            return DeliveryTrip.IsValidated;
        }

        private async Task<bool> ValidatePath(DeliveryTrip DeliveryTrip)
        {
            if(string.IsNullOrEmpty(DeliveryTrip.Path))
            {
                DeliveryTrip.AddError(nameof(DeliveryTripValidator), nameof(DeliveryTrip.Path), DeliveryTripMessage.Error.PathEmpty, DeliveryTripMessage);
            }
            else if(DeliveryTrip.Path.Count() > 500)
            {
                DeliveryTrip.AddError(nameof(DeliveryTripValidator), nameof(DeliveryTrip.Path), DeliveryTripMessage.Error.PathOverLength, DeliveryTripMessage);
            }
            return DeliveryTrip.IsValidated;
        }
        private async Task<bool> ValidateTravelDistance(DeliveryTrip DeliveryTrip)
        {   
            return true;
        }
        private async Task<bool> ValidateBusStop(DeliveryTrip DeliveryTrip)
        {       
            if(DeliveryTrip.BusStopId == 0)
            {
                DeliveryTrip.AddError(nameof(DeliveryTripValidator), nameof(DeliveryTrip.BusStop), DeliveryTripMessage.Error.BusStopEmpty, DeliveryTripMessage);
            }
            else
            {
                int count = await UOW.BusStopRepository.CountAll(new BusStopFilter
                {
                    Id = new IdFilter{ Equal =  DeliveryTrip.BusStopId },
                });
                if(count == 0)
                {
                    DeliveryTrip.AddError(nameof(DeliveryTripValidator), nameof(DeliveryTrip.BusStop), DeliveryTripMessage.Error.BusStopNotExisted, DeliveryTripMessage);
                }
            }
            return true;
        }
        private async Task<bool> ValidateCityFreighter(DeliveryTrip DeliveryTrip)
        {       
            if(DeliveryTrip.CityFreighterId == 0)
            {
                DeliveryTrip.AddError(nameof(DeliveryTripValidator), nameof(DeliveryTrip.CityFreighter), DeliveryTripMessage.Error.CityFreighterEmpty, DeliveryTripMessage);
            }
            else
            {
                int count = await UOW.CityFreighterRepository.CountAll(new CityFreighterFilter
                {
                    Id = new IdFilter{ Equal =  DeliveryTrip.CityFreighterId },
                });
                if(count == 0)
                {
                    DeliveryTrip.AddError(nameof(DeliveryTripValidator), nameof(DeliveryTrip.CityFreighter), DeliveryTripMessage.Error.CityFreighterNotExisted, DeliveryTripMessage);
                }
            }
            return true;
        }
    }
}
