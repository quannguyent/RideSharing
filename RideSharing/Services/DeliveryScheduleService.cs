using TrueSight.Common;
using RideSharing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RideSharing.Entities;
using RideSharing.Services.MDeliveryRoute;
using RideSharing.Handlers.Configuration;
using RideSharing.Repositories;
using RideSharing.Services.MDeliveryTrip;
using RideSharing.Helpers;

namespace RideSharing.Services
{
    public interface IDeliveryScheduleService : IServiceScoped
    {
        Task<DeliverySchedule> GetDeliverySchedule(DeliverySchedule DeliverySchedule);
    }
    public class DeliveryScheduleService : IDeliveryScheduleService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ICurrentContext CurrentContext;
        private IDeliveryRouteService DeliveryRouteService;
        public DeliveryScheduleService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            IDeliveryRouteService DeliveryRouteService)
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.DeliveryRouteService = DeliveryRouteService;
        }

        public DeliverySchedule InitRoute(DeliverySchedule DeliverySchedule)
        {
            List<DeliveryRoute> DeliveryRoutes = new List<DeliveryRoute>();
            foreach (CityFreighter CityFreighter in DeliverySchedule.CityFreighters)
            {
                DeliveryRoute deliveryRoute = new DeliveryRoute();
                deliveryRoute.CityFreighter = CityFreighter;
                deliveryRoute.DeliveryOrders = new List<DeliveryOrder>();
                deliveryRoute.DeliveryTrips = new List<DeliveryTrip>();
                deliveryRoute.BuildPath();
                DeliveryRoutes.Add(deliveryRoute);
            }
            foreach (DeliveryOrder DeliveryOrder in DeliverySchedule.DeliveryOrders)
            {
                DeliveryRoutes = BestInsertion(DeliveryOrder, DeliverySchedule.DeliveryRoutes, DeliverySchedule.BusStops, DeliverySchedule.Config.FreighterQuotientCost);
            }

            DeliverySchedule.DeliveryRoutes = DeliveryRoutes;
            return DeliverySchedule;
        }

        //public bool Repair(List<DeliveryOrder> DeliveryOrders)
        //{
        //    BestInsertion(DeliveryOrders);
        //    return true;
        //}

        public List<DeliveryRoute> BestInsertion(DeliveryOrder DeliveryOrder, List<DeliveryRoute> DeliveryRoutes, List<BusStop> BusStops, decimal FreighterQuotientCost)
        {
            decimal MinimumCost = int.MaxValue;
            var BestInsertion = new List<DeliveryRoute>();

            //Insert initial deport
            foreach (DeliveryRoute DeliveryRoute in DeliveryRoutes)
            {
                var newDeliveryRoutes = DeliveryRoutes;
                newDeliveryRoutes.Remove(DeliveryRoute);
                newDeliveryRoutes.Add(InsertNewTrip(DeliveryOrder, DeliveryRoute, BusStops, true));
                var cost = EvaluateTotalCost(newDeliveryRoutes, FreighterQuotientCost);
                if (cost < MinimumCost)
                {
                    MinimumCost = cost;
                    BestInsertion = newDeliveryRoutes;
                }
            }

            ////Insert to existed trip
            //foreach (DeliveryRoute DeliveryRoute in DeliveryRoutes)
            //{
            //    var newDeliveryRoutes = DeliveryRoutes;
            //    newDeliveryRoutes.Remove(DeliveryRoute);
            //    newDeliveryRoutes.Add(InsertNewTrip(DeliveryOrder, DeliveryRoute, BusStops));
            //    var cost = EvaluateTotalCost(newDeliveryRoutes);
            //    if (cost < MinimumCost)
            //    {
            //        MinimumCost = cost;
            //        BestInsertion = newDeliveryRoutes;
            //    }
            //}

            //Insert to last deport
            foreach (DeliveryRoute DeliveryRoute in DeliveryRoutes)
            {
                var newDeliveryRoutes = DeliveryRoutes;
                newDeliveryRoutes.Remove(DeliveryRoute);
                newDeliveryRoutes.Add(InsertNewTrip(DeliveryOrder, DeliveryRoute, BusStops));
                var cost = EvaluateTotalCost(newDeliveryRoutes, FreighterQuotientCost);
                if (cost < MinimumCost)
                {
                    MinimumCost = cost;
                    BestInsertion = newDeliveryRoutes;
                }
            }
            return DeliveryRoutes;
        }

        public decimal EvaluateTotalCost(List<DeliveryRoute> DeliveryRoutes, decimal FreighterQuotientCost)
        {
            var TotalTravelDistance = DeliveryRoutes.Sum(x => x.TotalTravelDistance);
            var CityFreighterCount = DeliveryRoutes.Select(x => x.CityFreighterId).Distinct().Count();
            var TotalCost = CityFreighterCount * (CityFreighterCount * (CityFreighterCount - 1) * FreighterQuotientCost / 2);
            return TotalCost;
        }
        public decimal CalculateDistance(Node start, Node destination)
        {
            return StaticParams.CalculateDistance(start.Latitude, start.Longtitude, destination.Latitude, destination.Longtitude);
        }
        public decimal CalculateTotalDistance(DeliveryRoute DeliveryRoute)
        {
            var LastLocation = new Node(DeliveryRoute.CityFreighter.Latitude, DeliveryRoute.CityFreighter.Longtitude);
            foreach (DeliveryTrip DeliveryTrip in DeliveryRoute.DeliveryTrips)
            {
                var FirstNode = DeliveryTrip.PlannedNode.FirstOrDefault();
                var Distance = StaticParams.CalculateDistance(LastLocation.Latitude, LastLocation.Latitude,
                    FirstNode.Latitude, FirstNode.Longtitude);
                DeliveryRoute.TotalEmptyRunDistance += Distance;
                DeliveryRoute.TotalTravelDistance += Distance;
                LastLocation = DeliveryTrip.PlannedNode.LastOrDefault();
            }
            return DeliveryRoute.TotalTravelDistance;
        }

        public DeliveryRoute InsertNewTrip(DeliveryOrder DeliveryOrder, DeliveryRoute DeliveryRoute, List<BusStop> BusStops, bool toFirst = false)
        {
            decimal BestTotalTravelDistance = 0;
            var BestInsertion = new DeliveryRoute();

            foreach (BusStop BusStop in BusStops)
            {
                var newDeliveryRoute = DeliveryRoute;
                var Distance = StaticParams.CalculateDistance(BusStop.Latitude, BusStop.Latitude,
                    DeliveryOrder.Customer.Latitude, DeliveryOrder.Customer.Longtitude);
                DeliveryTrip DeliveryTrip = new DeliveryTrip
                {
                    BusStopId = BusStop.Id,
                    BusStop = BusStop,
                    PlannedNode = new List<Node>() {
                        new Node(BusStop.Latitude, BusStop.Latitude),
                        new Node(DeliveryOrder.Customer.Latitude, DeliveryOrder.Customer.Longtitude)
                    },
                    DeliveryOrders = new List<DeliveryOrder>() { DeliveryOrder },
                    PlannedRoute = new List<Edge>()
                    {
                        new Edge
                        {
                            Source = new Node(BusStop.Latitude, BusStop.Latitude),
                            Destination = new Node(DeliveryOrder.Customer.Latitude, DeliveryOrder.Customer.Longtitude),
                            Distance = Distance,
                        }
                    },
                    TravelDistance = Distance,
                };
                DeliveryTrip.BuildPath();
                if (toFirst)
                {
                    newDeliveryRoute.DeliveryTrips.Insert(0, DeliveryTrip);
                }
                newDeliveryRoute.DeliveryTrips.Add(DeliveryTrip);

                var totalTravelDistance = CalculateTotalDistance(DeliveryRoute);
                if (totalTravelDistance >= BestTotalTravelDistance)
                {
                    BestTotalTravelDistance = totalTravelDistance;
                    BestInsertion = newDeliveryRoute;
                }

            }
            return BestInsertion;
        }

        public async Task<DeliverySchedule> GetDeliverySchedule(DeliverySchedule DeliverySchedule)
        {
            DeliverySchedule.Config = await UOW.SystemConfigRepository.Get(1);
            DeliverySchedule.BusStops = await UOW.BusStopRepository.List(new BusStopFilter
            {
                Take = int.MaxValue,
                Skip = 0,
                Selects = BusStopSelect.ALL,
            });
            DeliverySchedule.CityFreighters = await UOW.CityFreighterRepository.List(new CityFreighterFilter
            {
                Take = int.MaxValue,
                Skip = 0,
                Selects = CityFreighterSelect.ALL,
            });

            InitRoute(DeliverySchedule);

            return DeliverySchedule;
        }
    }
}
