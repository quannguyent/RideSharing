using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RideSharing.Common;
using RideSharing.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using OfficeOpenXml;
using System.Dynamic;
using System.Net;
using NGS.Templater;
using RideSharing.Entities;
using RideSharing.Services.MCustomer;

namespace RideSharing.Rpc.customer
{
    public partial class CustomerController : RpcController
    {
        private ICustomerService CustomerService;
        private ICurrentContext CurrentContext;
        public CustomerController(
            ICustomerService CustomerService,
            ICurrentContext CurrentContext
        )
        {
            this.CustomerService = CustomerService;
            this.CurrentContext = CurrentContext;
        }

        [Route(CustomerRoute.Count), HttpPost]
        public async Task<ActionResult<int>> Count([FromBody] Customer_CustomerFilterDTO Customer_CustomerFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CustomerFilter CustomerFilter = ConvertFilterDTOToFilterEntity(Customer_CustomerFilterDTO);
            CustomerFilter = await CustomerService.ToFilter(CustomerFilter);
            int count = await CustomerService.Count(CustomerFilter);
            return count;
        }

        [Route(CustomerRoute.List), HttpPost]
        public async Task<ActionResult<List<Customer_CustomerDTO>>> List([FromBody] Customer_CustomerFilterDTO Customer_CustomerFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CustomerFilter CustomerFilter = ConvertFilterDTOToFilterEntity(Customer_CustomerFilterDTO);
            CustomerFilter = await CustomerService.ToFilter(CustomerFilter);
            List<Customer> Customers = await CustomerService.List(CustomerFilter);
            List<Customer_CustomerDTO> Customer_CustomerDTOs = Customers
                .Select(c => new Customer_CustomerDTO(c)).ToList();
            return Customer_CustomerDTOs;
        }

        [Route(CustomerRoute.Get), HttpPost]
        public async Task<ActionResult<Customer_CustomerDTO>> Get([FromBody]Customer_CustomerDTO Customer_CustomerDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(Customer_CustomerDTO.Id))
                return Forbid();

            Customer Customer = await CustomerService.Get(Customer_CustomerDTO.Id);
            return new Customer_CustomerDTO(Customer);
        }

        [Route(CustomerRoute.Create), HttpPost]
        public async Task<ActionResult<Customer_CustomerDTO>> Create([FromBody] Customer_CustomerDTO Customer_CustomerDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(Customer_CustomerDTO.Id))
                return Forbid();

            Customer Customer = ConvertDTOToEntity(Customer_CustomerDTO);
            Customer = await CustomerService.Create(Customer);
            Customer_CustomerDTO = new Customer_CustomerDTO(Customer);
            if (Customer.IsValidated)
                return Customer_CustomerDTO;
            else
                return BadRequest(Customer_CustomerDTO);
        }

        [Route(CustomerRoute.Update), HttpPost]
        public async Task<ActionResult<Customer_CustomerDTO>> Update([FromBody] Customer_CustomerDTO Customer_CustomerDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(Customer_CustomerDTO.Id))
                return Forbid();

            Customer Customer = ConvertDTOToEntity(Customer_CustomerDTO);
            Customer = await CustomerService.Update(Customer);
            Customer_CustomerDTO = new Customer_CustomerDTO(Customer);
            if (Customer.IsValidated)
                return Customer_CustomerDTO;
            else
                return BadRequest(Customer_CustomerDTO);
        }

        [Route(CustomerRoute.Delete), HttpPost]
        public async Task<ActionResult<Customer_CustomerDTO>> Delete([FromBody] Customer_CustomerDTO Customer_CustomerDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(Customer_CustomerDTO.Id))
                return Forbid();

            Customer Customer = ConvertDTOToEntity(Customer_CustomerDTO);
            Customer = await CustomerService.Delete(Customer);
            Customer_CustomerDTO = new Customer_CustomerDTO(Customer);
            if (Customer.IsValidated)
                return Customer_CustomerDTO;
            else
                return BadRequest(Customer_CustomerDTO);
        }
        
        [Route(CustomerRoute.BulkDelete), HttpPost]
        public async Task<ActionResult<bool>> BulkDelete([FromBody] List<long> Ids)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CustomerFilter CustomerFilter = new CustomerFilter();
            CustomerFilter = await CustomerService.ToFilter(CustomerFilter);
            CustomerFilter.Id = new IdFilter { In = Ids };
            CustomerFilter.Selects = CustomerSelect.Id;
            CustomerFilter.Skip = 0;
            CustomerFilter.Take = int.MaxValue;

            List<Customer> Customers = await CustomerService.List(CustomerFilter);
            Customers = await CustomerService.BulkDelete(Customers);
            if (Customers.Any(x => !x.IsValidated))
                return BadRequest(Customers.Where(x => !x.IsValidated));
            return true;
        }
        
        [Route(CustomerRoute.Import), HttpPost]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            List<Customer> Customers = new List<Customer>();
            using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return Ok(Customers);
                int StartColumn = 1;
                int StartRow = 1;
                int IdColumn = 0 + StartColumn;
                int CodeColumn = 1 + StartColumn;
                int NameColumn = 2 + StartColumn;
                int LatitudeColumn = 3 + StartColumn;
                int LongtitudeColumn = 4 + StartColumn;

                for (int i = StartRow; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(worksheet.Cells[i, StartColumn].Value?.ToString()))
                        break;
                    string IdValue = worksheet.Cells[i, IdColumn].Value?.ToString();
                    string CodeValue = worksheet.Cells[i, CodeColumn].Value?.ToString();
                    string NameValue = worksheet.Cells[i, NameColumn].Value?.ToString();
                    string LatitudeValue = worksheet.Cells[i, LatitudeColumn].Value?.ToString();
                    string LongtitudeValue = worksheet.Cells[i, LongtitudeColumn].Value?.ToString();
                    
                    Customer Customer = new Customer();
                    Customer.Code = CodeValue;
                    Customer.Name = NameValue;
                    Customer.Latitude = decimal.TryParse(LatitudeValue, out decimal Latitude) ? Latitude : 0;
                    Customer.Longtitude = decimal.TryParse(LongtitudeValue, out decimal Longtitude) ? Longtitude : 0;
                    
                    Customers.Add(Customer);
                }
            }
            Customers = await CustomerService.BulkMerge(Customers);
            if (Customers.All(x => x.IsValidated))
                return Ok(true);
            else
            {
                List<string> Errors = new List<string>();
                for (int i = 0; i < Customers.Count; i++)
                {
                    Customer Customer = Customers[i];
                    if (!Customer.IsValidated)
                    {
                        string Error = $"Dòng {i + 2} có lỗi:";
                        if (Customer.Errors.ContainsKey(nameof(Customer.Id).Camelize()))
                            Error += Customer.Errors[nameof(Customer.Id)];
                        if (Customer.Errors.ContainsKey(nameof(Customer.Code).Camelize()))
                            Error += Customer.Errors[nameof(Customer.Code)];
                        if (Customer.Errors.ContainsKey(nameof(Customer.Name).Camelize()))
                            Error += Customer.Errors[nameof(Customer.Name)];
                        if (Customer.Errors.ContainsKey(nameof(Customer.Latitude).Camelize()))
                            Error += Customer.Errors[nameof(Customer.Latitude)];
                        if (Customer.Errors.ContainsKey(nameof(Customer.Longtitude).Camelize()))
                            Error += Customer.Errors[nameof(Customer.Longtitude)];
                        Errors.Add(Error);
                    }
                }
                return BadRequest(Errors);
            }
        }
        
        [Route(CustomerRoute.Export), HttpPost]
        public async Task<ActionResult> Export([FromBody] Customer_CustomerFilterDTO Customer_CustomerFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            var CustomerFilter = ConvertFilterDTOToFilterEntity(Customer_CustomerFilterDTO);
            CustomerFilter.Skip = 0;
            CustomerFilter.Take = int.MaxValue;
            CustomerFilter = await CustomerService.ToFilter(CustomerFilter);
            List<Customer> Customers = await CustomerService.List(CustomerFilter);
            List<Customer_CustomerExportDTO> Customer_CustomerExportDTOs = Customers.Select(x => new Customer_CustomerExportDTO(x)).ToList();    

            string path = "Templates/Customer_Export.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            Data.Data = Customer_CustomerExportDTOs;

            #region Organization
            //Data.Root = OrganizationRoot;
            #endregion

            #region Image
            //WebClient WebClient = new WebClient();
            //byte[] array = WebClient.DownloadData(""); //link image
            //MemoryStream MemoryStream = new MemoryStream(array);
            //ImageInfo image = new ImageInfo(MemoryStream, "png", 300, 300, 1000);
            //Data.Image = image;
            #endregion

            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {    
                document.Process(Data);
            }
            return File(output.ToArray(), "application/octet-stream", "Customer.xlsx");
        }

        [Route(CustomerRoute.ExportTemplate), HttpPost]
        public async Task<ActionResult> ExportTemplate([FromBody] Customer_CustomerFilterDTO Customer_CustomerFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            string path = "Templates/Customer_Template.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {
                document.Process(Data);
            };
            return File(output.ToArray(), "application/octet-stream", "Customer.xlsx");
        }

        private async Task<bool> HasPermission(long Id)
        {
            CustomerFilter CustomerFilter = new CustomerFilter();
            CustomerFilter = await CustomerService.ToFilter(CustomerFilter);
            if (Id == 0)
            {

            }
            else
            {
                CustomerFilter.Id = new IdFilter { Equal = Id };
                int count = await CustomerService.Count(CustomerFilter);
                if (count == 0)
                    return false;
            }
            return true;
        }

        private Customer ConvertDTOToEntity(Customer_CustomerDTO Customer_CustomerDTO)
        {
            Customer_CustomerDTO.TrimString();
            Customer Customer = new Customer();
            Customer.Id = Customer_CustomerDTO.Id;
            Customer.Code = Customer_CustomerDTO.Code;
            Customer.Name = Customer_CustomerDTO.Name;
            Customer.Latitude = Customer_CustomerDTO.Latitude;
            Customer.Longtitude = Customer_CustomerDTO.Longtitude;
            Customer.BaseLanguage = CurrentContext.Language;
            return Customer;
        }

        private CustomerFilter ConvertFilterDTOToFilterEntity(Customer_CustomerFilterDTO Customer_CustomerFilterDTO)
        {
            CustomerFilter CustomerFilter = new CustomerFilter();
            CustomerFilter.Selects = CustomerSelect.ALL;
            CustomerFilter.Skip = Customer_CustomerFilterDTO.Skip;
            CustomerFilter.Take = Customer_CustomerFilterDTO.Take;
            CustomerFilter.OrderBy = Customer_CustomerFilterDTO.OrderBy;
            CustomerFilter.OrderType = Customer_CustomerFilterDTO.OrderType;

            CustomerFilter.Id = Customer_CustomerFilterDTO.Id;
            CustomerFilter.Code = Customer_CustomerFilterDTO.Code;
            CustomerFilter.Name = Customer_CustomerFilterDTO.Name;
            CustomerFilter.Latitude = Customer_CustomerFilterDTO.Latitude;
            CustomerFilter.Longtitude = Customer_CustomerFilterDTO.Longtitude;
            CustomerFilter.CreatedAt = Customer_CustomerFilterDTO.CreatedAt;
            CustomerFilter.UpdatedAt = Customer_CustomerFilterDTO.UpdatedAt;
            return CustomerFilter;
        }
    }
}

