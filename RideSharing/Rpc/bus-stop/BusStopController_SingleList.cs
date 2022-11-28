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
using RideSharing.Services.MBusStop;
using RideSharing.Services.MNode;

namespace RideSharing.Rpc.bus_stop
{
    public partial class BusStopController 
    {
        [Route(BusStopRoute.SingleListNode), HttpPost]
        public async Task<List<BusStop_NodeDTO>> SingleListNode([FromBody] BusStop_NodeFilterDTO BusStop_NodeFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            NodeFilter NodeFilter = new NodeFilter();
            NodeFilter.Skip = 0;
            NodeFilter.Take = 20;
            NodeFilter.OrderBy = NodeOrder.Id;
            NodeFilter.OrderType = OrderType.ASC;
            NodeFilter.Selects = NodeSelect.ALL;
            NodeFilter.Id = BusStop_NodeFilterDTO.Id;
            NodeFilter.Code = BusStop_NodeFilterDTO.Code;
            NodeFilter.Longtitude = BusStop_NodeFilterDTO.Longtitude;
            NodeFilter.Latitude = BusStop_NodeFilterDTO.Latitude;
            List<Node> Nodes = await NodeService.List(NodeFilter);
            List<BusStop_NodeDTO> BusStop_NodeDTOs = Nodes
                .Select(x => new BusStop_NodeDTO(x)).ToList();
            return BusStop_NodeDTOs;
        }
    }
}

