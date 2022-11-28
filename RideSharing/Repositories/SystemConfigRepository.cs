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
    public interface ISystemConfigRepository
    {
        Task<int> CountAll(SystemConfigFilter SystemConfigFilter);
        Task<int> Count(SystemConfigFilter SystemConfigFilter);
        Task<List<SystemConfig>> List(SystemConfigFilter SystemConfigFilter);
        Task<List<SystemConfig>> List(List<long> Ids);
        Task<SystemConfig> Get(long Id);
        Task<bool> Create(SystemConfig SystemConfig);
        Task<bool> Update(SystemConfig SystemConfig);
        Task<bool> Delete(SystemConfig SystemConfig);
        Task<List<long>> BulkMerge(List<SystemConfig> SystemConfigs);
        Task<bool> BulkDelete(List<SystemConfig> SystemConfigs);
    }
    public class SystemConfigRepository : ISystemConfigRepository
    {
        private DataContext DataContext;
        public SystemConfigRepository(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        private async Task<IQueryable<SystemConfigDAO>> DynamicFilter(IQueryable<SystemConfigDAO> query, SystemConfigFilter filter)
        {
            if (filter == null)
                return query.Where(q => false);
            query = query.Where(q => !q.DeletedAt.HasValue);
            query = query.Where(q => q.CreatedAt, filter.CreatedAt);
            query = query.Where(q => q.UpdatedAt, filter.UpdatedAt);
            query = query.Where(q => q.Id, filter.Id);
            query = query.Where(q => q.Code, filter.Code);
            query = query.Where(q => q.FreighterQuotientCost, filter.FreighterQuotientCost);
            query = query.Where(q => q.DeliveryRadius, filter.DeliveryRadius);
            query = query.Where(q => q.DeliveryServiceDuration, filter.DeliveryServiceDuration);
            return query;
        }

        private async Task<IQueryable<SystemConfigDAO>> OrFilter(IQueryable<SystemConfigDAO> query, SystemConfigFilter filter)
        {
            if (filter.OrFilter == null || filter.OrFilter.Count == 0)
                return query;
            IQueryable<SystemConfigDAO> initQuery = query.Where(q => false);
            foreach (SystemConfigFilter SystemConfigFilter in filter.OrFilter)
            {
                IQueryable<SystemConfigDAO> queryable = query;
                queryable = queryable.Where(q => q.Id, SystemConfigFilter.Id);
                queryable = queryable.Where(q => q.Code, SystemConfigFilter.Code);
                queryable = queryable.Where(q => q.FreighterQuotientCost, SystemConfigFilter.FreighterQuotientCost);
                queryable = queryable.Where(q => q.DeliveryRadius, SystemConfigFilter.DeliveryRadius);
                queryable = queryable.Where(q => q.DeliveryServiceDuration, SystemConfigFilter.DeliveryServiceDuration);
                initQuery = initQuery.Union(queryable);
            }
            return initQuery;
        }    

        private IQueryable<SystemConfigDAO> DynamicOrder(IQueryable<SystemConfigDAO> query, SystemConfigFilter filter)
        {
            switch (filter.OrderType)
            {
                case OrderType.ASC:
                    switch (filter.OrderBy)
                    {
                        case SystemConfigOrder.Id:
                            query = query.OrderBy(q => q.Id);
                            break;
                        case SystemConfigOrder.Code:
                            query = query.OrderBy(q => q.Code);
                            break;
                        case SystemConfigOrder.FreighterQuotientCost:
                            query = query.OrderBy(q => q.FreighterQuotientCost);
                            break;
                        case SystemConfigOrder.DeliveryRadius:
                            query = query.OrderBy(q => q.DeliveryRadius);
                            break;
                        case SystemConfigOrder.DeliveryServiceDuration:
                            query = query.OrderBy(q => q.DeliveryServiceDuration);
                            break;
                    }
                    break;
                case OrderType.DESC:
                    switch (filter.OrderBy)
                    {
                        case SystemConfigOrder.Id:
                            query = query.OrderByDescending(q => q.Id);
                            break;
                        case SystemConfigOrder.Code:
                            query = query.OrderByDescending(q => q.Code);
                            break;
                        case SystemConfigOrder.FreighterQuotientCost:
                            query = query.OrderByDescending(q => q.FreighterQuotientCost);
                            break;
                        case SystemConfigOrder.DeliveryRadius:
                            query = query.OrderByDescending(q => q.DeliveryRadius);
                            break;
                        case SystemConfigOrder.DeliveryServiceDuration:
                            query = query.OrderByDescending(q => q.DeliveryServiceDuration);
                            break;
                    }
                    break;
            }
            query = query.Skip(filter.Skip).Take(filter.Take);
            return query;
        }

        private async Task<List<SystemConfig>> DynamicSelect(IQueryable<SystemConfigDAO> query, SystemConfigFilter filter)
        {
            List<SystemConfig> SystemConfigs = await query.Select(q => new SystemConfig()
            {
                Id = filter.Selects.Contains(SystemConfigSelect.Id) ? q.Id : default(long),
                Code = filter.Selects.Contains(SystemConfigSelect.Code) ? q.Code : default(string),
                FreighterQuotientCost = filter.Selects.Contains(SystemConfigSelect.FreighterQuotientCost) ? q.FreighterQuotientCost : default(decimal),
                DeliveryRadius = filter.Selects.Contains(SystemConfigSelect.DeliveryRadius) ? q.DeliveryRadius : default(decimal),
                DeliveryServiceDuration = filter.Selects.Contains(SystemConfigSelect.DeliveryServiceDuration) ? q.DeliveryServiceDuration : default(long),
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt,
                DeletedAt = q.DeletedAt,
            }).ToListAsync();
            return SystemConfigs;
        }

        public async Task<int> CountAll(SystemConfigFilter filter)
        {
            IQueryable<SystemConfigDAO> SystemConfigDAOs = DataContext.SystemConfig.AsNoTracking();
            SystemConfigDAOs = await DynamicFilter(SystemConfigDAOs, filter);
            return await SystemConfigDAOs.CountAsync();
        }

        public async Task<int> Count(SystemConfigFilter filter)
        {
            IQueryable<SystemConfigDAO> SystemConfigDAOs = DataContext.SystemConfig.AsNoTracking();
            SystemConfigDAOs = await DynamicFilter(SystemConfigDAOs, filter);
            SystemConfigDAOs = await OrFilter(SystemConfigDAOs, filter);
            return await SystemConfigDAOs.CountAsync();
        }

        public async Task<List<SystemConfig>> List(SystemConfigFilter filter)
        {
            if (filter == null) return new List<SystemConfig>();
            IQueryable<SystemConfigDAO> SystemConfigDAOs = DataContext.SystemConfig.AsNoTracking();
            SystemConfigDAOs = await DynamicFilter(SystemConfigDAOs, filter);
            SystemConfigDAOs = await OrFilter(SystemConfigDAOs, filter);
            SystemConfigDAOs = DynamicOrder(SystemConfigDAOs, filter);
            List<SystemConfig> SystemConfigs = await DynamicSelect(SystemConfigDAOs, filter);
            return SystemConfigs;
        }

        public async Task<List<SystemConfig>> List(List<long> Ids)
        {
            IdFilter IdFilter = new IdFilter { In = Ids };

            IQueryable<SystemConfigDAO> query = DataContext.SystemConfig.AsNoTracking();
            query = query.Where(q => q.Id, IdFilter);
            List<SystemConfig> SystemConfigs = await query.AsNoTracking()
            .Select(x => new SystemConfig()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt,
                Id = x.Id,
                Code = x.Code,
                FreighterQuotientCost = x.FreighterQuotientCost,
                DeliveryRadius = x.DeliveryRadius,
                DeliveryServiceDuration = x.DeliveryServiceDuration,
            }).ToListAsync();
            

            return SystemConfigs;
        }

        public async Task<SystemConfig> Get(long Id)
        {
            SystemConfig SystemConfig = await DataContext.SystemConfig.AsNoTracking()
            .Where(x => x.Id == Id)
            .Where(x => x.DeletedAt == null)
            .Select(x => new SystemConfig()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
                Code = x.Code,
                FreighterQuotientCost = x.FreighterQuotientCost,
                DeliveryRadius = x.DeliveryRadius,
                DeliveryServiceDuration = x.DeliveryServiceDuration,
            }).FirstOrDefaultAsync();

            if (SystemConfig == null)
                return null;

            return SystemConfig;
        }
        public async Task<bool> Create(SystemConfig SystemConfig)
        {
            SystemConfigDAO SystemConfigDAO = new SystemConfigDAO();
            SystemConfigDAO.Id = SystemConfig.Id;
            SystemConfigDAO.Code = SystemConfig.Code;
            SystemConfigDAO.FreighterQuotientCost = SystemConfig.FreighterQuotientCost;
            SystemConfigDAO.DeliveryRadius = SystemConfig.DeliveryRadius;
            SystemConfigDAO.DeliveryServiceDuration = SystemConfig.DeliveryServiceDuration;
            SystemConfigDAO.CreatedAt = StaticParams.DateTimeNow;
            SystemConfigDAO.UpdatedAt = StaticParams.DateTimeNow;
            DataContext.SystemConfig.Add(SystemConfigDAO);
            await DataContext.SaveChangesAsync();
            SystemConfig.Id = SystemConfigDAO.Id;
            await SaveReference(SystemConfig);
            return true;
        }

        public async Task<bool> Update(SystemConfig SystemConfig)
        {
            SystemConfigDAO SystemConfigDAO = DataContext.SystemConfig
                .Where(x => x.Id == SystemConfig.Id)
                .FirstOrDefault();
            if (SystemConfigDAO == null)
                return false;
            SystemConfigDAO.Id = SystemConfig.Id;
            SystemConfigDAO.Code = SystemConfig.Code;
            SystemConfigDAO.FreighterQuotientCost = SystemConfig.FreighterQuotientCost;
            SystemConfigDAO.DeliveryRadius = SystemConfig.DeliveryRadius;
            SystemConfigDAO.DeliveryServiceDuration = SystemConfig.DeliveryServiceDuration;
            SystemConfigDAO.UpdatedAt = StaticParams.DateTimeNow;
            await DataContext.SaveChangesAsync();
            await SaveReference(SystemConfig);
            return true;
        }

        public async Task<bool> Delete(SystemConfig SystemConfig)
        {
            await DataContext.SystemConfig
                .Where(x => x.Id == SystemConfig.Id)
                .UpdateFromQueryAsync(x => new SystemConfigDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        public async Task<List<long>> BulkMerge(List<SystemConfig> SystemConfigs)
        {
            IdFilter IdFilter = new IdFilter { In = SystemConfigs.Select(x => x.Id).ToList() };
            List<SystemConfigDAO> Inserts = new List<SystemConfigDAO>();
            List<SystemConfigDAO> Updates = new List<SystemConfigDAO>();
            List<SystemConfigDAO> DbSystemConfigDAOs = await DataContext.SystemConfig
                .Where(x => x.Id, IdFilter)
                .ToListAsync();
            foreach (SystemConfig SystemConfig in SystemConfigs)
            {
                SystemConfigDAO SystemConfigDAO = DbSystemConfigDAOs
                        .Where(x => x.Id == SystemConfig.Id)
                        .FirstOrDefault();
                if (SystemConfigDAO == null)
                {
                    SystemConfigDAO = new SystemConfigDAO();
                    SystemConfigDAO.CreatedAt = StaticParams.DateTimeNow;
                    Inserts.Add(SystemConfigDAO);
                }
                else
                {
                    Updates.Add(SystemConfigDAO);
                }
                SystemConfigDAO.Code = SystemConfig.Code;
                SystemConfigDAO.FreighterQuotientCost = SystemConfig.FreighterQuotientCost;
                SystemConfigDAO.DeliveryRadius = SystemConfig.DeliveryRadius;
                SystemConfigDAO.DeliveryServiceDuration = SystemConfig.DeliveryServiceDuration;
                SystemConfigDAO.UpdatedAt = StaticParams.DateTimeNow;
            }
            await DataContext.SystemConfig.BulkInsertAsync(Inserts);
            await DataContext.SystemConfig.BulkMergeAsync(Updates);
            var Ids = Inserts.Select(x => x.Id).ToList();
            Ids.AddRange(Updates.Select(x => x.Id));
            Ids = Ids.Distinct().ToList();
            return Ids;
        }
        
        public async Task<bool> BulkDelete(List<SystemConfig> SystemConfigs)
        {
            List<long> Ids = SystemConfigs.Select(x => x.Id).ToList();
            await DataContext.SystemConfig
                .WhereBulkContains(Ids, x => x.Id)
                .UpdateFromQueryAsync(x => new SystemConfigDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        private async Task SaveReference(SystemConfig SystemConfig)
        {
        }

    }
}
