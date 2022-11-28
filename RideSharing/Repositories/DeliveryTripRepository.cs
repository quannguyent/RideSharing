using TrueSight.Common;
using RideSharing.Common;
using RideSharing.Helpers;
using RideSharing.Entities;
using RideSharing.Models;
using RideSharing.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace RideSharing.Repositories
{
    public interface IDeliveryTripRepository
    {
        Task<int> CountAll(DeliveryTripFilter DeliveryTripFilter);
        Task<int> Count(DeliveryTripFilter DeliveryTripFilter);
        Task<List<DeliveryTrip>> List(DeliveryTripFilter DeliveryTripFilter);
        Task<List<DeliveryTrip>> List(List<long> Ids);
        Task<DeliveryTrip> Get(long Id);
        Task<bool> Create(DeliveryTrip DeliveryTrip);
        Task<bool> Update(DeliveryTrip DeliveryTrip);
        Task<bool> Delete(DeliveryTrip DeliveryTrip);
        Task<List<long>> BulkMerge(List<DeliveryTrip> DeliveryTrips);
        Task<bool> BulkDelete(List<DeliveryTrip> DeliveryTrips);
    }
    public class DeliveryTripRepository : IDeliveryTripRepository
    {
        private DataContext DataContext;
        public DeliveryTripRepository(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        private async Task<IQueryable<DeliveryTripDAO>> DynamicFilter(IQueryable<DeliveryTripDAO> query, DeliveryTripFilter filter)
        {
            if (filter == null)
                return query.Where(q => false);
            query = query.Where(q => !q.DeletedAt.HasValue);
            query = query.Where(q => q.CreatedAt, filter.CreatedAt);
            query = query.Where(q => q.UpdatedAt, filter.UpdatedAt);
            query = query.Where(q => q.Id, filter.Id);
            query = query.Where(q => q.Path, filter.Path);
            query = query.Where(q => q.TravelDistance, filter.TravelDistance);
            query = query.Where(q => q.BusStopId, filter.BusStopId);
            query = query.Where(q => q.CityFreighterId, filter.CityFreighterId);
            return query;
        }

        private async Task<IQueryable<DeliveryTripDAO>> OrFilter(IQueryable<DeliveryTripDAO> query, DeliveryTripFilter filter)
        {
            if (filter.OrFilter == null || filter.OrFilter.Count == 0)
                return query;
            IQueryable<DeliveryTripDAO> initQuery = query.Where(q => false);
            foreach (DeliveryTripFilter DeliveryTripFilter in filter.OrFilter)
            {
                IQueryable<DeliveryTripDAO> queryable = query;
                queryable = queryable.Where(q => q.Id, DeliveryTripFilter.Id);
                queryable = queryable.Where(q => q.Path, DeliveryTripFilter.Path);
                queryable = queryable.Where(q => q.TravelDistance, DeliveryTripFilter.TravelDistance);
                queryable = queryable.Where(q => q.BusStopId, DeliveryTripFilter.BusStopId);
                queryable = queryable.Where(q => q.CityFreighterId, DeliveryTripFilter.CityFreighterId);
                initQuery = initQuery.Union(queryable);
            }
            return initQuery;
        }    

        private IQueryable<DeliveryTripDAO> DynamicOrder(IQueryable<DeliveryTripDAO> query, DeliveryTripFilter filter)
        {
            switch (filter.OrderType)
            {
                case OrderType.ASC:
                    switch (filter.OrderBy)
                    {
                        case DeliveryTripOrder.Id:
                            query = query.OrderBy(q => q.Id);
                            break;
                        case DeliveryTripOrder.Path:
                            query = query.OrderBy(q => q.Path);
                            break;
                        case DeliveryTripOrder.CityFreighter:
                            query = query.OrderBy(q => q.CityFreighterId);
                            break;
                        case DeliveryTripOrder.BusStop:
                            query = query.OrderBy(q => q.BusStopId);
                            break;
                        case DeliveryTripOrder.TravelDistance:
                            query = query.OrderBy(q => q.TravelDistance);
                            break;
                    }
                    break;
                case OrderType.DESC:
                    switch (filter.OrderBy)
                    {
                        case DeliveryTripOrder.Id:
                            query = query.OrderByDescending(q => q.Id);
                            break;
                        case DeliveryTripOrder.Path:
                            query = query.OrderByDescending(q => q.Path);
                            break;
                        case DeliveryTripOrder.CityFreighter:
                            query = query.OrderByDescending(q => q.CityFreighterId);
                            break;
                        case DeliveryTripOrder.BusStop:
                            query = query.OrderByDescending(q => q.BusStopId);
                            break;
                        case DeliveryTripOrder.TravelDistance:
                            query = query.OrderByDescending(q => q.TravelDistance);
                            break;
                    }
                    break;
            }
            query = query.Skip(filter.Skip).Take(filter.Take);
            return query;
        }

        private async Task<List<DeliveryTrip>> DynamicSelect(IQueryable<DeliveryTripDAO> query, DeliveryTripFilter filter)
        {
            List<DeliveryTrip> DeliveryTrips = await query.Select(q => new DeliveryTrip()
            {
                Id = filter.Selects.Contains(DeliveryTripSelect.Id) ? q.Id : default(long),
                Path = filter.Selects.Contains(DeliveryTripSelect.Path) ? q.Path : default(string),
                CityFreighterId = filter.Selects.Contains(DeliveryTripSelect.CityFreighter) ? q.CityFreighterId : default(long),
                BusStopId = filter.Selects.Contains(DeliveryTripSelect.BusStop) ? q.BusStopId : default(long),
                TravelDistance = filter.Selects.Contains(DeliveryTripSelect.TravelDistance) ? q.TravelDistance : default(decimal),
                BusStop = filter.Selects.Contains(DeliveryTripSelect.BusStop) && q.BusStop != null ? new BusStop
                {
                    Id = q.BusStop.Id,
                    Name = q.BusStop.Name,
                    NodeId = q.BusStop.NodeId,
                } : null,
                CityFreighter = filter.Selects.Contains(DeliveryTripSelect.CityFreighter) && q.CityFreighter != null ? new CityFreighter
                {
                    Id = q.CityFreighter.Id,
                    Name = q.CityFreighter.Name,
                    Capacity = q.CityFreighter.Capacity,
                    NodeId = q.CityFreighter.NodeId,
                } : null,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt,
                DeletedAt = q.DeletedAt,
            }).ToListAsync();
            return DeliveryTrips;
        }

        public async Task<int> CountAll(DeliveryTripFilter filter)
        {
            IQueryable<DeliveryTripDAO> DeliveryTripDAOs = DataContext.DeliveryTrip.AsNoTracking();
            DeliveryTripDAOs = await DynamicFilter(DeliveryTripDAOs, filter);
            return await DeliveryTripDAOs.CountAsync();
        }

        public async Task<int> Count(DeliveryTripFilter filter)
        {
            IQueryable<DeliveryTripDAO> DeliveryTripDAOs = DataContext.DeliveryTrip.AsNoTracking();
            DeliveryTripDAOs = await DynamicFilter(DeliveryTripDAOs, filter);
            DeliveryTripDAOs = await OrFilter(DeliveryTripDAOs, filter);
            return await DeliveryTripDAOs.CountAsync();
        }

        public async Task<List<DeliveryTrip>> List(DeliveryTripFilter filter)
        {
            if (filter == null) return new List<DeliveryTrip>();
            IQueryable<DeliveryTripDAO> DeliveryTripDAOs = DataContext.DeliveryTrip.AsNoTracking();
            DeliveryTripDAOs = await DynamicFilter(DeliveryTripDAOs, filter);
            DeliveryTripDAOs = await OrFilter(DeliveryTripDAOs, filter);
            DeliveryTripDAOs = DynamicOrder(DeliveryTripDAOs, filter);
            List<DeliveryTrip> DeliveryTrips = await DynamicSelect(DeliveryTripDAOs, filter);
            return DeliveryTrips;
        }

        public async Task<List<DeliveryTrip>> List(List<long> Ids)
        {
            IdFilter IdFilter = new IdFilter { In = Ids };

            IQueryable<DeliveryTripDAO> query = DataContext.DeliveryTrip.AsNoTracking();
            query = query.Where(q => q.Id, IdFilter);
            List<DeliveryTrip> DeliveryTrips = await query.AsNoTracking()
            .Select(x => new DeliveryTrip()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt,
                Id = x.Id,
                Path = x.Path,
                CityFreighterId = x.CityFreighterId,
                BusStopId = x.BusStopId,
                TravelDistance = x.TravelDistance,
                BusStop = x.BusStop == null ? null : new BusStop
                {
                    Id = x.BusStop.Id,
                    Name = x.BusStop.Name,
                    NodeId = x.BusStop.NodeId,
                },
                CityFreighter = x.CityFreighter == null ? null : new CityFreighter
                {
                    Id = x.CityFreighter.Id,
                    Name = x.CityFreighter.Name,
                    Capacity = x.CityFreighter.Capacity,
                    NodeId = x.CityFreighter.NodeId,
                },
            }).ToListAsync();
            

            return DeliveryTrips;
        }

        public async Task<DeliveryTrip> Get(long Id)
        {
            DeliveryTrip DeliveryTrip = await DataContext.DeliveryTrip.AsNoTracking()
            .Where(x => x.Id == Id)
            .Where(x => x.DeletedAt == null)
            .Select(x => new DeliveryTrip()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
                Path = x.Path,
                CityFreighterId = x.CityFreighterId,
                BusStopId = x.BusStopId,
                TravelDistance = x.TravelDistance,
                BusStop = x.BusStop == null ? null : new BusStop
                {
                    Id = x.BusStop.Id,
                    Name = x.BusStop.Name,
                    NodeId = x.BusStop.NodeId,
                },
                CityFreighter = x.CityFreighter == null ? null : new CityFreighter
                {
                    Id = x.CityFreighter.Id,
                    Name = x.CityFreighter.Name,
                    Capacity = x.CityFreighter.Capacity,
                    NodeId = x.CityFreighter.NodeId,
                },
            }).FirstOrDefaultAsync();

            if (DeliveryTrip == null)
                return null;

            return DeliveryTrip;
        }
        public async Task<bool> Create(DeliveryTrip DeliveryTrip)
        {
            DeliveryTripDAO DeliveryTripDAO = new DeliveryTripDAO();
            DeliveryTripDAO.Id = DeliveryTrip.Id;
            DeliveryTripDAO.Path = DeliveryTrip.Path;
            DeliveryTripDAO.CityFreighterId = DeliveryTrip.CityFreighterId;
            DeliveryTripDAO.BusStopId = DeliveryTrip.BusStopId;
            DeliveryTripDAO.TravelDistance = DeliveryTrip.TravelDistance;
            DeliveryTripDAO.CreatedAt = StaticParams.DateTimeNow;
            DeliveryTripDAO.UpdatedAt = StaticParams.DateTimeNow;
            DataContext.DeliveryTrip.Add(DeliveryTripDAO);
            await DataContext.SaveChangesAsync();
            DeliveryTrip.Id = DeliveryTripDAO.Id;
            await SaveReference(DeliveryTrip);
            return true;
        }

        public async Task<bool> Update(DeliveryTrip DeliveryTrip)
        {
            DeliveryTripDAO DeliveryTripDAO = DataContext.DeliveryTrip
                .Where(x => x.Id == DeliveryTrip.Id)
                .FirstOrDefault();
            if (DeliveryTripDAO == null)
                return false;
            DeliveryTripDAO.Id = DeliveryTrip.Id;
            DeliveryTripDAO.Path = DeliveryTrip.Path;
            DeliveryTripDAO.CityFreighterId = DeliveryTrip.CityFreighterId;
            DeliveryTripDAO.BusStopId = DeliveryTrip.BusStopId;
            DeliveryTripDAO.TravelDistance = DeliveryTrip.TravelDistance;
            DeliveryTripDAO.UpdatedAt = StaticParams.DateTimeNow;
            await DataContext.SaveChangesAsync();
            await SaveReference(DeliveryTrip);
            return true;
        }

        public async Task<bool> Delete(DeliveryTrip DeliveryTrip)
        {
            await DataContext.DeliveryTrip
                .Where(x => x.Id == DeliveryTrip.Id)
                .UpdateFromQueryAsync(x => new DeliveryTripDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        public async Task<List<long>> BulkMerge(List<DeliveryTrip> DeliveryTrips)
        {
            IdFilter IdFilter = new IdFilter { In = DeliveryTrips.Select(x => x.Id).ToList() };
            List<DeliveryTripDAO> Inserts = new List<DeliveryTripDAO>();
            List<DeliveryTripDAO> Updates = new List<DeliveryTripDAO>();
            List<DeliveryTripDAO> DbDeliveryTripDAOs = await DataContext.DeliveryTrip
                .Where(x => x.Id, IdFilter)
                .ToListAsync();
            foreach (DeliveryTrip DeliveryTrip in DeliveryTrips)
            {
                DeliveryTripDAO DeliveryTripDAO = DbDeliveryTripDAOs
                        .Where(x => x.Id == DeliveryTrip.Id)
                        .FirstOrDefault();
                if (DeliveryTripDAO == null)
                {
                    DeliveryTripDAO = new DeliveryTripDAO();
                    DeliveryTripDAO.CreatedAt = StaticParams.DateTimeNow;
                    Inserts.Add(DeliveryTripDAO);
                }
                else
                {
                    Updates.Add(DeliveryTripDAO);
                }
                DeliveryTripDAO.Path = DeliveryTrip.Path;
                DeliveryTripDAO.CityFreighterId = DeliveryTrip.CityFreighterId;
                DeliveryTripDAO.BusStopId = DeliveryTrip.BusStopId;
                DeliveryTripDAO.TravelDistance = DeliveryTrip.TravelDistance;
                DeliveryTripDAO.UpdatedAt = StaticParams.DateTimeNow;
            }
            await DataContext.DeliveryTrip.BulkInsertAsync(Inserts);
            await DataContext.DeliveryTrip.BulkMergeAsync(Updates);
            var Ids = Inserts.Select(x => x.Id).ToList();
            Ids.AddRange(Updates.Select(x => x.Id));
            Ids = Ids.Distinct().ToList();
            return Ids;
        }
        
        public async Task<bool> BulkDelete(List<DeliveryTrip> DeliveryTrips)
        {
            List<long> Ids = DeliveryTrips.Select(x => x.Id).ToList();
            await DataContext.DeliveryTrip
                .WhereBulkContains(Ids, x => x.Id)
                .UpdateFromQueryAsync(x => new DeliveryTripDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        private async Task SaveReference(DeliveryTrip DeliveryTrip)
        {
        }

    }
}
