using System;
using System.Collections.Generic;

namespace RideSharing.Models
{
    public partial class NodeDAO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Longtitude { get; set; }
        public decimal Latitude { get; set; }
    }
}
