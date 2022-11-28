using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.system_config
{
    public class SystemConfig_SystemConfigExportDTO : DataDTO
    {
        public string STT {get; set; }
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal FreighterQuotientCost { get; set; }
        public decimal DeliveryRadius { get; set; }
        public long DeliveryServiceDuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public SystemConfig_SystemConfigExportDTO() {}
        public SystemConfig_SystemConfigExportDTO(SystemConfig SystemConfig)
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
}
