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
using RideSharing.Services.MDeliveryTrip;
using RideSharing.Services.MBusStop;
using RideSharing.Services.MCityFreighter;

namespace RideSharing.Rpc.delivery_trip
{
    public partial class DeliveryTripController 
    {
        [Route(DeliveryTripRoute.FilterListBusStop), HttpPost]
        public async Task<List<DeliveryTrip_BusStopDTO>> FilterListBusStop([FromBody] DeliveryTrip_BusStopFilterDTO DeliveryTrip_BusStopFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            BusStopFilter BusStopFilter = new BusStopFilter();
            BusStopFilter.Skip = 0;
            BusStopFilter.Take = 20;
            BusStopFilter.OrderBy = BusStopOrder.Id;
            BusStopFilter.OrderType = OrderType.ASC;
            BusStopFilter.Selects = BusStopSelect.ALL;
            BusStopFilter.Id = DeliveryTrip_BusStopFilterDTO.Id;
            BusStopFilter.Name = DeliveryTrip_BusStopFilterDTO.Name;
            BusStopFilter.Latitude = DeliveryTrip_BusStopFilterDTO.Latitude;
            BusStopFilter.Longtitude = DeliveryTrip_BusStopFilterDTO.Longtitude;

            List<BusStop> BusStops = await BusStopService.List(BusStopFilter);
            List<DeliveryTrip_BusStopDTO> DeliveryTrip_BusStopDTOs = BusStops
                .Select(x => new DeliveryTrip_BusStopDTO(x)).ToList();
            return DeliveryTrip_BusStopDTOs;
        }
        [Route(DeliveryTripRoute.FilterListCityFreighter), HttpPost]
        public async Task<List<DeliveryTrip_CityFreighterDTO>> FilterListCityFreighter([FromBody] DeliveryTrip_CityFreighterFilterDTO DeliveryTrip_CityFreighterFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CityFreighterFilter CityFreighterFilter = new CityFreighterFilter();
            CityFreighterFilter.Skip = 0;
            CityFreighterFilter.Take = 20;
            CityFreighterFilter.OrderBy = CityFreighterOrder.Id;
            CityFreighterFilter.OrderType = OrderType.ASC;
            CityFreighterFilter.Selects = CityFreighterSelect.ALL;
            CityFreighterFilter.Id = DeliveryTrip_CityFreighterFilterDTO.Id;
            CityFreighterFilter.Name = DeliveryTrip_CityFreighterFilterDTO.Name;
            CityFreighterFilter.Capacity = DeliveryTrip_CityFreighterFilterDTO.Capacity;
            CityFreighterFilter.Latitude = DeliveryTrip_CityFreighterFilterDTO.Latitude;
            CityFreighterFilter.Longtitude = DeliveryTrip_CityFreighterFilterDTO.Longtitude;

            List<CityFreighter> CityFreighters = await CityFreighterService.List(CityFreighterFilter);
            List<DeliveryTrip_CityFreighterDTO> DeliveryTrip_CityFreighterDTOs = CityFreighters
                .Select(x => new DeliveryTrip_CityFreighterDTO(x)).ToList();
            return DeliveryTrip_CityFreighterDTOs;
        }
    }
}

