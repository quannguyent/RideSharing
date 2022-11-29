using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace RideSharing.Entities
{
    public class DeliveryTrip : DataEntity,  IEquatable<DeliveryTrip>
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public long CityFreighterId { get; set; }
        public long BusStopId { get; set; }
        public decimal TravelDistance { get; set; }
        public BusStop BusStop { get; set; }
        public CityFreighter CityFreighter { get; set; }
        public List<DeliveryOrder> DeliveryOrders { get; set; }
        public List<Node> PlannedNode { get; set; }
        public List<Edge> PlannedRoute { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool Equals(DeliveryTrip other)
        {
            if (other == null) return false;
            if (this.Id != other.Id) return false;
            if (this.Path != other.Path) return false;
            if (this.CityFreighterId != other.CityFreighterId) return false;
            if (this.BusStopId != other.BusStopId) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public DeliveryTrip() { }
        public DeliveryTrip(DeliveryTrip DeliveryTrip)
        {
            this.Id = DeliveryTrip.Id;
            this.Path = DeliveryTrip.Path;
            this.CityFreighterId = DeliveryTrip.CityFreighterId;
            this.BusStopId = DeliveryTrip.BusStopId;
            this.TravelDistance = DeliveryTrip.TravelDistance;
            this.BusStop = DeliveryTrip.BusStop == null ? null : new BusStop(DeliveryTrip.BusStop);
            this.CityFreighter = DeliveryTrip.CityFreighter == null ? null : new CityFreighter(DeliveryTrip.CityFreighter);
            this.DeliveryOrders = DeliveryTrip.DeliveryOrders?.Select(x => new DeliveryOrder(x)).ToList();
            this.PlannedNode = DeliveryTrip.PlannedNode?.Select(x => new Node(x)).ToList();
            this.PlannedRoute = DeliveryTrip.PlannedRoute?.Select(x => new Edge(x)).ToList();
            this.CreatedAt = DeliveryTrip.CreatedAt;
            this.UpdatedAt = DeliveryTrip.UpdatedAt;
        }
    }

    public class DeliveryTripFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Path { get; set; }
        public IdFilter CityFreighterId { get; set; }
        public IdFilter BusStopId { get; set; }
        public DecimalFilter TravelDistance { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public List<DeliveryTripFilter> OrFilter { get; set; }
        public DeliveryTripOrder OrderBy {get; set;}
        public DeliveryTripSelect Selects {get; set;}
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeliveryTripOrder
    {
        Id = 0,
        Path = 1,
        CityFreighter = 2,
        BusStop = 3,
        TravelDistance = 4,
        CreatedAt = 50,
        UpdatedAt = 51,
    }

    [Flags]
    public enum DeliveryTripSelect:long
    {
        ALL = E.ALL,
        Id = E._0,
        Path = E._1,
        CityFreighter = E._2,
        BusStop = E._3,
        TravelDistance = E._4,
    }
}
