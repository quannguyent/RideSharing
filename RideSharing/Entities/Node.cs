using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RideSharing.Entities
{
    public class Node : DataEntity,  IEquatable<Node>
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Longtitude { get; set; }
        public decimal Latitude { get; set; }
        
        public bool Equals(Node other)
        {
            if (other == null) return false;
            if (this.Id != other.Id) return false;
            if (this.Code != other.Code) return false;
            if (this.Longtitude != other.Longtitude) return false;
            if (this.Latitude != other.Latitude) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public Node() { }
        public Node(decimal longtitude, decimal latitude)
        {
            Longtitude = longtitude;
            Latitude = latitude;
        }
    }

    public class NodeFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public DecimalFilter Longtitude { get; set; }
        public DecimalFilter Latitude { get; set; }
        public List<NodeFilter> OrFilter { get; set; }
        public NodeOrder OrderBy {get; set;}
        public NodeSelect Selects {get; set;}
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NodeOrder
    {
        Id = 0,
        Code = 1,
        Longtitude = 2,
        Latitude = 3,
    }

    [Flags]
    public enum NodeSelect:long
    {
        ALL = E.ALL,
        Id = E._0,
        Code = E._1,
        Longtitude = E._2,
        Latitude = E._3,
    }
}
