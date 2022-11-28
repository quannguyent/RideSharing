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
    public interface IDeliveryRouteRepository
    {
        Task<int> CountAll(DeliveryRouteFilter DeliveryRouteFilter);
        Task<int> Count(DeliveryRouteFilter DeliveryRouteFilter);
        Task<List<DeliveryRoute>> List(DeliveryRouteFilter DeliveryRouteFilter);
        Task<List<DeliveryRoute>> List(List<long> Ids);
        Task<DeliveryRoute> Get(long Id);
        Task<bool> Create(DeliveryRoute DeliveryRoute);
        Task<bool> Update(DeliveryRoute DeliveryRoute);
        Task<bool> Delete(DeliveryRoute DeliveryRoute);
        Task<List<long>> BulkMerge(List<DeliveryRoute> DeliveryRoutes);
        Task<bool> BulkDelete(List<DeliveryRoute> DeliveryRoutes);
    }
    public class DeliveryRouteRepository : IDeliveryRouteRepository
    {
        private DataContext DataContext;
        public DeliveryRouteRepository(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        private async Task<IQueryable<DeliveryRouteDAO>> DynamicFilter(IQueryable<DeliveryRouteDAO> query, DeliveryRouteFilter filter)
        {
            if (filter == null)
                return query.Where(q => false);
            query = query.Where(q => !q.DeletedAt.HasValue);
            query = query.Where(q => q.CreatedAt, filter.CreatedAt);
            query = query.Where(q => q.UpdatedAt, filter.UpdatedAt);
            query = query.Where(q => q.Id, filter.Id);
            query = query.Where(q => q.Path, filter.Path);
            query = query.Where(q => q.TotalTravelDistance, filter.TotalTravelDistance);
            query = query.Where(q => q.TotalEmptyRunDistance, filter.TotalEmptyRunDistance);
            query = query.Where(q => q.CityFreighterId, filter.CityFreighterId);
            return query;
        }

        private async Task<IQueryable<DeliveryRouteDAO>> OrFilter(IQueryable<DeliveryRouteDAO> query, DeliveryRouteFilter filter)
        {
            if (filter.OrFilter == null || filter.OrFilter.Count == 0)
                return query;
            IQueryable<DeliveryRouteDAO> initQuery = query.Where(q => false);
            foreach (DeliveryRouteFilter DeliveryRouteFilter in filter.OrFilter)
            {
                IQueryable<DeliveryRouteDAO> queryable = query;
                queryable = queryable.Where(q => q.Id, DeliveryRouteFilter.Id);
                queryable = queryable.Where(q => q.Path, DeliveryRouteFilter.Path);
                queryable = queryable.Where(q => q.TotalTravelDistance, DeliveryRouteFilter.TotalTravelDistance);
                queryable = queryable.Where(q => q.TotalEmptyRunDistance, DeliveryRouteFilter.TotalEmptyRunDistance);
                queryable = queryable.Where(q => q.CityFreighterId, DeliveryRouteFilter.CityFreighterId);
                initQuery = initQuery.Union(queryable);
            }
            return initQuery;
        }    

        private IQueryable<DeliveryRouteDAO> DynamicOrder(IQueryable<DeliveryRouteDAO> query, DeliveryRouteFilter filter)
        {
            switch (filter.OrderType)
            {
                case OrderType.ASC:
                    switch (filter.OrderBy)
                    {
                        case DeliveryRouteOrder.Id:
                            query = query.OrderBy(q => q.Id);
                            break;
                        case DeliveryRouteOrder.Path:
                            query = query.OrderBy(q => q.Path);
                            break;
                        case DeliveryRouteOrder.CityFreighter:
                            query = query.OrderBy(q => q.CityFreighterId);
                            break;
                        case DeliveryRouteOrder.TotalTravelDistance:
                            query = query.OrderBy(q => q.TotalTravelDistance);
                            break;
                        case DeliveryRouteOrder.TotalEmptyRunDistance:
                            query = query.OrderBy(q => q.TotalEmptyRunDistance);
                            break;
                    }
                    break;
                case OrderType.DESC:
                    switch (filter.OrderBy)
                    {
                        case DeliveryRouteOrder.Id:
                            query = query.OrderByDescending(q => q.Id);
                            break;
                        case DeliveryRouteOrder.Path:
                            query = query.OrderByDescending(q => q.Path);
                            break;
                        case DeliveryRouteOrder.CityFreighter:
                            query = query.OrderByDescending(q => q.CityFreighterId);
                            break;
                        case DeliveryRouteOrder.TotalTravelDistance:
                            query = query.OrderByDescending(q => q.TotalTravelDistance);
                            break;
                        case DeliveryRouteOrder.TotalEmptyRunDistance:
                            query = query.OrderByDescending(q => q.TotalEmptyRunDistance);
                            break;
                    }
                    break;
            }
            query = query.Skip(filter.Skip).Take(filter.Take);
            return query;
        }

        private async Task<List<DeliveryRoute>> DynamicSelect(IQueryable<DeliveryRouteDAO> query, DeliveryRouteFilter filter)
        {
            List<DeliveryRoute> DeliveryRoutes = await query.Select(q => new DeliveryRoute()
            {
                Id = filter.Selects.Contains(DeliveryRouteSelect.Id) ? q.Id : default(long),
                Path = filter.Selects.Contains(DeliveryRouteSelect.Path) ? q.Path : default(string),
                CityFreighterId = filter.Selects.Contains(DeliveryRouteSelect.CityFreighter) ? q.CityFreighterId : default(long),
                TotalTravelDistance = filter.Selects.Contains(DeliveryRouteSelect.TotalTravelDistance) ? q.TotalTravelDistance : default(decimal),
                TotalEmptyRunDistance = filter.Selects.Contains(DeliveryRouteSelect.TotalEmptyRunDistance) ? q.TotalEmptyRunDistance : default(decimal),
                CityFreighter = filter.Selects.Contains(DeliveryRouteSelect.CityFreighter) && q.CityFreighter != null ? new CityFreighter
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
            return DeliveryRoutes;
        }

        public async Task<int> CountAll(DeliveryRouteFilter filter)
        {
            IQueryable<DeliveryRouteDAO> DeliveryRouteDAOs = DataContext.DeliveryRoute.AsNoTracking();
            DeliveryRouteDAOs = await DynamicFilter(DeliveryRouteDAOs, filter);
            return await DeliveryRouteDAOs.CountAsync();
        }

        public async Task<int> Count(DeliveryRouteFilter filter)
        {
            IQueryable<DeliveryRouteDAO> DeliveryRouteDAOs = DataContext.DeliveryRoute.AsNoTracking();
            DeliveryRouteDAOs = await DynamicFilter(DeliveryRouteDAOs, filter);
            DeliveryRouteDAOs = await OrFilter(DeliveryRouteDAOs, filter);
            return await DeliveryRouteDAOs.CountAsync();
        }

        public async Task<List<DeliveryRoute>> List(DeliveryRouteFilter filter)
        {
            if (filter == null) return new List<DeliveryRoute>();
            IQueryable<DeliveryRouteDAO> DeliveryRouteDAOs = DataContext.DeliveryRoute.AsNoTracking();
            DeliveryRouteDAOs = await DynamicFilter(DeliveryRouteDAOs, filter);
            DeliveryRouteDAOs = await OrFilter(DeliveryRouteDAOs, filter);
            DeliveryRouteDAOs = DynamicOrder(DeliveryRouteDAOs, filter);
            List<DeliveryRoute> DeliveryRoutes = await DynamicSelect(DeliveryRouteDAOs, filter);
            return DeliveryRoutes;
        }

        public async Task<List<DeliveryRoute>> List(List<long> Ids)
        {
            IdFilter IdFilter = new IdFilter { In = Ids };

            IQueryable<DeliveryRouteDAO> query = DataContext.DeliveryRoute.AsNoTracking();
            query = query.Where(q => q.Id, IdFilter);
            List<DeliveryRoute> DeliveryRoutes = await query.AsNoTracking()
            .Select(x => new DeliveryRoute()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt,
                Id = x.Id,
                Path = x.Path,
                CityFreighterId = x.CityFreighterId,
                TotalTravelDistance = x.TotalTravelDistance,
                TotalEmptyRunDistance = x.TotalEmptyRunDistance,
                CityFreighter = x.CityFreighter == null ? null : new CityFreighter
                {
                    Id = x.CityFreighter.Id,
                    Name = x.CityFreighter.Name,
                    Capacity = x.CityFreighter.Capacity,
                    NodeId = x.CityFreighter.NodeId,
                },
            }).ToListAsync();
            

            return DeliveryRoutes;
        }

        public async Task<DeliveryRoute> Get(long Id)
        {
            DeliveryRoute DeliveryRoute = await DataContext.DeliveryRoute.AsNoTracking()
            .Where(x => x.Id == Id)
            .Where(x => x.DeletedAt == null)
            .Select(x => new DeliveryRoute()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
                Path = x.Path,
                CityFreighterId = x.CityFreighterId,
                TotalTravelDistance = x.TotalTravelDistance,
                TotalEmptyRunDistance = x.TotalEmptyRunDistance,
                CityFreighter = x.CityFreighter == null ? null : new CityFreighter
                {
                    Id = x.CityFreighter.Id,
                    Name = x.CityFreighter.Name,
                    Capacity = x.CityFreighter.Capacity,
                    NodeId = x.CityFreighter.NodeId,
                },
            }).FirstOrDefaultAsync();

            if (DeliveryRoute == null)
                return null;

            return DeliveryRoute;
        }
        public async Task<bool> Create(DeliveryRoute DeliveryRoute)
        {
            DeliveryRouteDAO DeliveryRouteDAO = new DeliveryRouteDAO();
            DeliveryRouteDAO.Id = DeliveryRoute.Id;
            DeliveryRouteDAO.Path = DeliveryRoute.Path;
            DeliveryRouteDAO.CityFreighterId = DeliveryRoute.CityFreighterId;
            DeliveryRouteDAO.TotalTravelDistance = DeliveryRoute.TotalTravelDistance;
            DeliveryRouteDAO.TotalEmptyRunDistance = DeliveryRoute.TotalEmptyRunDistance;
            DeliveryRouteDAO.CreatedAt = StaticParams.DateTimeNow;
            DeliveryRouteDAO.UpdatedAt = StaticParams.DateTimeNow;
            DataContext.DeliveryRoute.Add(DeliveryRouteDAO);
            await DataContext.SaveChangesAsync();
            DeliveryRoute.Id = DeliveryRouteDAO.Id;
            await SaveReference(DeliveryRoute);
            return true;
        }

        public async Task<bool> Update(DeliveryRoute DeliveryRoute)
        {
            DeliveryRouteDAO DeliveryRouteDAO = DataContext.DeliveryRoute
                .Where(x => x.Id == DeliveryRoute.Id)
                .FirstOrDefault();
            if (DeliveryRouteDAO == null)
                return false;
            DeliveryRouteDAO.Id = DeliveryRoute.Id;
            DeliveryRouteDAO.Path = DeliveryRoute.Path;
            DeliveryRouteDAO.CityFreighterId = DeliveryRoute.CityFreighterId;
            DeliveryRouteDAO.TotalTravelDistance = DeliveryRoute.TotalTravelDistance;
            DeliveryRouteDAO.TotalEmptyRunDistance = DeliveryRoute.TotalEmptyRunDistance;
            DeliveryRouteDAO.UpdatedAt = StaticParams.DateTimeNow;
            await DataContext.SaveChangesAsync();
            await SaveReference(DeliveryRoute);
            return true;
        }

        public async Task<bool> Delete(DeliveryRoute DeliveryRoute)
        {
            await DataContext.DeliveryRoute
                .Where(x => x.Id == DeliveryRoute.Id)
                .UpdateFromQueryAsync(x => new DeliveryRouteDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        public async Task<List<long>> BulkMerge(List<DeliveryRoute> DeliveryRoutes)
        {
            IdFilter IdFilter = new IdFilter { In = DeliveryRoutes.Select(x => x.Id).ToList() };
            List<DeliveryRouteDAO> Inserts = new List<DeliveryRouteDAO>();
            List<DeliveryRouteDAO> Updates = new List<DeliveryRouteDAO>();
            List<DeliveryRouteDAO> DbDeliveryRouteDAOs = await DataContext.DeliveryRoute
                .Where(x => x.Id, IdFilter)
                .ToListAsync();
            foreach (DeliveryRoute DeliveryRoute in DeliveryRoutes)
            {
                DeliveryRouteDAO DeliveryRouteDAO = DbDeliveryRouteDAOs
                        .Where(x => x.Id == DeliveryRoute.Id)
                        .FirstOrDefault();
                if (DeliveryRouteDAO == null)
                {
                    DeliveryRouteDAO = new DeliveryRouteDAO();
                    DeliveryRouteDAO.CreatedAt = StaticParams.DateTimeNow;
                    Inserts.Add(DeliveryRouteDAO);
                }
                else
                {
                    Updates.Add(DeliveryRouteDAO);
                }
                DeliveryRouteDAO.Path = DeliveryRoute.Path;
                DeliveryRouteDAO.CityFreighterId = DeliveryRoute.CityFreighterId;
                DeliveryRouteDAO.TotalTravelDistance = DeliveryRoute.TotalTravelDistance;
                DeliveryRouteDAO.TotalEmptyRunDistance = DeliveryRoute.TotalEmptyRunDistance;
                DeliveryRouteDAO.UpdatedAt = StaticParams.DateTimeNow;
            }
            await DataContext.DeliveryRoute.BulkInsertAsync(Inserts);
            await DataContext.DeliveryRoute.BulkMergeAsync(Updates);
            var Ids = Inserts.Select(x => x.Id).ToList();
            Ids.AddRange(Updates.Select(x => x.Id));
            Ids = Ids.Distinct().ToList();
            return Ids;
        }
        
        public async Task<bool> BulkDelete(List<DeliveryRoute> DeliveryRoutes)
        {
            List<long> Ids = DeliveryRoutes.Select(x => x.Id).ToList();
            await DataContext.DeliveryRoute
                .WhereBulkContains(Ids, x => x.Id)
                .UpdateFromQueryAsync(x => new DeliveryRouteDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        private async Task SaveReference(DeliveryRoute DeliveryRoute)
        {
        }

    }
}
