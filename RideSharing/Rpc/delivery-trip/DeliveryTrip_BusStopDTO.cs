using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.delivery_trip
{
    public class DeliveryTrip_BusStopDTO : DataDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long NodeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeliveryTrip_BusStopDTO() {}
        public DeliveryTrip_BusStopDTO(BusStop BusStop)
        {
            this.Id = BusStop.Id;
            this.Name = BusStop.Name;
            this.NodeId = BusStop.NodeId;
            this.CreatedAt = BusStop.CreatedAt;
            this.UpdatedAt = BusStop.UpdatedAt;
            this.Informations = BusStop.Informations;
            this.Warnings = BusStop.Warnings;
            this.Errors = BusStop.Errors;
        }
    }

    public class DeliveryTrip_BusStopFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Name { get; set; }
        public IdFilter NodeId { get; set; }
        public BusStopOrder OrderBy { get; set; }
    }
}