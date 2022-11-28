using System;
using System.Collections.Generic;

namespace RideSharing.Models
{
    public partial class DeliveryTripDAO
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public long CityFreighterId { get; set; }
        public long BusStopId { get; set; }
        public decimal TravelDistance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual BusStopDAO BusStop { get; set; }
        public virtual CityFreighterDAO CityFreighter { get; set; }
    }
}
