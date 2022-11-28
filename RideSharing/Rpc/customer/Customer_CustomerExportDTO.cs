using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.customer
{
    public class Customer_CustomerExportDTO : DataDTO
    {
        public string STT {get; set; }
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Customer_CustomerExportDTO() {}
        public Customer_CustomerExportDTO(Customer Customer)
        {
            this.Id = Customer.Id;
            this.Code = Customer.Code;
            this.Name = Customer.Name;
            this.Latitude = Customer.Latitude;
            this.Longtitude = Customer.Longtitude;
            this.CreatedAt = Customer.CreatedAt;
            this.UpdatedAt = Customer.UpdatedAt;
            this.Informations = Customer.Informations;
            this.Warnings = Customer.Warnings;
            this.Errors = Customer.Errors;
        }
    }
}
