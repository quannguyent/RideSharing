using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RideSharing.Entities
{
    public class Customer : DataEntity,  IEquatable<Customer>
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool Equals(Customer other)
        {
            if (other == null) return false;
            if (this.Id != other.Id) return false;
            if (this.Code != other.Code) return false;
            if (this.Name != other.Name) return false;
            if (this.Latitude != other.Latitude) return false;
            if (this.Longtitude != other.Longtitude) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class CustomerFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public StringFilter Name { get; set; }
        public DecimalFilter Latitude { get; set; }
        public DecimalFilter Longtitude { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public List<CustomerFilter> OrFilter { get; set; }
        public CustomerOrder OrderBy {get; set;}
        public CustomerSelect Selects {get; set;}
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomerOrder
    {
        Id = 0,
        Code = 1,
        Name = 2,
        Latitude = 3,
        Longtitude = 4,
        CreatedAt = 50,
        UpdatedAt = 51,
    }

    [Flags]
    public enum CustomerSelect:long
    {
        ALL = E.ALL,
        Id = E._0,
        Code = E._1,
        Name = E._2,
        Latitude = E._3,
        Longtitude = E._4,
    }
}
