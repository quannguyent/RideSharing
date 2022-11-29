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
                deliveryRoute.CityFreighterId = CityFreighter.Id;
                deliveryRoute.CityFreighter = CityFreighter;
                deliveryRoute.DeliveryOrders = new List<DeliveryOrder>();
                deliveryRoute.DeliveryTrips = new List<DeliveryTrip>();
                BuildPath(deliveryRoute);
                DeliveryRoutes.Add(deliveryRoute);
            }
            foreach (DeliveryOrder DeliveryOrder in DeliverySchedule.DeliveryOrders)
            {
                DeliveryRoutes = BestInsertion(DeliveryOrder, DeliveryRoutes, DeliverySchedule.BusStops, DeliverySchedule.Config.FreighterQuotientCost);
            }

            DeliverySchedule.DeliveryRoutes = DeliveryRoutes;
            return DeliverySchedule;
        }

        public List<DeliveryRoute> BestInsertion(DeliveryOrder DeliveryOrder, List<DeliveryRoute> DeliveryRoutes, List<BusStop> BusStops, decimal FreighterQuotientCost)
        {
            decimal MinimumCost = int.MaxValue;
            var BestInsertion = new List<DeliveryRoute>();

            foreach (DeliveryRoute DeliveryRoute in DeliveryRoutes)
            {
                var DeliveryOrderNode = new Node(DeliveryOrder.Customer.Latitude, DeliveryOrder.Customer.Longtitude);

                foreach (var BusStop in BusStops)
                {
                    //Insert to initial deport
                    var BusStopNode = new Node(BusStop.Latitude, BusStop.Longtitude);

                    var preDeliveryRoutes = DeliveryRoutes.ToList();
                    DeliveryRoute preDeliveryRoute = preDeliveryRoutes
                        .Where(x => x.CityFreighterId == DeliveryRoute.CityFreighterId)
                        .Select(x => new DeliveryRoute(x))
                        .FirstOrDefault();

                    DeliveryTrip DeliveryTrip = new DeliveryTrip
                    {
                        BusStopId = BusStop.Id,
                        BusStop = BusStop,
                        PlannedNode = new List<Node>() {
                            BusStopNode,
                            DeliveryOrderNode
                        },
                        DeliveryOrders = new List<DeliveryOrder>() { DeliveryOrder },
                    };

                    var insertIndex = 0;
                    preDeliveryRoute.DeliveryTrips.Insert(insertIndex, DeliveryTrip);
                    BuildPath(preDeliveryRoute);

                    preDeliveryRoutes.Remove(DeliveryRoute);
                    preDeliveryRoutes.Add(preDeliveryRoute);

                    var TotalCost = EvaluateTotalCost(preDeliveryRoutes, FreighterQuotientCost);
                    if (TotalCost < MinimumCost)
                    {
                        MinimumCost = TotalCost;
                        BestInsertion = preDeliveryRoutes;
                    }
                }

                //Insert to existed trip
                foreach (DeliveryTrip DeliveryTrip in DeliveryRoute.DeliveryTrips)
                {
                    var inDeliveryRoutes = DeliveryRoutes.ToList();
                    var inDeliveryRoute = inDeliveryRoutes
                        .Where(x => x.CityFreighterId == DeliveryRoute.CityFreighterId)
                        .Select(x => new DeliveryRoute(x))
                        .FirstOrDefault();

                    decimal minimumTripDistance = int.MaxValue;
                    var minimumDeliveryTrip = inDeliveryRoute.DeliveryTrips.Where(x => x.Equals(DeliveryTrip)).FirstOrDefault();

                    foreach (Node Node in DeliveryTrip.PlannedNode)
                    {
                        var inDeliveryTrip = inDeliveryRoute.DeliveryTrips
                            .Where(x => x.Equals(DeliveryTrip))
                            .Select(x => new DeliveryTrip(x))
                            .FirstOrDefault();

                        var inPlannedNode = inDeliveryTrip.PlannedNode.ToList();
                        var insertIndex = inPlannedNode.IndexOf(Node);
                        inPlannedNode.Insert(insertIndex, DeliveryOrderNode);

                        inDeliveryTrip.PlannedNode = inPlannedNode;
                        inDeliveryTrip.DeliveryOrders.Add(DeliveryOrder);
                        BuildPath(inDeliveryTrip);

                        if (inDeliveryTrip.TravelDistance < minimumTripDistance)
                        {
                            minimumTripDistance = inDeliveryTrip.TravelDistance;
                            minimumDeliveryTrip = inDeliveryTrip;
                        }
                    }

                    BuildPath(DeliveryRoute);
                    var TotalCost = EvaluateTotalCost(inDeliveryRoutes, FreighterQuotientCost);
                    if (TotalCost < MinimumCost)
                    {
                        MinimumCost = TotalCost; ;
                        BestInsertion = inDeliveryRoutes;
                    }
                }
            }

            ////Insert to last deport
            //foreach (DeliveryRoute DeliveryRoute in DeliveryRoutes)
            //{
            //    var newDeliveryRoutes = DeliveryRoutes.ToList();
            //    newDeliveryRoutes.Remove(DeliveryRoute);
            //    newDeliveryRoutes.Add(InsertNewTrip(DeliveryOrder, DeliveryRoute, BusStops));
            //    var cost = EvaluateTotalCost(newDeliveryRoutes, FreighterQuotientCost);
            //    if (cost < MinimumCost)
            //    {
            //        MinimumCost = cost;
            //        BestInsertion = newDeliveryRoutes;
            //    }
            //}
            return BestInsertion;
        }

        public decimal EvaluateTotalCost(List<DeliveryRoute> DeliveryRoutes, decimal FreighterQuotientCost)
        {
            var TotalTravelDistance = DeliveryRoutes.Sum(x => x.TotalTravelDistance);
            var CityFreighterCount = DeliveryRoutes.Where(x => x.TotalTravelDistance > 0).Select(x => x.CityFreighterId).Distinct().Count();
            var TotalCost = TotalTravelDistance + (CityFreighterCount * (CityFreighterCount - 1) * FreighterQuotientCost / 2);
            return TotalCost;
        }

        private void BuildPath(DeliveryTrip DeliveryTrip)
        {
            Node previousNode = new Node(DeliveryTrip.BusStop.Latitude, DeliveryTrip.BusStop.Longtitude);
            string Path = $"({DeliveryTrip.BusStop.Latitude}, {DeliveryTrip.BusStop.Longtitude})";
            decimal TravelDistance = 0;
            foreach (Node Node in DeliveryTrip.PlannedNode)
            {
                Path += $"-> ({Node.Latitude}, {Node.Longtitude})";
                TravelDistance += StaticParams.CalculateDistance(previousNode.Latitude, previousNode.Longtitude,
                    Node.Latitude, Node.Longtitude);
                previousNode = Node;
            }
            DeliveryTrip.Path = Path;
            DeliveryTrip.TravelDistance = TravelDistance;
            return;
        }

        private void BuildPath(DeliveryRoute DeliveryRoute)
        {
            DeliveryRoute.DeliveryTrips.ForEach(x => BuildPath(x));
            var Path = $"({DeliveryRoute.CityFreighter.Latitude}, {DeliveryRoute.CityFreighter.Longtitude})";
            var previousNode = new Node(DeliveryRoute.CityFreighter.Latitude, DeliveryRoute.CityFreighter.Longtitude);
            decimal TravelDistance = 0;
            decimal TotalEmptyRun = 0;

            foreach (var trip in DeliveryRoute.DeliveryTrips)
            {
                Path += $"-> {trip.Path}";
                decimal DistanceFromPrevious = StaticParams.CalculateDistance(previousNode.Latitude, previousNode.Longtitude,
                    trip.PlannedNode.FirstOrDefault().Latitude, trip.PlannedNode.FirstOrDefault().Longtitude);
                TotalEmptyRun += DistanceFromPrevious;
                TravelDistance += DistanceFromPrevious + trip.TravelDistance;
                previousNode = trip.PlannedNode.LastOrDefault();
            }
            Path += $"({DeliveryRoute.CityFreighter.Latitude}, {DeliveryRoute.CityFreighter.Longtitude})";
            TravelDistance += StaticParams.CalculateDistance(previousNode.Latitude, previousNode.Longtitude,
                    DeliveryRoute.CityFreighter.Latitude, DeliveryRoute.CityFreighter.Longtitude);

            DeliveryRoute.Path = Path;
            DeliveryRoute.TotalTravelDistance = TravelDistance;
            DeliveryRoute.TotalEmptyRunDistance = TotalEmptyRun;
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

            var Customers = await UOW.CustomerRepository.List(new CustomerFilter
            {
                Take = 20,
                Skip = 0,
                Selects = CustomerSelect.ALL,
            });

            DeliverySchedule.DeliveryOrders = Customers.Select(x => new DeliveryOrder
            {
                Code = $"Order_{x.Code}",
                Weight = 2,
                CustomerId = x.Id,
                Customer = x,
            }).ToList();

            InitRoute(DeliverySchedule);
            EvaluateDeliverySchedule(DeliverySchedule);
            return DeliverySchedule;
        }

        private void EvaluateDeliverySchedule(DeliverySchedule DeliverySchedule)
        {
            DeliverySchedule.TotalCost = EvaluateTotalCost(DeliverySchedule.DeliveryRoutes, DeliverySchedule.Config.FreighterQuotientCost);
            DeliverySchedule.TotalTravelDistance = DeliverySchedule.DeliveryRoutes.Sum(x => x.TotalTravelDistance);
            DeliverySchedule.TotalEmptyRun = DeliverySchedule.DeliveryRoutes.Sum(x => x.TotalEmptyRunDistance);
            DeliverySchedule.NumberOfFreigther = DeliverySchedule.DeliveryRoutes.Where(x => x.TotalTravelDistance > 0).Select(x => x.CityFreighterId).Distinct().Count();
            DeliverySchedule.NumberOfTrip = DeliverySchedule.DeliveryRoutes.Sum(x => x.DeliveryTrips.Count());

            List<DeliveryTrip> DeliveryTrips = DeliverySchedule.DeliveryRoutes.SelectMany(x => x.DeliveryTrips).ToList();
            foreach (BusStop BusStop in DeliverySchedule.BusStops)
            {
                BusStop.NumberOfUsed = DeliveryTrips.Count(x => x.BusStopId == BusStop.Id);
            }
            foreach (CityFreighter CityFreighter in DeliverySchedule.CityFreighters)
            {
                CityFreighter.TotalTravelDistance = DeliverySchedule.DeliveryRoutes
                    .Where(x => x.CityFreighterId == CityFreighter.Id)
                    .Sum(x => x.TotalTravelDistance);
                CityFreighter.TotalEmptyRun = DeliverySchedule.DeliveryRoutes
                    .Where(x => x.CityFreighterId == CityFreighter.Id)
                    .Sum(x => x.TotalEmptyRunDistance);
            }
        }
    }
}
