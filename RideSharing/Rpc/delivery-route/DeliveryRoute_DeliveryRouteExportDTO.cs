using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.delivery_route
{
    public class DeliveryRoute_DeliveryRouteExportDTO : DataDTO
    {
        public string STT {get; set; }
        public long Id { get; set; }
        public string Path { get; set; }
        public long CityFreighterId { get; set; }
        public decimal TotalTravelDistance { get; set; }
        public decimal TotalEmptyRunDistance { get; set; }
        public DeliveryRoute_CityFreighterDTO CityFreighter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeliveryRoute_DeliveryRouteExportDTO() {}
        public DeliveryRoute_DeliveryRouteExportDTO(DeliveryRoute DeliveryRoute)
        {
            this.Id = DeliveryRoute.Id;
            this.Path = DeliveryRoute.Path;
            this.CityFreighterId = DeliveryRoute.CityFreighterId;
            this.TotalTravelDistance = DeliveryRoute.TotalTravelDistance;
            this.TotalEmptyRunDistance = DeliveryRoute.TotalEmptyRunDistance;
            this.CityFreighter = DeliveryRoute.CityFreighter == null ? null : new DeliveryRoute_CityFreighterDTO(DeliveryRoute.CityFreighter);
            this.CreatedAt = DeliveryRoute.CreatedAt;
            this.UpdatedAt = DeliveryRoute.UpdatedAt;
            this.Informations = DeliveryRoute.Informations;
            this.Warnings = DeliveryRoute.Warnings;
            this.Errors = DeliveryRoute.Errors;
        }
    }
}
