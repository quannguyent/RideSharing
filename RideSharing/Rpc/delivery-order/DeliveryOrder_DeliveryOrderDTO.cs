using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using RideSharing.Entities;

namespace RideSharing.Rpc.delivery_order
{
    public class DeliveryOrder_DeliveryOrderDTO : DataDTO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public decimal Weight { get; set; }
        public long CustomerId { get; set; }
        public DeliveryOrder_CustomerDTO Customer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeliveryOrder_DeliveryOrderDTO() {}
        public DeliveryOrder_DeliveryOrderDTO(DeliveryOrder DeliveryOrder)
        {
            this.Id = DeliveryOrder.Id;
            this.Code = DeliveryOrder.Code;
            this.Weight = DeliveryOrder.Weight;
            this.CustomerId = DeliveryOrder.CustomerId;
            this.Customer = DeliveryOrder.Customer == null ? null : new DeliveryOrder_CustomerDTO(DeliveryOrder.Customer);
            this.CreatedAt = DeliveryOrder.CreatedAt;
            this.UpdatedAt = DeliveryOrder.UpdatedAt;
            this.Informations = DeliveryOrder.Informations;
            this.Warnings = DeliveryOrder.Warnings;
            this.Errors = DeliveryOrder.Errors;
        }
    }

    public class DeliveryOrder_DeliveryOrderFilterDTO : FilterDTO
    {
        public IdFilter Id { get; set; }
        public StringFilter Code { get; set; }
        public DecimalFilter Weight { get; set; }
        public IdFilter CustomerId { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
        public DeliveryOrderOrder OrderBy { get; set; }
    }
}
