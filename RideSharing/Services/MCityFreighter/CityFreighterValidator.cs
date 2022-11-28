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

namespace RideSharing.Services.MCityFreighter
{
    public interface ICityFreighterValidator : IServiceScoped
    {
        Task Get(CityFreighter CityFreighter);
        Task<bool> Create(CityFreighter CityFreighter);
        Task<bool> Update(CityFreighter CityFreighter);
        Task<bool> Delete(CityFreighter CityFreighter);
        Task<bool> BulkDelete(List<CityFreighter> CityFreighters);
        Task<bool> Import(List<CityFreighter> CityFreighters);
    }

    public class CityFreighterValidator : ICityFreighterValidator
    {
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private CityFreighterMessage CityFreighterMessage;

        public CityFreighterValidator(IUOW UOW, ICurrentContext CurrentContext)
        {
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.CityFreighterMessage = new CityFreighterMessage();
        }

        public async Task Get(CityFreighter CityFreighter)
        {
        }

        public async Task<bool> Create(CityFreighter CityFreighter)
        {
            await ValidateName(CityFreighter);
            await ValidateCapacity(CityFreighter);
            await ValidateNode(CityFreighter);
            return CityFreighter.IsValidated;
        }

        public async Task<bool> Update(CityFreighter CityFreighter)
        {
            if (await ValidateId(CityFreighter))
            {
                await ValidateName(CityFreighter);
                await ValidateCapacity(CityFreighter);
                await ValidateNode(CityFreighter);
            }
            return CityFreighter.IsValidated;
        }

        public async Task<bool> Delete(CityFreighter CityFreighter)
        {
            var oldData = await UOW.CityFreighterRepository.Get(CityFreighter.Id);
            if (oldData != null)
            {
            }
            else
            {
                CityFreighter.AddError(nameof(CityFreighterValidator), nameof(CityFreighter.Id), CityFreighterMessage.Error.IdNotExisted, CityFreighterMessage);
            }
            return CityFreighter.IsValidated;
        }
        
        public async Task<bool> BulkDelete(List<CityFreighter> CityFreighters)
        {
            return CityFreighters.All(x => x.IsValidated);
        }

        public async Task<bool> Import(List<CityFreighter> CityFreighters)
        {
            return true;
        }
        
        private async Task<bool> ValidateId(CityFreighter CityFreighter)
        {
            CityFreighterFilter CityFreighterFilter = new CityFreighterFilter
            {
                Skip = 0,
                Take = 10,
                Id = new IdFilter { Equal = CityFreighter.Id },
                Selects = CityFreighterSelect.Id
            };

            int count = await UOW.CityFreighterRepository.CountAll(CityFreighterFilter);
            if (count == 0)
                CityFreighter.AddError(nameof(CityFreighterValidator), nameof(CityFreighter.Id), CityFreighterMessage.Error.IdNotExisted, CityFreighterMessage);
            return CityFreighter.IsValidated;
        }

        private async Task<bool> ValidateName(CityFreighter CityFreighter)
        {
            if(string.IsNullOrEmpty(CityFreighter.Name))
            {
                CityFreighter.AddError(nameof(CityFreighterValidator), nameof(CityFreighter.Name), CityFreighterMessage.Error.NameEmpty, CityFreighterMessage);
            }
            else if(CityFreighter.Name.Count() > 500)
            {
                CityFreighter.AddError(nameof(CityFreighterValidator), nameof(CityFreighter.Name), CityFreighterMessage.Error.NameOverLength, CityFreighterMessage);
            }
            return CityFreighter.IsValidated;
        }
        private async Task<bool> ValidateCapacity(CityFreighter CityFreighter)
        {   
            return true;
        }
        private async Task<bool> ValidateNode(CityFreighter CityFreighter)
        {       
            if(CityFreighter.NodeId == 0)
            {
                CityFreighter.AddError(nameof(CityFreighterValidator), nameof(CityFreighter.Node), CityFreighterMessage.Error.NodeEmpty, CityFreighterMessage);
            }
            else
            {
                int count = await UOW.NodeRepository.CountAll(new NodeFilter
                {
                    Id = new IdFilter{ Equal =  CityFreighter.NodeId },
                });
                if(count == 0)
                {
                    CityFreighter.AddError(nameof(CityFreighterValidator), nameof(CityFreighter.Node), CityFreighterMessage.Error.NodeNotExisted, CityFreighterMessage);
                }
            }
            return true;
        }
    }
}
