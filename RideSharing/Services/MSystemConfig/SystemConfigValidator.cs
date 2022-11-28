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

namespace RideSharing.Services.MSystemConfig
{
    public interface ISystemConfigValidator : IServiceScoped
    {
        Task Get(SystemConfig SystemConfig);
        Task<bool> Create(SystemConfig SystemConfig);
        Task<bool> Update(SystemConfig SystemConfig);
        Task<bool> Delete(SystemConfig SystemConfig);
        Task<bool> BulkDelete(List<SystemConfig> SystemConfigs);
        Task<bool> Import(List<SystemConfig> SystemConfigs);
    }

    public class SystemConfigValidator : ISystemConfigValidator
    {
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private SystemConfigMessage SystemConfigMessage;

        public SystemConfigValidator(IUOW UOW, ICurrentContext CurrentContext)
        {
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.SystemConfigMessage = new SystemConfigMessage();
        }

        public async Task Get(SystemConfig SystemConfig)
        {
        }

        public async Task<bool> Create(SystemConfig SystemConfig)
        {
            await ValidateCode(SystemConfig);
            await ValidateFreighterQuotientCost(SystemConfig);
            await ValidateDeliveryRadius(SystemConfig);
            await ValidateDeliveryServiceDuration(SystemConfig);
            return SystemConfig.IsValidated;
        }

        public async Task<bool> Update(SystemConfig SystemConfig)
        {
            if (await ValidateId(SystemConfig))
            {
                await ValidateCode(SystemConfig);
                await ValidateFreighterQuotientCost(SystemConfig);
                await ValidateDeliveryRadius(SystemConfig);
                await ValidateDeliveryServiceDuration(SystemConfig);
            }
            return SystemConfig.IsValidated;
        }

        public async Task<bool> Delete(SystemConfig SystemConfig)
        {
            var oldData = await UOW.SystemConfigRepository.Get(SystemConfig.Id);
            if (oldData != null)
            {
            }
            else
            {
                SystemConfig.AddError(nameof(SystemConfigValidator), nameof(SystemConfig.Id), SystemConfigMessage.Error.IdNotExisted, SystemConfigMessage);
            }
            return SystemConfig.IsValidated;
        }
        
        public async Task<bool> BulkDelete(List<SystemConfig> SystemConfigs)
        {
            return SystemConfigs.All(x => x.IsValidated);
        }

        public async Task<bool> Import(List<SystemConfig> SystemConfigs)
        {
            return true;
        }
        
        private async Task<bool> ValidateId(SystemConfig SystemConfig)
        {
            SystemConfigFilter SystemConfigFilter = new SystemConfigFilter
            {
                Skip = 0,
                Take = 10,
                Id = new IdFilter { Equal = SystemConfig.Id },
                Selects = SystemConfigSelect.Id
            };

            int count = await UOW.SystemConfigRepository.CountAll(SystemConfigFilter);
            if (count == 0)
                SystemConfig.AddError(nameof(SystemConfigValidator), nameof(SystemConfig.Id), SystemConfigMessage.Error.IdNotExisted, SystemConfigMessage);
            return SystemConfig.IsValidated;
        }

        private async Task<bool> ValidateCode(SystemConfig SystemConfig)
        {
            if(string.IsNullOrEmpty(SystemConfig.Code))
            {
                SystemConfig.AddError(nameof(SystemConfigValidator), nameof(SystemConfig.Code), SystemConfigMessage.Error.CodeEmpty, SystemConfigMessage);
            }
            else if(SystemConfig.Code.Count() > 500)
            {
                SystemConfig.AddError(nameof(SystemConfigValidator), nameof(SystemConfig.Code), SystemConfigMessage.Error.CodeOverLength, SystemConfigMessage);
            }
            return SystemConfig.IsValidated;
        }
        private async Task<bool> ValidateFreighterQuotientCost(SystemConfig SystemConfig)
        {   
            return true;
        }
        private async Task<bool> ValidateDeliveryRadius(SystemConfig SystemConfig)
        {   
            return true;
        }
        private async Task<bool> ValidateDeliveryServiceDuration(SystemConfig SystemConfig)
        {   
            return true;
        }
    }
}
