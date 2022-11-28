using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.city_freighter
{
    public class CityFreighter_NodeDTO : DataDTO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Longtitude { get; set; }
        public decimal Latitude { get; set; }
        public CityFreighter_NodeDTO() {}
        public CityFreighter_NodeDTO(Node Node)
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

    public class CityFreighter_NodeFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public DecimalFilter Longtitude { get; set; }
        public DecimalFilter Latitude { get; set; }
        public NodeOrder OrderBy { get; set; }
    }
}