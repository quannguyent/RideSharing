using System;
using System.Collections.Generic;

namespace RideSharing.Models
{
    public partial class DeliveryOrderDAO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Weight { get; set; }
        public long CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual CustomerDAO Customer { get; set; }
    }
}
