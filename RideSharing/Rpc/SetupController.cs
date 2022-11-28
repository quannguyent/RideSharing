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

        [HttpGet, Route("rpc/RideSharing/setup/init-data")]
        public async Task<ActionResult> InitData()
        {
            List<NodeDAO> NodeDAOs = await DataContext.Node.ToListAsync();

            List<BusStop> BusStops = NodeDAOs.Where(x => x.Code.StartsWith("T") && x.Code.Length == 2).Select(x => new BusStop
            {
                Name = x.Code,
                NodeId = x.Id,
                Node = new Node
                {
                    Id = x.Id,
                    Latitude = x.Latitude,
                    Longtitude = x.Longtitude,
                },
            }).ToList();
            await UOW.BusStopRepository.BulkMerge(BusStops);

            NodeDAOs = NodeDAOs.Where(x => CalculateDistance(BusStops[3].Node, new Node { Latitude = x.Latitude, Longtitude = x.Longtitude }) < 2).ToList(); ;

            List<CityFreighter> CityFreighter = NodeDAOs.Take(10).Select(x => new CityFreighter
            {
                Name = x.Code,
                NodeId = x.Id,
            }).ToList();
            await UOW.CityFreighterRepository.BulkMerge(CityFreighter);

            List<Customer> Customer = NodeDAOs.Skip(10).Select(x => new Customer
            {
                Code = x.Code,
                Name = x.Code,
                NodeId = x.Id,
            }).ToList();
            await UOW.CustomerRepository.BulkMerge(Customer);

            return Ok();
        }

        [HttpGet, Route("rpc/RideSharing/setup/init")]
        public async Task<ActionResult> Init()
        {
            await InitEnum();
            return Ok();
        }


        private async Task InitEnum()
        {
        }
        private decimal CalculateDistance(Node start, Node destination)
        {
            return StaticParams.CalculateDistance(start.Latitude, start.Longtitude, destination.Latitude, destination.Longtitude);
        }
    }
}
