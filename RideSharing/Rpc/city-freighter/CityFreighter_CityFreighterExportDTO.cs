using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.city_freighter
{
    public class CityFreighter_CityFreighterExportDTO : DataDTO
    {
        public string STT {get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Capacity { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CityFreighter_CityFreighterExportDTO() {}
        public CityFreighter_CityFreighterExportDTO(CityFreighter CityFreighter)
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
}
