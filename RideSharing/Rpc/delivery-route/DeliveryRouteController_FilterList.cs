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
using RideSharing.Services.MDeliveryRoute;
using RideSharing.Services.MCityFreighter;

namespace RideSharing.Rpc.delivery_route
{
    public partial class DeliveryRouteController 
    {
        [Route(DeliveryRouteRoute.FilterListCityFreighter), HttpPost]
        public async Task<List<DeliveryRoute_CityFreighterDTO>> FilterListCityFreighter([FromBody] DeliveryRoute_CityFreighterFilterDTO DeliveryRoute_CityFreighterFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CityFreighterFilter CityFreighterFilter = new CityFreighterFilter();
            CityFreighterFilter.Skip = 0;
            CityFreighterFilter.Take = 20;
            CityFreighterFilter.OrderBy = CityFreighterOrder.Id;
            CityFreighterFilter.OrderType = OrderType.ASC;
            CityFreighterFilter.Selects = CityFreighterSelect.ALL;
            CityFreighterFilter.Id = DeliveryRoute_CityFreighterFilterDTO.Id;
            CityFreighterFilter.Name = DeliveryRoute_CityFreighterFilterDTO.Name;
            CityFreighterFilter.Capacity = DeliveryRoute_CityFreighterFilterDTO.Capacity;
            CityFreighterFilter.Latitude = DeliveryRoute_CityFreighterFilterDTO.Latitude;
            CityFreighterFilter.Longtitude = DeliveryRoute_CityFreighterFilterDTO.Longtitude;

            List<CityFreighter> CityFreighters = await CityFreighterService.List(CityFreighterFilter);
            List<DeliveryRoute_CityFreighterDTO> DeliveryRoute_CityFreighterDTOs = CityFreighters
                .Select(x => new DeliveryRoute_CityFreighterDTO(x)).ToList();
            return DeliveryRoute_CityFreighterDTOs;
        }
    }
}

