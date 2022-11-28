using System;
using System.Collections.Generic;

namespace RideSharing.Models
{
    public partial class DeliveryRouteDAO
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public long CityFreighterId { get; set; }
        public decimal TotalTravelDistance { get; set; }
        public decimal TotalEmptyRunDistance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual CityFreighterDAO CityFreighter { get; set; }
    }
}
