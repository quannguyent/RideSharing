using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RideSharing.Entities
{
    public class CityFreighter : DataEntity,  IEquatable<CityFreighter>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Capacity { get; set; }
        public long NodeId { get; set; }
        public Node Node { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool Equals(CityFreighter other)
        {
            if (other == null) return false;
            if (this.Id != other.Id) return false;
            if (this.Name != other.Name) return false;
            if (this.Capacity != other.Capacity) return false;
            if (this.NodeId != other.NodeId) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class CityFreighterFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Name { get; set; }
        public DecimalFilter Capacity { get; set; }
        public IdFilter NodeId { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public List<CityFreighterFilter> OrFilter { get; set; }
        public CityFreighterOrder OrderBy {get; set;}
        public CityFreighterSelect Selects {get; set;}
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CityFreighterOrder
    {
        Id = 0,
        Name = 1,
        Capacity = 2,
        Node = 3,
        CreatedAt = 50,
        UpdatedAt = 51,
    }

    [Flags]
    public enum CityFreighterSelect:long
    {
        ALL = E.ALL,
        Id = E._0,
        Name = E._1,
        Capacity = E._2,
        Node = E._3,
    }
}
