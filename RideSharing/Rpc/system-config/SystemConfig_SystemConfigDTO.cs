using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.system_config
{
    public class SystemConfig_SystemConfigDTO : DataDTO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal FreighterQuotientCost { get; set; }
        public decimal DeliveryRadius { get; set; }
        public long DeliveryServiceDuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public SystemConfig_SystemConfigDTO() {}
        public SystemConfig_SystemConfigDTO(SystemConfig SystemConfig)
        {
            this.Id = SystemConfig.Id;
            this.Code = SystemConfig.Code;
            this.FreighterQuotientCost = SystemConfig.FreighterQuotientCost;
            this.DeliveryRadius = SystemConfig.DeliveryRadius;
            this.DeliveryServiceDuration = SystemConfig.DeliveryServiceDuration;
            this.CreatedAt = SystemConfig.CreatedAt;
            this.UpdatedAt = SystemConfig.UpdatedAt;
            this.Informations = SystemConfig.Informations;
            this.Warnings = SystemConfig.Warnings;
            this.Errors = SystemConfig.Errors;
        }
    }

    public class SystemConfig_SystemConfigFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public DecimalFilter FreighterQuotientCost { get; set; }
        public DecimalFilter DeliveryRadius { get; set; }
        public LongFilter DeliveryServiceDuration { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public SystemConfigOrder OrderBy { get; set; }
    }
}
