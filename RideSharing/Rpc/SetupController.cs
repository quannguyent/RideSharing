using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideSharing.Common;
using RideSharing.Entities;
using RideSharing.Enums;
using RideSharing.Handlers.Configuration;
using RideSharing.Helpers;
using RideSharing.Models;
using RideSharing.Repositories;
using RideSharing.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TrueSight.Common;
using TrueSight.PER;
using TrueSight.PER.Entities;
using Action = TrueSight.PER.Entities.Action;
using Role = TrueSight.PER.Entities.Role;
using Site = TrueSight.PER.Entities.Site;

namespace RideSharing.Rpc
{
    public partial class SetupController : ControllerBase
    {
        private DataContext DataContext;
        private IRabbitManager RabbitManager;
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        public SetupController(
            DataContext DataContext,
            IRabbitManager RabbitManager,
            IUOW UOW,
            ICurrentContext CurrentContext
            )
        {
            this.DataContext = DataContext;
            this.RabbitManager = RabbitManager;
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
        }

        [HttpGet, Route("rpc/ride-sharing/setup/init-data")]
        public async Task<ActionResult> InitData()
        {
            List<NodeDAO> NodeDAOs = await DataContext.Node.ToListAsync();

            List<BusStop> BusStops = NodeDAOs.Where(x => x.Code.StartsWith("T") && x.Code.Length == 2).Select(x => new BusStop
            {
                Name = x.Code,
                Latitude = x.Latitude,
                Longtitude = x.Longtitude,
            }).ToList();
            await UOW.BusStopRepository.BulkMerge(BusStops);

            NodeDAOs = NodeDAOs.Where(x => StaticParams.CalculateDistance(BusStops[3].Latitude, BusStops[3].Longtitude, x.Latitude, x.Longtitude) < 2).ToList(); ;

            List<CityFreighter> CityFreighter = NodeDAOs.Take(10).Select(x => new CityFreighter
            {
                Name = x.Code,
                Capacity = 10,
                Latitude = x.Latitude,
                Longtitude = x.Longtitude,
            }).ToList();
            await UOW.CityFreighterRepository.BulkMerge(CityFreighter);

            List<Customer> Customer = NodeDAOs.Skip(10).Select(x => new Customer
            {
                Code = x.Code,
                Name = x.Code,
                Latitude = x.Latitude,
                Longtitude = x.Longtitude,
            }).ToList();
            await UOW.CustomerRepository.BulkMerge(Customer);

            return Ok();
        }


        [HttpGet, Route("rpc/ride-sharing/setup/init")]
        public async Task<ActionResult> Init()
        {
            var NodeDAO = await DataContext.Node.ToListAsync();
            NodeDAO.ForEach(x =>
            {
                var temp = x.Latitude;
                x.Latitude = x.Longtitude;
                x.Longtitude = temp;
            });
            await DataContext.BulkMergeAsync(NodeDAO);

            return Ok();
        }


        private async Task InitEnum()
        {
        }
    }
}
