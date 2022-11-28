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
    public interface IBusStopRepository
    {
        Task<int> CountAll(BusStopFilter BusStopFilter);
        Task<int> Count(BusStopFilter BusStopFilter);
        Task<List<BusStop>> List(BusStopFilter BusStopFilter);
        Task<List<BusStop>> List(List<long> Ids);
        Task<BusStop> Get(long Id);
        Task<bool> Create(BusStop BusStop);
        Task<bool> Update(BusStop BusStop);
        Task<bool> Delete(BusStop BusStop);
        Task<List<long>> BulkMerge(List<BusStop> BusStops);
        Task<bool> BulkDelete(List<BusStop> BusStops);
    }
    public class BusStopRepository : IBusStopRepository
    {
        private DataContext DataContext;
        public BusStopRepository(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        private async Task<IQueryable<BusStopDAO>> DynamicFilter(IQueryable<BusStopDAO> query, BusStopFilter filter)
        {
            if (filter == null)
                return query.Where(q => false);
            query = query.Where(q => !q.DeletedAt.HasValue);
            query = query.Where(q => q.CreatedAt, filter.CreatedAt);
            query = query.Where(q => q.UpdatedAt, filter.UpdatedAt);
            query = query.Where(q => q.Id, filter.Id);
            query = query.Where(q => q.Name, filter.Name);
            query = query.Where(q => q.NodeId, filter.NodeId);
            return query;
        }

        private async Task<IQueryable<BusStopDAO>> OrFilter(IQueryable<BusStopDAO> query, BusStopFilter filter)
        {
            if (filter.OrFilter == null || filter.OrFilter.Count == 0)
                return query;
            IQueryable<BusStopDAO> initQuery = query.Where(q => false);
            foreach (BusStopFilter BusStopFilter in filter.OrFilter)
            {
                IQueryable<BusStopDAO> queryable = query;
                queryable = queryable.Where(q => q.Id, BusStopFilter.Id);
                queryable = queryable.Where(q => q.Name, BusStopFilter.Name);
                queryable = queryable.Where(q => q.NodeId, BusStopFilter.NodeId);
                initQuery = initQuery.Union(queryable);
            }
            return initQuery;
        }    

        private IQueryable<BusStopDAO> DynamicOrder(IQueryable<BusStopDAO> query, BusStopFilter filter)
        {
            switch (filter.OrderType)
            {
                case OrderType.ASC:
                    switch (filter.OrderBy)
                    {
                        case BusStopOrder.Id:
                            query = query.OrderBy(q => q.Id);
                            break;
                        case BusStopOrder.Name:
                            query = query.OrderBy(q => q.Name);
                            break;
                        case BusStopOrder.Node:
                            query = query.OrderBy(q => q.NodeId);
                            break;
                    }
                    break;
                case OrderType.DESC:
                    switch (filter.OrderBy)
                    {
                        case BusStopOrder.Id:
                            query = query.OrderByDescending(q => q.Id);
                            break;
                        case BusStopOrder.Name:
                            query = query.OrderByDescending(q => q.Name);
                            break;
                        case BusStopOrder.Node:
                            query = query.OrderByDescending(q => q.NodeId);
                            break;
                    }
                    break;
            }
            query = query.Skip(filter.Skip).Take(filter.Take);
            return query;
        }

        private async Task<List<BusStop>> DynamicSelect(IQueryable<BusStopDAO> query, BusStopFilter filter)
        {
            List<BusStop> BusStops = await query.Select(q => new BusStop()
            {
                Id = filter.Selects.Contains(BusStopSelect.Id) ? q.Id : default(long),
                Name = filter.Selects.Contains(BusStopSelect.Name) ? q.Name : default(string),
                NodeId = filter.Selects.Contains(BusStopSelect.Node) ? q.NodeId : default(long),
                Node = filter.Selects.Contains(BusStopSelect.Node) && q.Node != null ? new Node
                {
                    Id = q.Node.Id,
                    Code = q.Node.Code,
                    Longtitude = q.Node.Longtitude,
                    Latitude = q.Node.Latitude,
                } : null,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt,
                DeletedAt = q.DeletedAt,
            }).ToListAsync();
            return BusStops;
        }

        public async Task<int> CountAll(BusStopFilter filter)
        {
            IQueryable<BusStopDAO> BusStopDAOs = DataContext.BusStop.AsNoTracking();
            BusStopDAOs = await DynamicFilter(BusStopDAOs, filter);
            return await BusStopDAOs.CountAsync();
        }

        public async Task<int> Count(BusStopFilter filter)
        {
            IQueryable<BusStopDAO> BusStopDAOs = DataContext.BusStop.AsNoTracking();
            BusStopDAOs = await DynamicFilter(BusStopDAOs, filter);
            BusStopDAOs = await OrFilter(BusStopDAOs, filter);
            return await BusStopDAOs.CountAsync();
        }

        public async Task<List<BusStop>> List(BusStopFilter filter)
        {
            if (filter == null) return new List<BusStop>();
            IQueryable<BusStopDAO> BusStopDAOs = DataContext.BusStop.AsNoTracking();
            BusStopDAOs = await DynamicFilter(BusStopDAOs, filter);
            BusStopDAOs = await OrFilter(BusStopDAOs, filter);
            BusStopDAOs = DynamicOrder(BusStopDAOs, filter);
            List<BusStop> BusStops = await DynamicSelect(BusStopDAOs, filter);
            return BusStops;
        }

        public async Task<List<BusStop>> List(List<long> Ids)
        {
            IdFilter IdFilter = new IdFilter { In = Ids };

            IQueryable<BusStopDAO> query = DataContext.BusStop.AsNoTracking();
            query = query.Where(q => q.Id, IdFilter);
            List<BusStop> BusStops = await query.AsNoTracking()
            .Select(x => new BusStop()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt,
                Id = x.Id,
                Name = x.Name,
                NodeId = x.NodeId,
                Node = x.Node == null ? null : new Node
                {
                    Id = x.Node.Id,
                    Code = x.Node.Code,
                    Longtitude = x.Node.Longtitude,
                    Latitude = x.Node.Latitude,
                },
            }).ToListAsync();
            

            return BusStops;
        }

        public async Task<BusStop> Get(long Id)
        {
            BusStop BusStop = await DataContext.BusStop.AsNoTracking()
            .Where(x => x.Id == Id)
            .Where(x => x.DeletedAt == null)
            .Select(x => new BusStop()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
                Name = x.Name,
                NodeId = x.NodeId,
                Node = x.Node == null ? null : new Node
                {
                    Id = x.Node.Id,
                    Code = x.Node.Code,
                    Longtitude = x.Node.Longtitude,
                    Latitude = x.Node.Latitude,
                },
            }).FirstOrDefaultAsync();

            if (BusStop == null)
                return null;

            return BusStop;
        }
        public async Task<bool> Create(BusStop BusStop)
        {
            BusStopDAO BusStopDAO = new BusStopDAO();
            BusStopDAO.Id = BusStop.Id;
            BusStopDAO.Name = BusStop.Name;
            BusStopDAO.NodeId = BusStop.NodeId;
            BusStopDAO.CreatedAt = StaticParams.DateTimeNow;
            BusStopDAO.UpdatedAt = StaticParams.DateTimeNow;
            DataContext.BusStop.Add(BusStopDAO);
            await DataContext.SaveChangesAsync();
            BusStop.Id = BusStopDAO.Id;
            await SaveReference(BusStop);
            return true;
        }

        public async Task<bool> Update(BusStop BusStop)
        {
            BusStopDAO BusStopDAO = DataContext.BusStop
                .Where(x => x.Id == BusStop.Id)
                .FirstOrDefault();
            if (BusStopDAO == null)
                return false;
            BusStopDAO.Id = BusStop.Id;
            BusStopDAO.Name = BusStop.Name;
            BusStopDAO.NodeId = BusStop.NodeId;
            BusStopDAO.UpdatedAt = StaticParams.DateTimeNow;
            await DataContext.SaveChangesAsync();
            await SaveReference(BusStop);
            return true;
        }

        public async Task<bool> Delete(BusStop BusStop)
        {
            await DataContext.BusStop
                .Where(x => x.Id == BusStop.Id)
                .UpdateFromQueryAsync(x => new BusStopDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        public async Task<List<long>> BulkMerge(List<BusStop> BusStops)
        {
            IdFilter IdFilter = new IdFilter { In = BusStops.Select(x => x.Id).ToList() };
            List<BusStopDAO> Inserts = new List<BusStopDAO>();
            List<BusStopDAO> Updates = new List<BusStopDAO>();
            List<BusStopDAO> DbBusStopDAOs = await DataContext.BusStop
                .Where(x => x.Id, IdFilter)
                .ToListAsync();
            foreach (BusStop BusStop in BusStops)
            {
                BusStopDAO BusStopDAO = DbBusStopDAOs
                        .Where(x => x.Id == BusStop.Id)
                        .FirstOrDefault();
                if (BusStopDAO == null)
                {
                    BusStopDAO = new BusStopDAO();
                    BusStopDAO.CreatedAt = StaticParams.DateTimeNow;
                    Inserts.Add(BusStopDAO);
                }
                else
                {
                    Updates.Add(BusStopDAO);
                }
                BusStopDAO.Name = BusStop.Name;
                BusStopDAO.NodeId = BusStop.NodeId;
                BusStopDAO.UpdatedAt = StaticParams.DateTimeNow;
            }
            await DataContext.BusStop.BulkInsertAsync(Inserts);
            await DataContext.BusStop.BulkMergeAsync(Updates);
            var Ids = Inserts.Select(x => x.Id).ToList();
            Ids.AddRange(Updates.Select(x => x.Id));
            Ids = Ids.Distinct().ToList();
            return Ids;
        }
        
        public async Task<bool> BulkDelete(List<BusStop> BusStops)
        {
            List<long> Ids = BusStops.Select(x => x.Id).ToList();
            await DataContext.BusStop
                .WhereBulkContains(Ids, x => x.Id)
                .UpdateFromQueryAsync(x => new BusStopDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        private async Task SaveReference(BusStop BusStop)
        {
        }

    }
}
