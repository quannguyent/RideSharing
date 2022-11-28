using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.delivery_trip
{
    public class DeliveryTrip_CityFreighterDTO : DataDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Capacity { get; set; }
        public long NodeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeliveryTrip_CityFreighterDTO() {}
        public DeliveryTrip_CityFreighterDTO(CityFreighter CityFreighter)
        {
            this.Id = CityFreighter.Id;
            this.Name = CityFreighter.Name;
            this.Capacity = CityFreighter.Capacity;
            this.NodeId = CityFreighter.NodeId;
            this.CreatedAt = CityFreighter.CreatedAt;
            this.UpdatedAt = CityFreighter.UpdatedAt;
            this.Informations = CityFreighter.Informations;
            this.Warnings = CityFreighter.Warnings;
            this.Errors = CityFreighter.Errors;
        }
    }

    public class DeliveryTrip_CityFreighterFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Name { get; set; }
        public DecimalFilter Capacity { get; set; }
        public IdFilter NodeId { get; set; }
        public CityFreighterOrder OrderBy { get; set; }
    }
}