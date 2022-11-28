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

namespace RideSharing.Services.MBusStop
{
    public interface IBusStopValidator : IServiceScoped
    {
        Task Get(BusStop BusStop);
        Task<bool> Create(BusStop BusStop);
        Task<bool> Update(BusStop BusStop);
        Task<bool> Delete(BusStop BusStop);
        Task<bool> BulkDelete(List<BusStop> BusStops);
        Task<bool> Import(List<BusStop> BusStops);
    }

    public class BusStopValidator : IBusStopValidator
    {
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private BusStopMessage BusStopMessage;

        public BusStopValidator(IUOW UOW, ICurrentContext CurrentContext)
        {
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.BusStopMessage = new BusStopMessage();
        }

        public async Task Get(BusStop BusStop)
        {
        }

        public async Task<bool> Create(BusStop BusStop)
        {
            await ValidateName(BusStop);
            await ValidateLatitude(BusStop);
            await ValidateLongtitude(BusStop);
            return BusStop.IsValidated;
        }

        public async Task<bool> Update(BusStop BusStop)
        {
            if (await ValidateId(BusStop))
            {
                await ValidateName(BusStop);
                await ValidateLatitude(BusStop);
                await ValidateLongtitude(BusStop);
            }
            return BusStop.IsValidated;
        }

        public async Task<bool> Delete(BusStop BusStop)
        {
            var oldData = await UOW.BusStopRepository.Get(BusStop.Id);
            if (oldData != null)
            {
            }
            else
            {
                BusStop.AddError(nameof(BusStopValidator), nameof(BusStop.Id), BusStopMessage.Error.IdNotExisted, BusStopMessage);
            }
            return BusStop.IsValidated;
        }
        
        public async Task<bool> BulkDelete(List<BusStop> BusStops)
        {
            return BusStops.All(x => x.IsValidated);
        }

        public async Task<bool> Import(List<BusStop> BusStops)
        {
            return true;
        }
        
        private async Task<bool> ValidateId(BusStop BusStop)
        {
            BusStopFilter BusStopFilter = new BusStopFilter
            {
                Skip = 0,
                Take = 10,
                Id = new IdFilter { Equal = BusStop.Id },
                Selects = BusStopSelect.Id
            };

            int count = await UOW.BusStopRepository.CountAll(BusStopFilter);
            if (count == 0)
                BusStop.AddError(nameof(BusStopValidator), nameof(BusStop.Id), BusStopMessage.Error.IdNotExisted, BusStopMessage);
            return BusStop.IsValidated;
        }

        private async Task<bool> ValidateName(BusStop BusStop)
        {
            if(string.IsNullOrEmpty(BusStop.Name))
            {
                BusStop.AddError(nameof(BusStopValidator), nameof(BusStop.Name), BusStopMessage.Error.NameEmpty, BusStopMessage);
            }
            else if(BusStop.Name.Count() > 500)
            {
                BusStop.AddError(nameof(BusStopValidator), nameof(BusStop.Name), BusStopMessage.Error.NameOverLength, BusStopMessage);
            }
            return BusStop.IsValidated;
        }
        private async Task<bool> ValidateLatitude(BusStop BusStop)
        {   
            return true;
        }
        private async Task<bool> ValidateLongtitude(BusStop BusStop)
        {   
            return true;
        }
    }
}
