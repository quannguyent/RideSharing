using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RideSharing.Entities
{
    public class Edge : DataEntity, IEquatable<Edge>
    {
        public Node Source { get; set; }
        public Node Destination { get; set; }
        public decimal Distance { get; set; }
        public bool Equals(Edge other)
        {
            if (this.Source.Equals(other.Source)) return false;
            if (this.Destination.Equals(other.Destination)) return false;
            return true;
        }
    }
}
