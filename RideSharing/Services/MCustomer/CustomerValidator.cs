using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RideSharing;
using RideSharing.Common;
using RideSharing.Enums;
using RideSharing.Entities;
using RideSharing.Repositories;

namespace RideSharing.Services.MCustomer
{
    public interface ICustomerValidator : IServiceScoped
    {
        Task Get(Customer Customer);
        Task<bool> Create(Customer Customer);
        Task<bool> Update(Customer Customer);
        Task<bool> Delete(Customer Customer);
        Task<bool> BulkDelete(List<Customer> Customers);
        Task<bool> Import(List<Customer> Customers);
    }

    public class CustomerValidator : ICustomerValidator
    {
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private CustomerMessage CustomerMessage;

        public CustomerValidator(IUOW UOW, ICurrentContext CurrentContext)
        {
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.CustomerMessage = new CustomerMessage();
        }

        public async Task Get(Customer Customer)
        {
        }

        public async Task<bool> Create(Customer Customer)
        {
            await ValidateCode(Customer);
            await ValidateName(Customer);
            await ValidateLatitude(Customer);
            await ValidateLongtitude(Customer);
            return Customer.IsValidated;
        }

        public async Task<bool> Update(Customer Customer)
        {
            if (await ValidateId(Customer))
            {
                await ValidateCode(Customer);
                await ValidateName(Customer);
                await ValidateLatitude(Customer);
                await ValidateLongtitude(Customer);
            }
            return Customer.IsValidated;
        }

        public async Task<bool> Delete(Customer Customer)
        {
            var oldData = await UOW.CustomerRepository.Get(Customer.Id);
            if (oldData != null)
            {
            }
            else
            {
                Customer.AddError(nameof(CustomerValidator), nameof(Customer.Id), CustomerMessage.Error.IdNotExisted, CustomerMessage);
            }
            return Customer.IsValidated;
        }
        
        public async Task<bool> BulkDelete(List<Customer> Customers)
        {
            return Customers.All(x => x.IsValidated);
        }

        public async Task<bool> Import(List<Customer> Customers)
        {
            return true;
        }
        
        private async Task<bool> ValidateId(Customer Customer)
        {
            CustomerFilter CustomerFilter = new CustomerFilter
            {
                Skip = 0,
                Take = 10,
                Id = new IdFilter { Equal = Customer.Id },
                Selects = CustomerSelect.Id
            };

            int count = await UOW.CustomerRepository.CountAll(CustomerFilter);
            if (count == 0)
                Customer.AddError(nameof(CustomerValidator), nameof(Customer.Id), CustomerMessage.Error.IdNotExisted, CustomerMessage);
            return Customer.IsValidated;
        }

        private async Task<bool> ValidateCode(Customer Customer)
        {
            if(string.IsNullOrEmpty(Customer.Code))
            {
                Customer.AddError(nameof(CustomerValidator), nameof(Customer.Code), CustomerMessage.Error.CodeEmpty, CustomerMessage);
            }
            else if(Customer.Code.Count() > 500)
            {
                Customer.AddError(nameof(CustomerValidator), nameof(Customer.Code), CustomerMessage.Error.CodeOverLength, CustomerMessage);
            }
            return Customer.IsValidated;
        }
        private async Task<bool> ValidateName(Customer Customer)
        {
            if(string.IsNullOrEmpty(Customer.Name))
            {
                Customer.AddError(nameof(CustomerValidator), nameof(Customer.Name), CustomerMessage.Error.NameEmpty, CustomerMessage);
            }
            else if(Customer.Name.Count() > 500)
            {
                Customer.AddError(nameof(CustomerValidator), nameof(Customer.Name), CustomerMessage.Error.NameOverLength, CustomerMessage);
            }
            return Customer.IsValidated;
        }
        private async Task<bool> ValidateLatitude(Customer Customer)
        {   
            return true;
        }
        private async Task<bool> ValidateLongtitude(Customer Customer)
        {   
            return true;
        }
    }
}
