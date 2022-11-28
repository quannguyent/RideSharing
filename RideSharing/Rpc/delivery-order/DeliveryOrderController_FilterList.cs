using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RideSharing.Common;
using RideSharing.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using OfficeOpenXml;
using RideSharing.Entities;
using RideSharing.Services.MDeliveryOrder;
using RideSharing.Services.MCustomer;

namespace RideSharing.Rpc.delivery_order
{
    public partial class DeliveryOrderController 
    {
        [Route(DeliveryOrderRoute.FilterListCustomer), HttpPost]
        public async Task<List<DeliveryOrder_CustomerDTO>> FilterListCustomer([FromBody] DeliveryOrder_CustomerFilterDTO DeliveryOrder_CustomerFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CustomerFilter CustomerFilter = new CustomerFilter();
            CustomerFilter.Skip = 0;
            CustomerFilter.Take = 20;
            CustomerFilter.OrderBy = CustomerOrder.Id;
            CustomerFilter.OrderType = OrderType.ASC;
            CustomerFilter.Selects = CustomerSelect.ALL;
            CustomerFilter.Id = DeliveryOrder_CustomerFilterDTO.Id;
            CustomerFilter.Code = DeliveryOrder_CustomerFilterDTO.Code;
            CustomerFilter.Name = DeliveryOrder_CustomerFilterDTO.Name;
            CustomerFilter.NodeId = DeliveryOrder_CustomerFilterDTO.NodeId;

            List<Customer> Customers = await CustomerService.List(CustomerFilter);
            List<DeliveryOrder_CustomerDTO> DeliveryOrder_CustomerDTOs = Customers
                .Select(x => new DeliveryOrder_CustomerDTO(x)).ToList();
            return DeliveryOrder_CustomerDTOs;
        }
    }
}

