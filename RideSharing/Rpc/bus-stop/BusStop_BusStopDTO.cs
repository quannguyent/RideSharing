using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.bus_stop
{
    public class BusStop_BusStopDTO : DataDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long NodeId { get; set; }
        public BusStop_NodeDTO Node { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public BusStop_BusStopDTO() {}
        public BusStop_BusStopDTO(BusStop BusStop)
        {
            this.Id = BusStop.Id;
            this.Name = BusStop.Name;
            this.NodeId = BusStop.NodeId;
            this.Node = BusStop.Node == null ? null : new BusStop_NodeDTO(BusStop.Node);
            this.CreatedAt = BusStop.CreatedAt;
            this.UpdatedAt = BusStop.UpdatedAt;
            this.Informations = BusStop.Informations;
            this.Warnings = BusStop.Warnings;
            this.Errors = BusStop.Errors;
        }
    }

    public class BusStop_BusStopFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Name { get; set; }
        public IdFilter NodeId { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public BusStopOrder OrderBy { get; set; }
    }
}
