using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.delivery_order
{
    public class DeliveryOrder_CustomerDTO : DataDTO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeliveryOrder_CustomerDTO() {}
        public DeliveryOrder_CustomerDTO(Customer Customer)
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

    public class DeliveryOrder_CustomerFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public StringFilter Name { get; set; }
        public DecimalFilter Latitude { get; set; }
        public DecimalFilter Longtitude { get; set; }
        public CustomerOrder OrderBy { get; set; }
    }
}