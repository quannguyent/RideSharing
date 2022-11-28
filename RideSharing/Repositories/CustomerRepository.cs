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
    public interface ICustomerRepository
    {
        Task<int> CountAll(CustomerFilter CustomerFilter);
        Task<int> Count(CustomerFilter CustomerFilter);
        Task<List<Customer>> List(CustomerFilter CustomerFilter);
        Task<List<Customer>> List(List<long> Ids);
        Task<Customer> Get(long Id);
        Task<bool> Create(Customer Customer);
        Task<bool> Update(Customer Customer);
        Task<bool> Delete(Customer Customer);
        Task<List<long>> BulkMerge(List<Customer> Customers);
        Task<bool> BulkDelete(List<Customer> Customers);
    }
    public class CustomerRepository : ICustomerRepository
    {
        private DataContext DataContext;
        public CustomerRepository(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        private async Task<IQueryable<CustomerDAO>> DynamicFilter(IQueryable<CustomerDAO> query, CustomerFilter filter)
        {
            if (filter == null)
                return query.Where(q => false);
            query = query.Where(q => !q.DeletedAt.HasValue);
            query = query.Where(q => q.CreatedAt, filter.CreatedAt);
            query = query.Where(q => q.UpdatedAt, filter.UpdatedAt);
            query = query.Where(q => q.Id, filter.Id);
            query = query.Where(q => q.Code, filter.Code);
            query = query.Where(q => q.Name, filter.Name);
            query = query.Where(q => q.NodeId, filter.NodeId);
            return query;
        }

        private async Task<IQueryable<CustomerDAO>> OrFilter(IQueryable<CustomerDAO> query, CustomerFilter filter)
        {
            if (filter.OrFilter == null || filter.OrFilter.Count == 0)
                return query;
            IQueryable<CustomerDAO> initQuery = query.Where(q => false);
            foreach (CustomerFilter CustomerFilter in filter.OrFilter)
            {
                IQueryable<CustomerDAO> queryable = query;
                queryable = queryable.Where(q => q.Id, CustomerFilter.Id);
                queryable = queryable.Where(q => q.Code, CustomerFilter.Code);
                queryable = queryable.Where(q => q.Name, CustomerFilter.Name);
                queryable = queryable.Where(q => q.NodeId, CustomerFilter.NodeId);
                initQuery = initQuery.Union(queryable);
            }
            return initQuery;
        }    

        private IQueryable<CustomerDAO> DynamicOrder(IQueryable<CustomerDAO> query, CustomerFilter filter)
        {
            switch (filter.OrderType)
            {
                case OrderType.ASC:
                    switch (filter.OrderBy)
                    {
                        case CustomerOrder.Id:
                            query = query.OrderBy(q => q.Id);
                            break;
                        case CustomerOrder.Code:
                            query = query.OrderBy(q => q.Code);
                            break;
                        case CustomerOrder.Name:
                            query = query.OrderBy(q => q.Name);
                            break;
                        case CustomerOrder.Node:
                            query = query.OrderBy(q => q.NodeId);
                            break;
                    }
                    break;
                case OrderType.DESC:
                    switch (filter.OrderBy)
                    {
                        case CustomerOrder.Id:
                            query = query.OrderByDescending(q => q.Id);
                            break;
                        case CustomerOrder.Code:
                            query = query.OrderByDescending(q => q.Code);
                            break;
                        case CustomerOrder.Name:
                            query = query.OrderByDescending(q => q.Name);
                            break;
                        case CustomerOrder.Node:
                            query = query.OrderByDescending(q => q.NodeId);
                            break;
                    }
                    break;
            }
            query = query.Skip(filter.Skip).Take(filter.Take);
            return query;
        }

        private async Task<List<Customer>> DynamicSelect(IQueryable<CustomerDAO> query, CustomerFilter filter)
        {
            List<Customer> Customers = await query.Select(q => new Customer()
            {
                Id = filter.Selects.Contains(CustomerSelect.Id) ? q.Id : default(long),
                Code = filter.Selects.Contains(CustomerSelect.Code) ? q.Code : default(string),
                Name = filter.Selects.Contains(CustomerSelect.Name) ? q.Name : default(string),
                NodeId = filter.Selects.Contains(CustomerSelect.Node) ? q.NodeId : default(long),
                Node = filter.Selects.Contains(CustomerSelect.Node) && q.Node != null ? new Node
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
            return Customers;
        }

        public async Task<int> CountAll(CustomerFilter filter)
        {
            IQueryable<CustomerDAO> CustomerDAOs = DataContext.Customer.AsNoTracking();
            CustomerDAOs = await DynamicFilter(CustomerDAOs, filter);
            return await CustomerDAOs.CountAsync();
        }

        public async Task<int> Count(CustomerFilter filter)
        {
            IQueryable<CustomerDAO> CustomerDAOs = DataContext.Customer.AsNoTracking();
            CustomerDAOs = await DynamicFilter(CustomerDAOs, filter);
            CustomerDAOs = await OrFilter(CustomerDAOs, filter);
            return await CustomerDAOs.CountAsync();
        }

        public async Task<List<Customer>> List(CustomerFilter filter)
        {
            if (filter == null) return new List<Customer>();
            IQueryable<CustomerDAO> CustomerDAOs = DataContext.Customer.AsNoTracking();
            CustomerDAOs = await DynamicFilter(CustomerDAOs, filter);
            CustomerDAOs = await OrFilter(CustomerDAOs, filter);
            CustomerDAOs = DynamicOrder(CustomerDAOs, filter);
            List<Customer> Customers = await DynamicSelect(CustomerDAOs, filter);
            return Customers;
        }

        public async Task<List<Customer>> List(List<long> Ids)
        {
            IdFilter IdFilter = new IdFilter { In = Ids };

            IQueryable<CustomerDAO> query = DataContext.Customer.AsNoTracking();
            query = query.Where(q => q.Id, IdFilter);
            List<Customer> Customers = await query.AsNoTracking()
            .Select(x => new Customer()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt,
                Id = x.Id,
                Code = x.Code,
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
            

            return Customers;
        }

        public async Task<Customer> Get(long Id)
        {
            Customer Customer = await DataContext.Customer.AsNoTracking()
            .Where(x => x.Id == Id)
            .Where(x => x.DeletedAt == null)
            .Select(x => new Customer()
            {
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Id = x.Id,
                Code = x.Code,
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

            if (Customer == null)
                return null;

            return Customer;
        }
        public async Task<bool> Create(Customer Customer)
        {
            CustomerDAO CustomerDAO = new CustomerDAO();
            CustomerDAO.Id = Customer.Id;
            CustomerDAO.Code = Customer.Code;
            CustomerDAO.Name = Customer.Name;
            CustomerDAO.NodeId = Customer.NodeId;
            CustomerDAO.CreatedAt = StaticParams.DateTimeNow;
            CustomerDAO.UpdatedAt = StaticParams.DateTimeNow;
            DataContext.Customer.Add(CustomerDAO);
            await DataContext.SaveChangesAsync();
            Customer.Id = CustomerDAO.Id;
            await SaveReference(Customer);
            return true;
        }

        public async Task<bool> Update(Customer Customer)
        {
            CustomerDAO CustomerDAO = DataContext.Customer
                .Where(x => x.Id == Customer.Id)
                .FirstOrDefault();
            if (CustomerDAO == null)
                return false;
            CustomerDAO.Id = Customer.Id;
            CustomerDAO.Code = Customer.Code;
            CustomerDAO.Name = Customer.Name;
            CustomerDAO.NodeId = Customer.NodeId;
            CustomerDAO.UpdatedAt = StaticParams.DateTimeNow;
            await DataContext.SaveChangesAsync();
            await SaveReference(Customer);
            return true;
        }

        public async Task<bool> Delete(Customer Customer)
        {
            await DataContext.Customer
                .Where(x => x.Id == Customer.Id)
                .UpdateFromQueryAsync(x => new CustomerDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        public async Task<List<long>> BulkMerge(List<Customer> Customers)
        {
            IdFilter IdFilter = new IdFilter { In = Customers.Select(x => x.Id).ToList() };
            List<CustomerDAO> Inserts = new List<CustomerDAO>();
            List<CustomerDAO> Updates = new List<CustomerDAO>();
            List<CustomerDAO> DbCustomerDAOs = await DataContext.Customer
                .Where(x => x.Id, IdFilter)
                .ToListAsync();
            foreach (Customer Customer in Customers)
            {
                CustomerDAO CustomerDAO = DbCustomerDAOs
                        .Where(x => x.Id == Customer.Id)
                        .FirstOrDefault();
                if (CustomerDAO == null)
                {
                    CustomerDAO = new CustomerDAO();
                    CustomerDAO.CreatedAt = StaticParams.DateTimeNow;
                    Inserts.Add(CustomerDAO);
                }
                else
                {
                    Updates.Add(CustomerDAO);
                }
                CustomerDAO.Code = Customer.Code;
                CustomerDAO.Name = Customer.Name;
                CustomerDAO.NodeId = Customer.NodeId;
                CustomerDAO.UpdatedAt = StaticParams.DateTimeNow;
            }
            await DataContext.Customer.BulkInsertAsync(Inserts);
            await DataContext.Customer.BulkMergeAsync(Updates);
            var Ids = Inserts.Select(x => x.Id).ToList();
            Ids.AddRange(Updates.Select(x => x.Id));
            Ids = Ids.Distinct().ToList();
            return Ids;
        }
        
        public async Task<bool> BulkDelete(List<Customer> Customers)
        {
            List<long> Ids = Customers.Select(x => x.Id).ToList();
            await DataContext.Customer
                .WhereBulkContains(Ids, x => x.Id)
                .UpdateFromQueryAsync(x => new CustomerDAO 
                { 
                    DeletedAt = StaticParams.DateTimeNow, 
                    UpdatedAt = StaticParams.DateTimeNow 
                });
            return true;
        }

        private async Task SaveReference(Customer Customer)
        {
        }

    }
}
