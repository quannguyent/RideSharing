using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RideSharing.Entities
{
    public class SystemConfig : DataEntity,  IEquatable<SystemConfig>
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal FreighterQuotientCost { get; set; }
        public decimal DeliveryRadius { get; set; }
        public long DeliveryServiceDuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool Equals(SystemConfig other)
        {
            if (other == null) return false;
            if (this.Id != other.Id) return false;
            if (this.Code != other.Code) return false;
            if (this.FreighterQuotientCost != other.FreighterQuotientCost) return false;
            if (this.DeliveryRadius != other.DeliveryRadius) return false;
            if (this.DeliveryServiceDuration != other.DeliveryServiceDuration) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class SystemConfigFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public DecimalFilter FreighterQuotientCost { get; set; }
        public DecimalFilter DeliveryRadius { get; set; }
        public LongFilter DeliveryServiceDuration { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public List<SystemConfigFilter> OrFilter { get; set; }
        public SystemConfigOrder OrderBy {get; set;}
        public SystemConfigSelect Selects {get; set;}
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SystemConfigOrder
    {
        Id = 0,
        Code = 1,
        FreighterQuotientCost = 2,
        DeliveryRadius = 3,
        DeliveryServiceDuration = 4,
        CreatedAt = 50,
        UpdatedAt = 51,
    }

    [Flags]
    public enum SystemConfigSelect:long
    {
        ALL = E.ALL,
        Id = E._0,
        Code = E._1,
        FreighterQuotientCost = E._2,
        DeliveryRadius = E._3,
        DeliveryServiceDuration = E._4,
    }
}
