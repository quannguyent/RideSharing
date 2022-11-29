using TrueSight.Common;
using System;
using System.Collections.Generic;
using RideSharing.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nest;

namespace RideSharing.Entities
{
    public class DeliverySchedule : DataEntity,  IEquatable<DeliveryRoute>
    {
        public SystemConfig Config { get; set; }
        public List<DeliveryOrder> DeliveryOrders { get; set; }
        public List<BusStop> BusStops { get; set; }
        public List<CityFreighter> CityFreighters { get; set; }
        public List<DeliveryRoute> DeliveryRoutes { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalTravelDistance { get; set; }
        public decimal TotalEmptyRun { get; set; }
        public long NumberOfFreigther { get; set; }
        public long NumberOfTrip { get; set; }
        public decimal MaximumOrderWeight { get; set; }
        public decimal MinimumOrderWeight { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool Equals(DeliveryRoute other)
        {
            if (other == null) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class DeliveryScheduleFilter : FilterEntity
    {
        public IdFilter Id { get; set; }
        public StringFilter Path { get; set; }
        public IdFilter CityFreighterId { get; set; }
        public DecimalFilter TotalTravelDistance { get; set; }
        public DecimalFilter TotalEmptyRunDistance { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public DeliveryRouteOrder OrderBy {get; set;}
        public DeliveryRouteSelect Selects {get; set;}
    }
}
