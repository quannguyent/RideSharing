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
using RideSharing.Services.MCustomer;
using RideSharing.Services.MNode;

namespace RideSharing.Rpc.customer
{
    public partial class CustomerController 
    {
        [Route(CustomerRoute.SingleListNode), HttpPost]
        public async Task<List<Customer_NodeDTO>> SingleListNode([FromBody] Customer_NodeFilterDTO Customer_NodeFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            NodeFilter NodeFilter = new NodeFilter();
            NodeFilter.Skip = 0;
            NodeFilter.Take = 20;
            NodeFilter.OrderBy = NodeOrder.Id;
            NodeFilter.OrderType = OrderType.ASC;
            NodeFilter.Selects = NodeSelect.ALL;
            NodeFilter.Id = Customer_NodeFilterDTO.Id;
            NodeFilter.Code = Customer_NodeFilterDTO.Code;
            NodeFilter.Longtitude = Customer_NodeFilterDTO.Longtitude;
            NodeFilter.Latitude = Customer_NodeFilterDTO.Latitude;
            List<Node> Nodes = await NodeService.List(NodeFilter);
            List<Customer_NodeDTO> Customer_NodeDTOs = Nodes
                .Select(x => new Customer_NodeDTO(x)).ToList();
            return Customer_NodeDTOs;
        }
    }
}

