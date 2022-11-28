using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.delivery_route
{
    public class DeliveryRoute_CityFreighterDTO : DataDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Capacity { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeliveryRoute_CityFreighterDTO() {}
        public DeliveryRoute_CityFreighterDTO(CityFreighter CityFreighter)
        {
            this.Id = CityFreighter.Id;
            this.Name = CityFreighter.Name;
            this.Capacity = CityFreighter.Capacity;
            this.Latitude = CityFreighter.Latitude;
            this.Longtitude = CityFreighter.Longtitude;
            this.CreatedAt = CityFreighter.CreatedAt;
            this.UpdatedAt = CityFreighter.UpdatedAt;
            this.Informations = CityFreighter.Informations;
            this.Warnings = CityFreighter.Warnings;
            this.Errors = CityFreighter.Errors;
        }
    }

    public class DeliveryRoute_CityFreighterFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Name { get; set; }
        public DecimalFilter Capacity { get; set; }
        public DecimalFilter Latitude { get; set; }
        public DecimalFilter Longtitude { get; set; }
        public CityFreighterOrder OrderBy { get; set; }
    }
}