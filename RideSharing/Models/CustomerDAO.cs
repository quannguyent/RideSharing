using System;
using System.Collections.Generic;

namespace RideSharing.Models
{
    public partial class CustomerDAO
    {
        public CustomerDAO()
        {
            DeliveryOrders = new HashSet<DeliveryOrderDAO>();
        }

        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public long NodeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual NodeDAO Node { get; set; }
        public virtual ICollection<DeliveryOrderDAO> DeliveryOrders { get; set; }
    }
}
