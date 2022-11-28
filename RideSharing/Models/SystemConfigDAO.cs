using System;
using System.Collections.Generic;

namespace RideSharing.Models
{
    public partial class SystemConfigDAO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal FreighterQuotientCost { get; set; }
        public decimal DeliveryRadius { get; set; }
        public long DeliveryServiceDuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
