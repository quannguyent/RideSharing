using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RideSharing.Entities
{
    public class BusStop : DataEntity,  IEquatable<BusStop>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long NodeId { get; set; }
        public Node Node { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool Equals(BusStop other)
        {
            if (other == null) return false;
            if (this.Id != other.Id) return false;
            if (this.Name != other.Name) return false;
            if (this.NodeId != other.NodeId) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class BusStopFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Name { get; set; }
        public IdFilter NodeId { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public List<BusStopFilter> OrFilter { get; set; }
        public BusStopOrder OrderBy {get; set;}
        public BusStopSelect Selects {get; set;}
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BusStopOrder
    {
        Id = 0,
        Name = 1,
        Node = 2,
        CreatedAt = 50,
        UpdatedAt = 51,
    }

    [Flags]
    public enum BusStopSelect:long
    {
        ALL = E.ALL,
        Id = E._0,
        Name = E._1,
        Node = E._2,
    }
}
