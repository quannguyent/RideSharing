using TrueSight.Common;
using RideSharing.Handlers.Configuration;
using RideSharing.Common;
using RideSharing.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using RideSharing.Repositories;
using RideSharing.Entities;
using RideSharing.Enums;

namespace RideSharing.Services.MCustomer
{
    public interface ICustomerService :  IServiceScoped
    {
        Task<int> Count(CustomerFilter CustomerFilter);
        Task<List<Customer>> List(CustomerFilter CustomerFilter);
        Task<Customer> Get(long Id);
        Task<Customer> Create(Customer Customer);
        Task<Customer> Update(Customer Customer);
        Task<Customer> Delete(Customer Customer);
        Task<List<Customer>> BulkDelete(List<Customer> Customers);
        Task<List<Customer>> BulkMerge(List<Customer> Customers);
        Task<CustomerFilter> ToFilter(CustomerFilter CustomerFilter);
    }

    public class CustomerService : BaseService, ICustomerService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ILogging Logging;
        private ICurrentContext CurrentContext;
        
        private ICustomerValidator CustomerValidator;

        public CustomerService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            ICustomerValidator CustomerValidator,
            ILogging Logging
        )
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.Logging = Logging;
           
            this.CustomerValidator = CustomerValidator;
        }

        public async Task<int> Count(CustomerFilter CustomerFilter)
        {
            try
            {
                int result = await UOW.CustomerRepository.Count(CustomerFilter);
                return result;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CustomerService));
            }
            return 0;
        }

        public async Task<List<Customer>> List(CustomerFilter CustomerFilter)
        {
            try
            {
                List<Customer> Customers = await UOW.CustomerRepository.List(CustomerFilter);
                return Customers;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CustomerService));
            }
            return null;
        }

        public async Task<Customer> Get(long Id)
        {
            Customer Customer = await UOW.CustomerRepository.Get(Id);
            if (Customer == null)
                return null;
            await CustomerValidator.Get(Customer);
            return Customer;
        }
        
        public async Task<Customer> Create(Customer Customer)
        {
            if (!await CustomerValidator.Create(Customer))
                return Customer;

            try
            {
                await UOW.CustomerRepository.Create(Customer);
                Customer = await UOW.CustomerRepository.Get(Customer.Id);
                Sync(new List<Customer> { Customer });
                return Customer;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CustomerService));
            }
            return null;
        }

        public async Task<Customer> Update(Customer Customer)
        {
            if (!await CustomerValidator.Update(Customer))
                return Customer;
            try
            {
                var oldData = await UOW.CustomerRepository.Get(Customer.Id);

                await UOW.CustomerRepository.Update(Customer);

                Customer = await UOW.CustomerRepository.Get(Customer.Id);
                Sync(new List<Customer> { Customer });
                return Customer;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CustomerService));
            }
            return null;
        }

        public async Task<Customer> Delete(Customer Customer)
        {
            if (!await CustomerValidator.Delete(Customer))
                return Customer;

            try
            {
                await UOW.CustomerRepository.Delete(Customer);
                return Customer;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CustomerService));
            }
            return null;
        }

        public async Task<List<Customer>> BulkDelete(List<Customer> Customers)
        {
            if (!await CustomerValidator.BulkDelete(Customers))
                return Customers;

            try
            {
                await UOW.CustomerRepository.BulkDelete(Customers);
                return Customers;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CustomerService));
            }
            return null;
        }

        public async Task<List<Customer>> BulkMerge(List<Customer> Customers)
        {
            if (!await CustomerValidator.Import(Customers))
                return Customers;
            try
            {
                var Ids = await UOW.CustomerRepository.BulkMerge(Customers);
                Customers = await UOW.CustomerRepository.List(Ids);
                Sync(Customers);
                return Customers;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(CustomerService));
            }
            return null;
        }     
        
        public async Task<CustomerFilter> ToFilter(CustomerFilter filter)
        {
            if (filter.OrFilter == null) filter.OrFilter = new List<CustomerFilter>();
            if (CurrentContext.Filters == null || CurrentContext.Filters.Count == 0) return filter;
            foreach (var currentFilter in CurrentContext.Filters)
            {
                CustomerFilter subFilter = new CustomerFilter();
                filter.OrFilter.Add(subFilter);
                List<FilterPermissionDefinition> FilterPermissionDefinitions = currentFilter.Value;
                foreach (FilterPermissionDefinition FilterPermissionDefinition in FilterPermissionDefinitions)
                {
                    if (FilterPermissionDefinition.Name == "CustomerId")
                        subFilter.Id = FilterBuilder.Merge(subFilter.Id, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Code))
                        subFilter.Code = FilterBuilder.Merge(subFilter.Code, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Name))
                        subFilter.Name = FilterBuilder.Merge(subFilter.Name, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.NodeId))
                        subFilter.NodeId = FilterBuilder.Merge(subFilter.NodeId, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(CurrentContext.UserId) && FilterPermissionDefinition.IdFilter != null)
                    {
                        if (FilterPermissionDefinition.IdFilter.Equal.HasValue && FilterPermissionDefinition.IdFilter.Equal.Value == CurrentUserEnum.IS.Id)
                        {
                        }
                        if (FilterPermissionDefinition.IdFilter.Equal.HasValue && FilterPermissionDefinition.IdFilter.Equal.Value == CurrentUserEnum.ISNT.Id)
                        {
                        }
                    }
                }
            }
            return filter;
        }

        private void Sync(List<Customer> Customers)
        {


            
        }
    }
}
