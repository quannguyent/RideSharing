using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RideSharing.Entities
{
    public class DeliveryOrder : DataEntity,  IEquatable<DeliveryOrder>
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Weight { get; set; }
        public long CustomerId { get; set; }
        public Customer Customer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool Equals(DeliveryOrder other)
        {
            if (other == null) return false;
            if (this.Id != other.Id) return false;
            if (this.Code != other.Code) return false;
            if (this.Weight != other.Weight) return false;
            if (this.CustomerId != other.CustomerId) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class DeliveryOrderFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public DecimalFilter Weight { get; set; }
        public IdFilter CustomerId { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public List<DeliveryOrderFilter> OrFilter { get; set; }
        public DeliveryOrderOrder OrderBy {get; set;}
        public DeliveryOrderSelect Selects {get; set;}
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeliveryOrderOrder
    {
        Id = 0,
        Code = 1,
        Weight = 2,
        Customer = 3,
        CreatedAt = 50,
        UpdatedAt = 51,
    }

    [Flags]
    public enum DeliveryOrderSelect:long
    {
        ALL = E.ALL,
        Id = E._0,
        Code = E._1,
        Weight = E._2,
        Customer = E._3,
    }
}
