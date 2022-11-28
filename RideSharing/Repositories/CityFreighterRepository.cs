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
    public interface ICityFreighterRepository
    {
        Task<int> CountAll(CityFreighterFilter CityFreighterFilter);
        Task<int> Count(CityFreighterFilter CityFreighterFilter);
        Task<List<CityFreighter>> List(CityFreighterFilter CityFreighterFilter);
        Task<List<CityFreighter>> List(List<long> Ids);
        Task<CityFreighter> Get(long Id);
        Task<bool> Create(CityFreighter CityFreighter);
        Task<bool> Update(CityFreighter CityFreighter);
        Task<bool> Delete(CityFreighter CityFreighter);
        Task<List<long>> BulkMerge(List<CityFreighter> CityFreighters);
        Task<bool> BulkDelete(List<CityFreighter> CityFreighters);
    }
    public class CityFreighterRepository : ICityFreighterRepository
    {
        private DataContext DataContext;
        public CityFreighterRepository(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        private async Task<IQueryable<CityFreighterDAO>> DynamicFilter(IQueryable<CityFreighterDAO> query, CityFreighterFilter filter)
        {
            if (filter == null)
                return query.Where(q => false);
            query = query.Where(q => !q.DeletedAt.HasValue);
            query = query.Where(q => q.CreatedAt, filter.CreatedAt);
            query = query.Where(q => q.UpdatedAt, filter.UpdatedAt);
            query = query.Where(q => q.Id, filter.Id);
            query = query.Where(q => q.Name, filter.Name);
            query = query.Where(q => q.Capacity, filter.Capacity);
            query = query.Where(q => q.Latitude, filter.Latitude);
            query = query.Where(q => q.Longtitude, filter.Longtitude);
            return query;
        }

        private async Task<IQueryable<CityFreighterDAO>> OrFilter(IQueryable<CityFreighterDAO> query, CityFreighterFilter filter)
        {
            if (filter.OrFilter == null || filter.OrFilter.Count == 0)
                return query;
            IQueryable<CityFreighterDAO> initQuery = query.Where(q => false);
            foreach (CityFreighterFilter CityFreighterFilter in filter.OrFilter)
            {
                IQueryable<CityFreighterDAO> queryable = query;
                queryable = queryable.Where(q => q.Id, CityFreighterFilter.Id);
                queryable = queryable.Where(q => q.Name, CityFreighterFilter.Name);
                queryable = queryable.Where(q => q.Capacity, CityFreighterFilter.Capacity);
                queryable = queryable.Where(q => q.Latitude, CityFreighterFilter.Latitude);
                queryable = queryable.Where(q => q.Longtitude, CityFreighterFilter.Longtitude);
                initQuery = initQuery.Union(queryable);
            }
            return initQuery;
        }    

        private IQueryable<CityFreighterDAO> DynamicOrder(IQueryable<CityFreighterDAO> query, CityFreighterFilter filter)
        {
            switch (filter.OrderType)
            {
                case OrderType.ASC:
                    switch (filter.OrderBy)
                    {
                        case CityFreighterOrder.Id:
                            query = query.OrderBy(q => q.Id);
                            break;
                        case CityFreighterOrder.Name:
                            query = query.OrderBy(q => q.Name);
                            break;
                        case CityFreighterOrder.Capacity:
                            query = query.OrderBy(q => q.Capacity);
                            break;
                        case CityFreighterOrder.Latitude:
                            query = query.OrderBy(q => q.Latitude);
                            break;
                        case CityFreighterOrder.Longtitude:
                            query = query.OrderBy(q => q.Longtitude);
                            break;
                    }
                    break;
                case OrderType.DESC:
                    switch (filter.OrderBy)
                    {
                        case CityFreighterOrder.Id:
                            query = query.OrderByDescending(q => q.Id);
                            break;
                        case CityFreighterOrder.Name:
                            query = query.OrderByDescending(q => q.Name);
                            break;
                        case CityFreighterOrder.Capacity:
                            query = query.OrderByDescending(q => q.Capacity);
                            break;
                        case CityFreighterOrder.Latitude:
                            query = query.OrderByDescending(q => q.Latitude);
                            break;
                        case CityFreighterOrder.Longtitude:
                            query = query.OrderByDescending(q => q.Longtitude);
                            break;
                    }
                    break;
            }
            query = query.Skip(filter.Skip).Take(filter.Take);
            return query;
        }

        private async Task<List<CityFreighter>> DynamicSelect(IQueryable<CityFreighterDAO> query, CityFreighterFilter filter)
        {
            List<CityFreighter> CityFreighters = await query.Select(q => new CityFreighter()
            {
                Id = filter.Selects.Contains(CityFreighterSelect.Id) ? q.Id : default(long),
                Name = filter.Selects.Contains(CityFreighterSelect.Name) ? q.Name : default(string),
                Capacity = filter.Selects.Contains(CityFreighterSelect.Capacity) ? q.Capacity : default(decimal),
                Latitude = filter.Selects.Contains(CityFreighterSelect.Latitude) ? q.Latitude : default(decimal),
                Longtitude = filter.Selects.Contains(CityFreighterSelect.Longtitude) ? q.Longtitude : default(decimal),
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt,
                DeletedAt = q.DeletedAt,
            }).ToListAsync();
            return CityFreighters;
        }

        public async Task<int> CountAll(CityFreighterFilter filter)
        {
            IQueryable<CityFreighterDAO> CityFreighterDAOs = DataContext.CityFreighter.AsNoTracking();
            CityFreighterDAOs = await DynamicFilter(CityFreighterDAOs, filter);
            return await CityFreighterDAOs.CountAsync();
        }

        public async Task<int> Count(CityFreighterFilter filter)
        {
            IQueryable<CityFreighterDAO> CityFreighterDAOs = DataContext.CityFreighter.AsNoTracking();
            CityFreighterDAOs = await DynamicFilter(CityFreighterDAOs, filter);
            CityFreighterDAOs = await OrFilter(CityFreighterDAOs, filter);
            return await CityFreighterDAOs.CountAsync();
        }

        public async Task<List<CityFreighter>> List(CityFreighterFilter filter)
        {
            if (filter == null) return new List<CityFreighter>();
            IQueryable<CityFreighterDAO> CityFreighterDAOs = DataContext.CityFreighter.AsNoTracking();
            CityFreighterDAOs = await DynamicFilter(CityFreighterDAOs, filter);
            CityFreighterDAOs = await OrFilter(CityFreighterDAOs, filter);
            CityFreighterDAOs = DynamicOrder(CityFreighterDAOs, filter);
            List<CityFreighter> CityFreighters = await DynamicSelect(CityFreighterDAOs, filter);
            return CityFreighters;
        }

        public async Task<List<CityFreighter>> List(List<long> Ids)
        {
            IdFilter IdFilter = new IdFilter { In = Ids };

            IQueryable<CityFreighterDAO> query = DataContext.CityFreighter.AsNoTracking();
            query = query.Where(q => q.Id, IdFilter);
            List<CityFreighter> CityFreighters = await query.AsNoTracking()
            .Select(x => new CityFreighter()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt,
                Id = x.Id,
                Name = x.Name,
                Capacity = x.Capacity,
                Latitude = x.Latitude,
                Longtitude = x.Longtitude,
            }).ToListAsync();
            

            return CityFreighters;
        }

        public async Task<CityFreighter> Get(long Id)
        {
            CityFreighter CityFreighter = await DataContext.CityFreighter.AsNoTracking()
            .Where(x => x.Id == Id)
            .Where(x => x.DeletedAt == null)
            .Select(x => new CityFreighter()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
                Name = x.Name,
                Capacity = x.Capacity,
                Latitude = x.Latitude,
                Longtitude = x.Longtitude,
            }).FirstOrDefaultAsync();

            if (CityFreighter == null)
                return null;

            return CityFreighter;
        }
        public async Task<bool> Create(CityFreighter CityFreighter)
        {
            CityFreighterDAO CityFreighterDAO = new CityFreighterDAO();
            CityFreighterDAO.Id = CityFreighter.Id;
            CityFreighterDAO.Name = CityFreighter.Name;
            CityFreighterDAO.Capacity = CityFreighter.Capacity;
            CityFreighterDAO.Latitude = CityFreighter.Latitude;
            CityFreighterDAO.Longtitude = CityFreighter.Longtitude;
            CityFreighterDAO.CreatedAt = StaticParams.DateTimeNow;
            CityFreighterDAO.UpdatedAt = StaticParams.DateTimeNow;
            DataContext.CityFreighter.Add(CityFreighterDAO);
            await DataContext.SaveChangesAsync();
            CityFreighter.Id = CityFreighterDAO.Id;
            await SaveReference(CityFreighter);
            return true;
        }

        public async Task<bool> Update(CityFreighter CityFreighter)
        {
            CityFreighterDAO CityFreighterDAO = DataContext.CityFreighter
                .Where(x => x.Id == CityFreighter.Id)
                .FirstOrDefault();
            if (CityFreighterDAO == null)
                return false;
            CityFreighterDAO.Id = CityFreighter.Id;
            CityFreighterDAO.Name = CityFreighter.Name;
            CityFreighterDAO.Capacity = CityFreighter.Capacity;
            CityFreighterDAO.Latitude = CityFreighter.Latitude;
            CityFreighterDAO.Longtitude = CityFreighter.Longtitude;
            CityFreighterDAO.UpdatedAt = StaticParams.DateTimeNow;
            await DataContext.SaveChangesAsync();
            await SaveReference(CityFreighter);
            return true;
        }

        public async Task<bool> Delete(CityFreighter CityFreighter)
        {
            await DataContext.CityFreighter
                .Where(x => x.Id == CityFreighter.Id)
                .UpdateFromQueryAsync(x => new CityFreighterDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        public async Task<List<long>> BulkMerge(List<CityFreighter> CityFreighters)
        {
            IdFilter IdFilter = new IdFilter { In = CityFreighters.Select(x => x.Id).ToList() };
            List<CityFreighterDAO> Inserts = new List<CityFreighterDAO>();
            List<CityFreighterDAO> Updates = new List<CityFreighterDAO>();
            List<CityFreighterDAO> DbCityFreighterDAOs = await DataContext.CityFreighter
                .Where(x => x.Id, IdFilter)
                .ToListAsync();
            foreach (CityFreighter CityFreighter in CityFreighters)
            {
                CityFreighterDAO CityFreighterDAO = DbCityFreighterDAOs
                        .Where(x => x.Id == CityFreighter.Id)
                        .FirstOrDefault();
                if (CityFreighterDAO == null)
                {
                    CityFreighterDAO = new CityFreighterDAO();
                    CityFreighterDAO.CreatedAt = StaticParams.DateTimeNow;
                    Inserts.Add(CityFreighterDAO);
                }
                else
                {
                    Updates.Add(CityFreighterDAO);
                }
                CityFreighterDAO.Name = CityFreighter.Name;
                CityFreighterDAO.Capacity = CityFreighter.Capacity;
                CityFreighterDAO.Latitude = CityFreighter.Latitude;
                CityFreighterDAO.Longtitude = CityFreighter.Longtitude;
                CityFreighterDAO.UpdatedAt = StaticParams.DateTimeNow;
            }
            await DataContext.CityFreighter.BulkInsertAsync(Inserts);
            await DataContext.CityFreighter.BulkMergeAsync(Updates);
            var Ids = Inserts.Select(x => x.Id).ToList();
            Ids.AddRange(Updates.Select(x => x.Id));
            Ids = Ids.Distinct().ToList();
            return Ids;
        }
        
        public async Task<bool> BulkDelete(List<CityFreighter> CityFreighters)
        {
            List<long> Ids = CityFreighters.Select(x => x.Id).ToList();
            await DataContext.CityFreighter
                .WhereBulkContains(Ids, x => x.Id)
                .UpdateFromQueryAsync(x => new CityFreighterDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        private async Task SaveReference(CityFreighter CityFreighter)
        {
        }

    }
}
