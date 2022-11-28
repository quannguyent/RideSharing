using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.customer
{
    public class Customer_NodeDTO : DataDTO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Longtitude { get; set; }
        public decimal Latitude { get; set; }
        public Customer_NodeDTO() {}
        public Customer_NodeDTO(Node Node)
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

    public class Customer_NodeFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public DecimalFilter Longtitude { get; set; }
        public DecimalFilter Latitude { get; set; }
        public NodeOrder OrderBy { get; set; }
    }
}