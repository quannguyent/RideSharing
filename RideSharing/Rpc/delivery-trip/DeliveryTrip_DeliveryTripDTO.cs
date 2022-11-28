using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.delivery_trip
{
    public class DeliveryTrip_DeliveryTripDTO : DataDTO
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public long CityFreighterId { get; set; }
        public long BusStopId { get; set; }
        public decimal TravelDistance { get; set; }
        public DeliveryTrip_BusStopDTO BusStop { get; set; }
        public DeliveryTrip_CityFreighterDTO CityFreighter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeliveryTrip_DeliveryTripDTO() {}
        public DeliveryTrip_DeliveryTripDTO(DeliveryTrip DeliveryTrip)
        {
            this.Id = DeliveryTrip.Id;
            this.Path = DeliveryTrip.Path;
            this.CityFreighterId = DeliveryTrip.CityFreighterId;
            this.BusStopId = DeliveryTrip.BusStopId;
            this.TravelDistance = DeliveryTrip.TravelDistance;
            this.BusStop = DeliveryTrip.BusStop == null ? null : new DeliveryTrip_BusStopDTO(DeliveryTrip.BusStop);
            this.CityFreighter = DeliveryTrip.CityFreighter == null ? null : new DeliveryTrip_CityFreighterDTO(DeliveryTrip.CityFreighter);
            this.CreatedAt = DeliveryTrip.CreatedAt;
            this.UpdatedAt = DeliveryTrip.UpdatedAt;
            this.Informations = DeliveryTrip.Informations;
            this.Warnings = DeliveryTrip.Warnings;
            this.Errors = DeliveryTrip.Errors;
        }
    }

    public class DeliveryTrip_DeliveryTripFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Path { get; set; }
        public IdFilter CityFreighterId { get; set; }
        public IdFilter BusStopId { get; set; }
        public DecimalFilter TravelDistance { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public DeliveryTripOrder OrderBy { get; set; }
    }
}
