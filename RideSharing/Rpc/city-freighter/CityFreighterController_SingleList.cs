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
using RideSharing.Services.MCityFreighter;
using RideSharing.Services.MNode;

namespace RideSharing.Rpc.city_freighter
{
    public partial class CityFreighterController 
    {
        [Route(CityFreighterRoute.SingleListNode), HttpPost]
        public async Task<List<CityFreighter_NodeDTO>> SingleListNode([FromBody] CityFreighter_NodeFilterDTO CityFreighter_NodeFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            NodeFilter NodeFilter = new NodeFilter();
            NodeFilter.Skip = 0;
            NodeFilter.Take = 20;
            NodeFilter.OrderBy = NodeOrder.Id;
            NodeFilter.OrderType = OrderType.ASC;
            NodeFilter.Selects = NodeSelect.ALL;
            NodeFilter.Id = CityFreighter_NodeFilterDTO.Id;
            NodeFilter.Code = CityFreighter_NodeFilterDTO.Code;
            NodeFilter.Longtitude = CityFreighter_NodeFilterDTO.Longtitude;
            NodeFilter.Latitude = CityFreighter_NodeFilterDTO.Latitude;
            List<Node> Nodes = await NodeService.List(NodeFilter);
            List<CityFreighter_NodeDTO> CityFreighter_NodeDTOs = Nodes
                .Select(x => new CityFreighter_NodeDTO(x)).ToList();
            return CityFreighter_NodeDTOs;
        }
    }
}

