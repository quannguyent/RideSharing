using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.bus_stop
{
    public class BusStop_NodeDTO : DataDTO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Longtitude { get; set; }
        public decimal Latitude { get; set; }
        public BusStop_NodeDTO() {}
        public BusStop_NodeDTO(Node Node)
        {
            this.Id = Node.Id;
            this.Code = Node.Code;
            this.Longtitude = Node.Longtitude;
            this.Latitude = Node.Latitude;
            this.Informations = Node.Informations;
            this.Warnings = Node.Warnings;
            this.Errors = Node.Errors;
        }
    }

    public class BusStop_NodeFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public DecimalFilter Longtitude { get; set; }
        public DecimalFilter Latitude { get; set; }
        public NodeOrder OrderBy { get; set; }
    }
}