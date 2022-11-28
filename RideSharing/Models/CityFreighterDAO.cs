using System;
using System.Collections.Generic;

namespace RideSharing.Models
{
    public partial class CityFreighterDAO
    {
        public CityFreighterDAO()
        {
            DeliveryRoutes = new HashSet<DeliveryRouteDAO>();
            DeliveryTrips = new HashSet<DeliveryTripDAO>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Capacity { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<DeliveryRouteDAO> DeliveryRoutes { get; set; }
        public virtual ICollection<DeliveryTripDAO> DeliveryTrips { get; set; }
    }
}
