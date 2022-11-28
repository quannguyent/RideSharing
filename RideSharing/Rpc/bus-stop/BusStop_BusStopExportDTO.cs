using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.bus_stop
{
    public class BusStop_BusStopExportDTO : DataDTO
    {
        public string STT {get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public long NodeId { get; set; }
        public BusStop_NodeDTO Node { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public BusStop_BusStopExportDTO() {}
        public BusStop_BusStopExportDTO(BusStop BusStop)
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
}
