using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideSharing.Common;
using RideSharing.Entities;
using RideSharing.Enums;
using RideSharing.Handlers.Configuration;
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

        [HttpGet, Route("rpc/RideSharing/setup/init-customer")]
        public async Task<ActionResult> InitCustomer()
        {
            await InitEnum();
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
    }
}
