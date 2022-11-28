using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.node
{
    public class Node_NodeExportDTO : DataDTO
    {
        public string STT {get; set; }
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Longtitude { get; set; }
        public decimal Latitude { get; set; }
        public Node_NodeExportDTO() {}
        public Node_NodeExportDTO(Node Node)
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
}
