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
    public partial class DeliveryScheduleController : ControllerBase
    {
        private DataContext DataContext;
        private IRabbitManager RabbitManager;
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private IDeliveryScheduleService DeliveryScheduleService;
        public DeliveryScheduleController(
            DataContext DataContext,
            IRabbitManager RabbitManager,
            IUOW UOW,
            ICurrentContext CurrentContext,
            IDeliveryScheduleService DeliveryScheduleService
            )
        {
            this.DataContext = DataContext;
            this.RabbitManager = RabbitManager;
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.DeliveryScheduleService = DeliveryScheduleService;
        }

        [HttpPost, Route("rpc/ride-sharing/delivery-schedule/schedule")]
        public async Task<DeliverySchedule> Schedule([FromBody] DeliverySchedule DeliverySchedule)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliverySchedule = await DeliveryScheduleService.GetDeliverySchedule(DeliverySchedule);
            return DeliverySchedule;
        }
        [HttpPost, Route("rpc/ride-sharing/delivery-schedule/generate-delivery-order")]
        public async Task<DeliverySchedule> GenerateDeliveryOrder([FromBody] DeliverySchedule DeliverySchedule)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliverySchedule = await DeliveryScheduleService.GetDeliverySchedule(DeliverySchedule);
            return DeliverySchedule;
        }
    }
}
