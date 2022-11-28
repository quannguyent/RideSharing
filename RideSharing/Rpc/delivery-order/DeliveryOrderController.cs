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
using RideSharing.Services.MDeliveryOrder;
using RideSharing.Services.MCustomer;

namespace RideSharing.Rpc.delivery_order
{
    public partial class DeliveryOrderController : RpcController
    {
        private ICustomerService CustomerService;
        private IDeliveryOrderService DeliveryOrderService;
        private ICurrentContext CurrentContext;
        public DeliveryOrderController(
            ICustomerService CustomerService,
            IDeliveryOrderService DeliveryOrderService,
            ICurrentContext CurrentContext
        )
        {
            this.CustomerService = CustomerService;
            this.DeliveryOrderService = DeliveryOrderService;
            this.CurrentContext = CurrentContext;
        }

        [Route(DeliveryOrderRoute.Count), HttpPost]
        public async Task<ActionResult<int>> Count([FromBody] DeliveryOrder_DeliveryOrderFilterDTO DeliveryOrder_DeliveryOrderFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryOrderFilter DeliveryOrderFilter = ConvertFilterDTOToFilterEntity(DeliveryOrder_DeliveryOrderFilterDTO);
            DeliveryOrderFilter = await DeliveryOrderService.ToFilter(DeliveryOrderFilter);
            int count = await DeliveryOrderService.Count(DeliveryOrderFilter);
            return count;
        }

        [Route(DeliveryOrderRoute.List), HttpPost]
        public async Task<ActionResult<List<DeliveryOrder_DeliveryOrderDTO>>> List([FromBody] DeliveryOrder_DeliveryOrderFilterDTO DeliveryOrder_DeliveryOrderFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryOrderFilter DeliveryOrderFilter = ConvertFilterDTOToFilterEntity(DeliveryOrder_DeliveryOrderFilterDTO);
            DeliveryOrderFilter = await DeliveryOrderService.ToFilter(DeliveryOrderFilter);
            List<DeliveryOrder> DeliveryOrders = await DeliveryOrderService.List(DeliveryOrderFilter);
            List<DeliveryOrder_DeliveryOrderDTO> DeliveryOrder_DeliveryOrderDTOs = DeliveryOrders
                .Select(c => new DeliveryOrder_DeliveryOrderDTO(c)).ToList();
            return DeliveryOrder_DeliveryOrderDTOs;
        }

        [Route(DeliveryOrderRoute.Get), HttpPost]
        public async Task<ActionResult<DeliveryOrder_DeliveryOrderDTO>> Get([FromBody]DeliveryOrder_DeliveryOrderDTO DeliveryOrder_DeliveryOrderDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(DeliveryOrder_DeliveryOrderDTO.Id))
                return Forbid();

            DeliveryOrder DeliveryOrder = await DeliveryOrderService.Get(DeliveryOrder_DeliveryOrderDTO.Id);
            return new DeliveryOrder_DeliveryOrderDTO(DeliveryOrder);
        }

        [Route(DeliveryOrderRoute.Create), HttpPost]
        public async Task<ActionResult<DeliveryOrder_DeliveryOrderDTO>> Create([FromBody] DeliveryOrder_DeliveryOrderDTO DeliveryOrder_DeliveryOrderDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(DeliveryOrder_DeliveryOrderDTO.Id))
                return Forbid();

            DeliveryOrder DeliveryOrder = ConvertDTOToEntity(DeliveryOrder_DeliveryOrderDTO);
            DeliveryOrder = await DeliveryOrderService.Create(DeliveryOrder);
            DeliveryOrder_DeliveryOrderDTO = new DeliveryOrder_DeliveryOrderDTO(DeliveryOrder);
            if (DeliveryOrder.IsValidated)
                return DeliveryOrder_DeliveryOrderDTO;
            else
                return BadRequest(DeliveryOrder_DeliveryOrderDTO);
        }

        [Route(DeliveryOrderRoute.Update), HttpPost]
        public async Task<ActionResult<DeliveryOrder_DeliveryOrderDTO>> Update([FromBody] DeliveryOrder_DeliveryOrderDTO DeliveryOrder_DeliveryOrderDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(DeliveryOrder_DeliveryOrderDTO.Id))
                return Forbid();

            DeliveryOrder DeliveryOrder = ConvertDTOToEntity(DeliveryOrder_DeliveryOrderDTO);
            DeliveryOrder = await DeliveryOrderService.Update(DeliveryOrder);
            DeliveryOrder_DeliveryOrderDTO = new DeliveryOrder_DeliveryOrderDTO(DeliveryOrder);
            if (DeliveryOrder.IsValidated)
                return DeliveryOrder_DeliveryOrderDTO;
            else
                return BadRequest(DeliveryOrder_DeliveryOrderDTO);
        }

        [Route(DeliveryOrderRoute.Delete), HttpPost]
        public async Task<ActionResult<DeliveryOrder_DeliveryOrderDTO>> Delete([FromBody] DeliveryOrder_DeliveryOrderDTO DeliveryOrder_DeliveryOrderDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(DeliveryOrder_DeliveryOrderDTO.Id))
                return Forbid();

            DeliveryOrder DeliveryOrder = ConvertDTOToEntity(DeliveryOrder_DeliveryOrderDTO);
            DeliveryOrder = await DeliveryOrderService.Delete(DeliveryOrder);
            DeliveryOrder_DeliveryOrderDTO = new DeliveryOrder_DeliveryOrderDTO(DeliveryOrder);
            if (DeliveryOrder.IsValidated)
                return DeliveryOrder_DeliveryOrderDTO;
            else
                return BadRequest(DeliveryOrder_DeliveryOrderDTO);
        }
        
        [Route(DeliveryOrderRoute.BulkDelete), HttpPost]
        public async Task<ActionResult<bool>> BulkDelete([FromBody] List<long> Ids)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryOrderFilter DeliveryOrderFilter = new DeliveryOrderFilter();
            DeliveryOrderFilter = await DeliveryOrderService.ToFilter(DeliveryOrderFilter);
            DeliveryOrderFilter.Id = new IdFilter { In = Ids };
            DeliveryOrderFilter.Selects = DeliveryOrderSelect.Id;
            DeliveryOrderFilter.Skip = 0;
            DeliveryOrderFilter.Take = int.MaxValue;

            List<DeliveryOrder> DeliveryOrders = await DeliveryOrderService.List(DeliveryOrderFilter);
            DeliveryOrders = await DeliveryOrderService.BulkDelete(DeliveryOrders);
            if (DeliveryOrders.Any(x => !x.IsValidated))
                return BadRequest(DeliveryOrders.Where(x => !x.IsValidated));
            return true;
        }
        
        [Route(DeliveryOrderRoute.Import), HttpPost]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            CustomerFilter CustomerFilter = new CustomerFilter
            {
                Skip = 0,
                Take = int.MaxValue,
                Selects = CustomerSelect.ALL
            };
            List<Customer> Customers = await CustomerService.List(CustomerFilter);
            List<DeliveryOrder> DeliveryOrders = new List<DeliveryOrder>();
            using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return Ok(DeliveryOrders);
                int StartColumn = 1;
                int StartRow = 1;
                int IdColumn = 0 + StartColumn;
                int CodeColumn = 1 + StartColumn;
                int WeightColumn = 2 + StartColumn;
                int CustomerIdColumn = 3 + StartColumn;

                for (int i = StartRow; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(worksheet.Cells[i, StartColumn].Value?.ToString()))
                        break;
                    string IdValue = worksheet.Cells[i, IdColumn].Value?.ToString();
                    string CodeValue = worksheet.Cells[i, CodeColumn].Value?.ToString();
                    string WeightValue = worksheet.Cells[i, WeightColumn].Value?.ToString();
                    string CustomerIdValue = worksheet.Cells[i, CustomerIdColumn].Value?.ToString();
                    
                    DeliveryOrder DeliveryOrder = new DeliveryOrder();
                    DeliveryOrder.Code = CodeValue;
                    DeliveryOrder.Weight = decimal.TryParse(WeightValue, out decimal Weight) ? Weight : 0;
                    Customer Customer = Customers.Where(x => x.Id.ToString() == CustomerIdValue).FirstOrDefault();
                    DeliveryOrder.CustomerId = Customer == null ? 0 : Customer.Id;
                    DeliveryOrder.Customer = Customer;
                    
                    DeliveryOrders.Add(DeliveryOrder);
                }
            }
            DeliveryOrders = await DeliveryOrderService.BulkMerge(DeliveryOrders);
            if (DeliveryOrders.All(x => x.IsValidated))
                return Ok(true);
            else
            {
                List<string> Errors = new List<string>();
                for (int i = 0; i < DeliveryOrders.Count; i++)
                {
                    DeliveryOrder DeliveryOrder = DeliveryOrders[i];
                    if (!DeliveryOrder.IsValidated)
                    {
                        string Error = $"Dòng {i + 2} có lỗi:";
                        if (DeliveryOrder.Errors.ContainsKey(nameof(DeliveryOrder.Id).Camelize()))
                            Error += DeliveryOrder.Errors[nameof(DeliveryOrder.Id)];
                        if (DeliveryOrder.Errors.ContainsKey(nameof(DeliveryOrder.Code).Camelize()))
                            Error += DeliveryOrder.Errors[nameof(DeliveryOrder.Code)];
                        if (DeliveryOrder.Errors.ContainsKey(nameof(DeliveryOrder.Weight).Camelize()))
                            Error += DeliveryOrder.Errors[nameof(DeliveryOrder.Weight)];
                        if (DeliveryOrder.Errors.ContainsKey(nameof(DeliveryOrder.CustomerId).Camelize()))
                            Error += DeliveryOrder.Errors[nameof(DeliveryOrder.CustomerId)];
                        Errors.Add(Error);
                    }
                }
                return BadRequest(Errors);
            }
        }
        
        [Route(DeliveryOrderRoute.Export), HttpPost]
        public async Task<ActionResult> Export([FromBody] DeliveryOrder_DeliveryOrderFilterDTO DeliveryOrder_DeliveryOrderFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            var DeliveryOrderFilter = ConvertFilterDTOToFilterEntity(DeliveryOrder_DeliveryOrderFilterDTO);
            DeliveryOrderFilter.Skip = 0;
            DeliveryOrderFilter.Take = int.MaxValue;
            DeliveryOrderFilter = await DeliveryOrderService.ToFilter(DeliveryOrderFilter);
            List<DeliveryOrder> DeliveryOrders = await DeliveryOrderService.List(DeliveryOrderFilter);
            List<DeliveryOrder_DeliveryOrderExportDTO> DeliveryOrder_DeliveryOrderExportDTOs = DeliveryOrders.Select(x => new DeliveryOrder_DeliveryOrderExportDTO(x)).ToList();    

            string path = "Templates/DeliveryOrder_Export.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            Data.Data = DeliveryOrder_DeliveryOrderExportDTOs;

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
            return File(output.ToArray(), "application/octet-stream", "DeliveryOrder.xlsx");
        }

        [Route(DeliveryOrderRoute.ExportTemplate), HttpPost]
        public async Task<ActionResult> ExportTemplate([FromBody] DeliveryOrder_DeliveryOrderFilterDTO DeliveryOrder_DeliveryOrderFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            string path = "Templates/DeliveryOrder_Template.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {
                document.Process(Data);
            };
            return File(output.ToArray(), "application/octet-stream", "DeliveryOrder.xlsx");
        }

        private async Task<bool> HasPermission(long Id)
        {
            DeliveryOrderFilter DeliveryOrderFilter = new DeliveryOrderFilter();
            DeliveryOrderFilter = await DeliveryOrderService.ToFilter(DeliveryOrderFilter);
            if (Id == 0)
            {

            }
            else
            {
                DeliveryOrderFilter.Id = new IdFilter { Equal = Id };
                int count = await DeliveryOrderService.Count(DeliveryOrderFilter);
                if (count == 0)
                    return false;
            }
            return true;
        }

        private DeliveryOrder ConvertDTOToEntity(DeliveryOrder_DeliveryOrderDTO DeliveryOrder_DeliveryOrderDTO)
        {
            DeliveryOrder_DeliveryOrderDTO.TrimString();
            DeliveryOrder DeliveryOrder = new DeliveryOrder();
            DeliveryOrder.Id = DeliveryOrder_DeliveryOrderDTO.Id;
            DeliveryOrder.Code = DeliveryOrder_DeliveryOrderDTO.Code;
            DeliveryOrder.Weight = DeliveryOrder_DeliveryOrderDTO.Weight;
            DeliveryOrder.CustomerId = DeliveryOrder_DeliveryOrderDTO.CustomerId;
            DeliveryOrder.Customer = DeliveryOrder_DeliveryOrderDTO.Customer == null ? null : new Customer
            {
                Id = DeliveryOrder_DeliveryOrderDTO.Customer.Id,
                Code = DeliveryOrder_DeliveryOrderDTO.Customer.Code,
                Name = DeliveryOrder_DeliveryOrderDTO.Customer.Name,
                Latitude = DeliveryOrder_DeliveryOrderDTO.Customer.Latitude,
                Longtitude = DeliveryOrder_DeliveryOrderDTO.Customer.Longtitude,
            };
            DeliveryOrder.BaseLanguage = CurrentContext.Language;
            return DeliveryOrder;
        }

        private DeliveryOrderFilter ConvertFilterDTOToFilterEntity(DeliveryOrder_DeliveryOrderFilterDTO DeliveryOrder_DeliveryOrderFilterDTO)
        {
            DeliveryOrderFilter DeliveryOrderFilter = new DeliveryOrderFilter();
            DeliveryOrderFilter.Selects = DeliveryOrderSelect.ALL;
            DeliveryOrderFilter.Skip = DeliveryOrder_DeliveryOrderFilterDTO.Skip;
            DeliveryOrderFilter.Take = DeliveryOrder_DeliveryOrderFilterDTO.Take;
            DeliveryOrderFilter.OrderBy = DeliveryOrder_DeliveryOrderFilterDTO.OrderBy;
            DeliveryOrderFilter.OrderType = DeliveryOrder_DeliveryOrderFilterDTO.OrderType;

            DeliveryOrderFilter.Id = DeliveryOrder_DeliveryOrderFilterDTO.Id;
            DeliveryOrderFilter.Code = DeliveryOrder_DeliveryOrderFilterDTO.Code;
            DeliveryOrderFilter.Weight = DeliveryOrder_DeliveryOrderFilterDTO.Weight;
            DeliveryOrderFilter.CustomerId = DeliveryOrder_DeliveryOrderFilterDTO.CustomerId;
            DeliveryOrderFilter.CreatedAt = DeliveryOrder_DeliveryOrderFilterDTO.CreatedAt;
            DeliveryOrderFilter.UpdatedAt = DeliveryOrder_DeliveryOrderFilterDTO.UpdatedAt;
            return DeliveryOrderFilter;
        }
    }
}

