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
    public interface IDeliveryOrderRepository
    {
        Task<int> CountAll(DeliveryOrderFilter DeliveryOrderFilter);
        Task<int> Count(DeliveryOrderFilter DeliveryOrderFilter);
        Task<List<DeliveryOrder>> List(DeliveryOrderFilter DeliveryOrderFilter);
        Task<List<DeliveryOrder>> List(List<long> Ids);
        Task<DeliveryOrder> Get(long Id);
        Task<bool> Create(DeliveryOrder DeliveryOrder);
        Task<bool> Update(DeliveryOrder DeliveryOrder);
        Task<bool> Delete(DeliveryOrder DeliveryOrder);
        Task<List<long>> BulkMerge(List<DeliveryOrder> DeliveryOrders);
        Task<bool> BulkDelete(List<DeliveryOrder> DeliveryOrders);
    }
    public class DeliveryOrderRepository : IDeliveryOrderRepository
    {
        private DataContext DataContext;
        public DeliveryOrderRepository(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        private async Task<IQueryable<DeliveryOrderDAO>> DynamicFilter(IQueryable<DeliveryOrderDAO> query, DeliveryOrderFilter filter)
        {
            if (filter == null)
                return query.Where(q => false);
            query = query.Where(q => !q.DeletedAt.HasValue);
            query = query.Where(q => q.CreatedAt, filter.CreatedAt);
            query = query.Where(q => q.UpdatedAt, filter.UpdatedAt);
            query = query.Where(q => q.Id, filter.Id);
            query = query.Where(q => q.Code, filter.Code);
            query = query.Where(q => q.Weight, filter.Weight);
            query = query.Where(q => q.CustomerId, filter.CustomerId);
            return query;
        }

        private async Task<IQueryable<DeliveryOrderDAO>> OrFilter(IQueryable<DeliveryOrderDAO> query, DeliveryOrderFilter filter)
        {
            if (filter.OrFilter == null || filter.OrFilter.Count == 0)
                return query;
            IQueryable<DeliveryOrderDAO> initQuery = query.Where(q => false);
            foreach (DeliveryOrderFilter DeliveryOrderFilter in filter.OrFilter)
            {
                IQueryable<DeliveryOrderDAO> queryable = query;
                queryable = queryable.Where(q => q.Id, DeliveryOrderFilter.Id);
                queryable = queryable.Where(q => q.Code, DeliveryOrderFilter.Code);
                queryable = queryable.Where(q => q.Weight, DeliveryOrderFilter.Weight);
                queryable = queryable.Where(q => q.CustomerId, DeliveryOrderFilter.CustomerId);
                initQuery = initQuery.Union(queryable);
            }
            return initQuery;
        }    

        private IQueryable<DeliveryOrderDAO> DynamicOrder(IQueryable<DeliveryOrderDAO> query, DeliveryOrderFilter filter)
        {
            switch (filter.OrderType)
            {
                case OrderType.ASC:
                    switch (filter.OrderBy)
                    {
                        case DeliveryOrderOrder.Id:
                            query = query.OrderBy(q => q.Id);
                            break;
                        case DeliveryOrderOrder.Code:
                            query = query.OrderBy(q => q.Code);
                            break;
                        case DeliveryOrderOrder.Weight:
                            query = query.OrderBy(q => q.Weight);
                            break;
                        case DeliveryOrderOrder.Customer:
                            query = query.OrderBy(q => q.CustomerId);
                            break;
                    }
                    break;
                case OrderType.DESC:
                    switch (filter.OrderBy)
                    {
                        case DeliveryOrderOrder.Id:
                            query = query.OrderByDescending(q => q.Id);
                            break;
                        case DeliveryOrderOrder.Code:
                            query = query.OrderByDescending(q => q.Code);
                            break;
                        case DeliveryOrderOrder.Weight:
                            query = query.OrderByDescending(q => q.Weight);
                            break;
                        case DeliveryOrderOrder.Customer:
                            query = query.OrderByDescending(q => q.CustomerId);
                            break;
                    }
                    break;
            }
            query = query.Skip(filter.Skip).Take(filter.Take);
            return query;
        }

        private async Task<List<DeliveryOrder>> DynamicSelect(IQueryable<DeliveryOrderDAO> query, DeliveryOrderFilter filter)
        {
            List<DeliveryOrder> DeliveryOrders = await query.Select(q => new DeliveryOrder()
            {
                Id = filter.Selects.Contains(DeliveryOrderSelect.Id) ? q.Id : default(long),
                Code = filter.Selects.Contains(DeliveryOrderSelect.Code) ? q.Code : default(string),
                Weight = filter.Selects.Contains(DeliveryOrderSelect.Weight) ? q.Weight : default(decimal),
                CustomerId = filter.Selects.Contains(DeliveryOrderSelect.Customer) ? q.CustomerId : default(long),
                Customer = filter.Selects.Contains(DeliveryOrderSelect.Customer) && q.Customer != null ? new Customer
                {
                    Id = q.Customer.Id,
                    Code = q.Customer.Code,
                    Name = q.Customer.Name,
                    NodeId = q.Customer.NodeId,
                } : null,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt,
                DeletedAt = q.DeletedAt,
            }).ToListAsync();
            return DeliveryOrders;
        }

        public async Task<int> CountAll(DeliveryOrderFilter filter)
        {
            IQueryable<DeliveryOrderDAO> DeliveryOrderDAOs = DataContext.DeliveryOrder.AsNoTracking();
            DeliveryOrderDAOs = await DynamicFilter(DeliveryOrderDAOs, filter);
            return await DeliveryOrderDAOs.CountAsync();
        }

        public async Task<int> Count(DeliveryOrderFilter filter)
        {
            IQueryable<DeliveryOrderDAO> DeliveryOrderDAOs = DataContext.DeliveryOrder.AsNoTracking();
            DeliveryOrderDAOs = await DynamicFilter(DeliveryOrderDAOs, filter);
            DeliveryOrderDAOs = await OrFilter(DeliveryOrderDAOs, filter);
            return await DeliveryOrderDAOs.CountAsync();
        }

        public async Task<List<DeliveryOrder>> List(DeliveryOrderFilter filter)
        {
            if (filter == null) return new List<DeliveryOrder>();
            IQueryable<DeliveryOrderDAO> DeliveryOrderDAOs = DataContext.DeliveryOrder.AsNoTracking();
            DeliveryOrderDAOs = await DynamicFilter(DeliveryOrderDAOs, filter);
            DeliveryOrderDAOs = await OrFilter(DeliveryOrderDAOs, filter);
            DeliveryOrderDAOs = DynamicOrder(DeliveryOrderDAOs, filter);
            List<DeliveryOrder> DeliveryOrders = await DynamicSelect(DeliveryOrderDAOs, filter);
            return DeliveryOrders;
        }

        public async Task<List<DeliveryOrder>> List(List<long> Ids)
        {
            IdFilter IdFilter = new IdFilter { In = Ids };

            IQueryable<DeliveryOrderDAO> query = DataContext.DeliveryOrder.AsNoTracking();
            query = query.Where(q => q.Id, IdFilter);
            List<DeliveryOrder> DeliveryOrders = await query.AsNoTracking()
            .Select(x => new DeliveryOrder()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt,
                Id = x.Id,
                Code = x.Code,
                Weight = x.Weight,
                CustomerId = x.CustomerId,
                Customer = x.Customer == null ? null : new Customer
                {
                    Id = x.Customer.Id,
                    Code = x.Customer.Code,
                    Name = x.Customer.Name,
                    NodeId = x.Customer.NodeId,
                },
            }).ToListAsync();
            

            return DeliveryOrders;
        }

        public async Task<DeliveryOrder> Get(long Id)
        {
            DeliveryOrder DeliveryOrder = await DataContext.DeliveryOrder.AsNoTracking()
            .Where(x => x.Id == Id)
            .Where(x => x.DeletedAt == null)
            .Select(x => new DeliveryOrder()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
                Code = x.Code,
                Weight = x.Weight,
                CustomerId = x.CustomerId,
                Customer = x.Customer == null ? null : new Customer
                {
                    Id = x.Customer.Id,
                    Code = x.Customer.Code,
                    Name = x.Customer.Name,
                    NodeId = x.Customer.NodeId,
                },
            }).FirstOrDefaultAsync();

            if (DeliveryOrder == null)
                return null;

            return DeliveryOrder;
        }
        public async Task<bool> Create(DeliveryOrder DeliveryOrder)
        {
            DeliveryOrderDAO DeliveryOrderDAO = new DeliveryOrderDAO();
            DeliveryOrderDAO.Id = DeliveryOrder.Id;
            DeliveryOrderDAO.Code = DeliveryOrder.Code;
            DeliveryOrderDAO.Weight = DeliveryOrder.Weight;
            DeliveryOrderDAO.CustomerId = DeliveryOrder.CustomerId;
            DeliveryOrderDAO.CreatedAt = StaticParams.DateTimeNow;
            DeliveryOrderDAO.UpdatedAt = StaticParams.DateTimeNow;
            DataContext.DeliveryOrder.Add(DeliveryOrderDAO);
            await DataContext.SaveChangesAsync();
            DeliveryOrder.Id = DeliveryOrderDAO.Id;
            await SaveReference(DeliveryOrder);
            return true;
        }

        public async Task<bool> Update(DeliveryOrder DeliveryOrder)
        {
            DeliveryOrderDAO DeliveryOrderDAO = DataContext.DeliveryOrder
                .Where(x => x.Id == DeliveryOrder.Id)
                .FirstOrDefault();
            if (DeliveryOrderDAO == null)
                return false;
            DeliveryOrderDAO.Id = DeliveryOrder.Id;
            DeliveryOrderDAO.Code = DeliveryOrder.Code;
            DeliveryOrderDAO.Weight = DeliveryOrder.Weight;
            DeliveryOrderDAO.CustomerId = DeliveryOrder.CustomerId;
            DeliveryOrderDAO.UpdatedAt = StaticParams.DateTimeNow;
            await DataContext.SaveChangesAsync();
            await SaveReference(DeliveryOrder);
            return true;
        }

        public async Task<bool> Delete(DeliveryOrder DeliveryOrder)
        {
            await DataContext.DeliveryOrder
                .Where(x => x.Id == DeliveryOrder.Id)
                .UpdateFromQueryAsync(x => new DeliveryOrderDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        public async Task<List<long>> BulkMerge(List<DeliveryOrder> DeliveryOrders)
        {
            IdFilter IdFilter = new IdFilter { In = DeliveryOrders.Select(x => x.Id).ToList() };
            List<DeliveryOrderDAO> Inserts = new List<DeliveryOrderDAO>();
            List<DeliveryOrderDAO> Updates = new List<DeliveryOrderDAO>();
            List<DeliveryOrderDAO> DbDeliveryOrderDAOs = await DataContext.DeliveryOrder
                .Where(x => x.Id, IdFilter)
                .ToListAsync();
            foreach (DeliveryOrder DeliveryOrder in DeliveryOrders)
            {
                DeliveryOrderDAO DeliveryOrderDAO = DbDeliveryOrderDAOs
                        .Where(x => x.Id == DeliveryOrder.Id)
                        .FirstOrDefault();
                if (DeliveryOrderDAO == null)
                {
                    DeliveryOrderDAO = new DeliveryOrderDAO();
                    DeliveryOrderDAO.CreatedAt = StaticParams.DateTimeNow;
                    Inserts.Add(DeliveryOrderDAO);
                }
                else
                {
                    Updates.Add(DeliveryOrderDAO);
                }
                DeliveryOrderDAO.Code = DeliveryOrder.Code;
                DeliveryOrderDAO.Weight = DeliveryOrder.Weight;
                DeliveryOrderDAO.CustomerId = DeliveryOrder.CustomerId;
                DeliveryOrderDAO.UpdatedAt = StaticParams.DateTimeNow;
            }
            await DataContext.DeliveryOrder.BulkInsertAsync(Inserts);
            await DataContext.DeliveryOrder.BulkMergeAsync(Updates);
            var Ids = Inserts.Select(x => x.Id).ToList();
            Ids.AddRange(Updates.Select(x => x.Id));
            Ids = Ids.Distinct().ToList();
            return Ids;
        }
        
        public async Task<bool> BulkDelete(List<DeliveryOrder> DeliveryOrders)
        {
            List<long> Ids = DeliveryOrders.Select(x => x.Id).ToList();
            await DataContext.DeliveryOrder
                .WhereBulkContains(Ids, x => x.Id)
                .UpdateFromQueryAsync(x => new DeliveryOrderDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        private async Task SaveReference(DeliveryOrder DeliveryOrder)
        {
        }

    }
}
