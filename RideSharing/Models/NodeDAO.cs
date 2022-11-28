using System;
using System.Collections.Generic;

namespace RideSharing.Models
{
    public partial class NodeDAO
    {
        public NodeDAO()
        {
            BusStops = new HashSet<BusStopDAO>();
            CityFreighters = new HashSet<CityFreighterDAO>();
            Customers = new HashSet<CustomerDAO>();
        }

        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Longtitude { get; set; }
        public decimal Latitude { get; set; }

        public virtual ICollection<BusStopDAO> BusStops { get; set; }
        public virtual ICollection<CityFreighterDAO> CityFreighters { get; set; }
        public virtual ICollection<CustomerDAO> Customers { get; set; }
    }
}
