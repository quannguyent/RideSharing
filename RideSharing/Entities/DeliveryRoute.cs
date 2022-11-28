using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RideSharing.Entities
{
    public class DeliveryRoute : DataEntity,  IEquatable<DeliveryRoute>
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public long CityFreighterId { get; set; }
        public decimal TotalTravelDistance { get; set; }
        public decimal TotalEmptyRunDistance { get; set; }
        public CityFreighter CityFreighter { get; set; }
        public List<DeliveryOrder> DeliveryOrders { get; set; }
        public List<DeliveryTrip> DeliveryTrips { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool Equals(DeliveryRoute other)
        {
            if (other == null) return false;
            if (this.Id != other.Id) return false;
            if (this.Path != other.Path) return false;
            if (this.CityFreighterId != other.CityFreighterId) return false;
            if (this.TotalTravelDistance != other.TotalTravelDistance) return false;
            if (this.TotalEmptyRunDistance != other.TotalEmptyRunDistance) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public void BuildPath()
        {
            Path = $"({CityFreighter.Latitude}, {CityFreighter.Longtitude})";
            foreach (var trip in DeliveryTrips)
            {
                Path += $"-> {trip.Path}";
            }
            Path += $"({CityFreighter.Latitude}, {CityFreighter.Longtitude})";
        }
    }

    public class DeliveryRouteFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Path { get; set; }
        public IdFilter CityFreighterId { get; set; }
        public DecimalFilter TotalTravelDistance { get; set; }
        public DecimalFilter TotalEmptyRunDistance { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public List<DeliveryRouteFilter> OrFilter { get; set; }
        public DeliveryRouteOrder OrderBy {get; set;}
        public DeliveryRouteSelect Selects {get; set;}
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeliveryRouteOrder
    {
        Id = 0,
        Path = 1,
        CityFreighter = 2,
        TotalTravelDistance = 3,
        TotalEmptyRunDistance = 4,
        CreatedAt = 50,
        UpdatedAt = 51,
    }

    [Flags]
    public enum DeliveryRouteSelect:long
    {
        ALL = E.ALL,
        Id = E._0,
        Path = E._1,
        CityFreighter = E._2,
        TotalTravelDistance = E._3,
        TotalEmptyRunDistance = E._4,
    }
}
