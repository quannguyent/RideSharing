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
using RideSharing.Services.MBusStop;

namespace RideSharing.Rpc.bus_stop
{
    public partial class BusStopController : RpcController
    {
        private IBusStopService BusStopService;
        private ICurrentContext CurrentContext;
        public BusStopController(
            IBusStopService BusStopService,
            ICurrentContext CurrentContext
        )
        {
            this.BusStopService = BusStopService;
            this.CurrentContext = CurrentContext;
        }

        [Route(BusStopRoute.Count), HttpPost]
        public async Task<ActionResult<int>> Count([FromBody] BusStop_BusStopFilterDTO BusStop_BusStopFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            BusStopFilter BusStopFilter = ConvertFilterDTOToFilterEntity(BusStop_BusStopFilterDTO);
            BusStopFilter = await BusStopService.ToFilter(BusStopFilter);
            int count = await BusStopService.Count(BusStopFilter);
            return count;
        }

        [Route(BusStopRoute.List), HttpPost]
        public async Task<ActionResult<List<BusStop_BusStopDTO>>> List([FromBody] BusStop_BusStopFilterDTO BusStop_BusStopFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            BusStopFilter BusStopFilter = ConvertFilterDTOToFilterEntity(BusStop_BusStopFilterDTO);
            BusStopFilter = await BusStopService.ToFilter(BusStopFilter);
            List<BusStop> BusStops = await BusStopService.List(BusStopFilter);
            List<BusStop_BusStopDTO> BusStop_BusStopDTOs = BusStops
                .Select(c => new BusStop_BusStopDTO(c)).ToList();
            return BusStop_BusStopDTOs;
        }

        [Route(BusStopRoute.Get), HttpPost]
        public async Task<ActionResult<BusStop_BusStopDTO>> Get([FromBody]BusStop_BusStopDTO BusStop_BusStopDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(BusStop_BusStopDTO.Id))
                return Forbid();

            BusStop BusStop = await BusStopService.Get(BusStop_BusStopDTO.Id);
            return new BusStop_BusStopDTO(BusStop);
        }

        [Route(BusStopRoute.Create), HttpPost]
        public async Task<ActionResult<BusStop_BusStopDTO>> Create([FromBody] BusStop_BusStopDTO BusStop_BusStopDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(BusStop_BusStopDTO.Id))
                return Forbid();

            BusStop BusStop = ConvertDTOToEntity(BusStop_BusStopDTO);
            BusStop = await BusStopService.Create(BusStop);
            BusStop_BusStopDTO = new BusStop_BusStopDTO(BusStop);
            if (BusStop.IsValidated)
                return BusStop_BusStopDTO;
            else
                return BadRequest(BusStop_BusStopDTO);
        }

        [Route(BusStopRoute.Update), HttpPost]
        public async Task<ActionResult<BusStop_BusStopDTO>> Update([FromBody] BusStop_BusStopDTO BusStop_BusStopDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(BusStop_BusStopDTO.Id))
                return Forbid();

            BusStop BusStop = ConvertDTOToEntity(BusStop_BusStopDTO);
            BusStop = await BusStopService.Update(BusStop);
            BusStop_BusStopDTO = new BusStop_BusStopDTO(BusStop);
            if (BusStop.IsValidated)
                return BusStop_BusStopDTO;
            else
                return BadRequest(BusStop_BusStopDTO);
        }

        [Route(BusStopRoute.Delete), HttpPost]
        public async Task<ActionResult<BusStop_BusStopDTO>> Delete([FromBody] BusStop_BusStopDTO BusStop_BusStopDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(BusStop_BusStopDTO.Id))
                return Forbid();

            BusStop BusStop = ConvertDTOToEntity(BusStop_BusStopDTO);
            BusStop = await BusStopService.Delete(BusStop);
            BusStop_BusStopDTO = new BusStop_BusStopDTO(BusStop);
            if (BusStop.IsValidated)
                return BusStop_BusStopDTO;
            else
                return BadRequest(BusStop_BusStopDTO);
        }
        
        [Route(BusStopRoute.BulkDelete), HttpPost]
        public async Task<ActionResult<bool>> BulkDelete([FromBody] List<long> Ids)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            BusStopFilter BusStopFilter = new BusStopFilter();
            BusStopFilter = await BusStopService.ToFilter(BusStopFilter);
            BusStopFilter.Id = new IdFilter { In = Ids };
            BusStopFilter.Selects = BusStopSelect.Id;
            BusStopFilter.Skip = 0;
            BusStopFilter.Take = int.MaxValue;

            List<BusStop> BusStops = await BusStopService.List(BusStopFilter);
            BusStops = await BusStopService.BulkDelete(BusStops);
            if (BusStops.Any(x => !x.IsValidated))
                return BadRequest(BusStops.Where(x => !x.IsValidated));
            return true;
        }
        
        [Route(BusStopRoute.Import), HttpPost]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            List<BusStop> BusStops = new List<BusStop>();
            using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return Ok(BusStops);
                int StartColumn = 1;
                int StartRow = 1;
                int IdColumn = 0 + StartColumn;
                int NameColumn = 1 + StartColumn;
                int LatitudeColumn = 2 + StartColumn;
                int LongtitudeColumn = 3 + StartColumn;

                for (int i = StartRow; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(worksheet.Cells[i, StartColumn].Value?.ToString()))
                        break;
                    string IdValue = worksheet.Cells[i, IdColumn].Value?.ToString();
                    string NameValue = worksheet.Cells[i, NameColumn].Value?.ToString();
                    string LatitudeValue = worksheet.Cells[i, LatitudeColumn].Value?.ToString();
                    string LongtitudeValue = worksheet.Cells[i, LongtitudeColumn].Value?.ToString();
                    
                    BusStop BusStop = new BusStop();
                    BusStop.Name = NameValue;
                    BusStop.Latitude = decimal.TryParse(LatitudeValue, out decimal Latitude) ? Latitude : 0;
                    BusStop.Longtitude = decimal.TryParse(LongtitudeValue, out decimal Longtitude) ? Longtitude : 0;
                    
                    BusStops.Add(BusStop);
                }
            }
            BusStops = await BusStopService.BulkMerge(BusStops);
            if (BusStops.All(x => x.IsValidated))
                return Ok(true);
            else
            {
                List<string> Errors = new List<string>();
                for (int i = 0; i < BusStops.Count; i++)
                {
                    BusStop BusStop = BusStops[i];
                    if (!BusStop.IsValidated)
                    {
                        string Error = $"Dòng {i + 2} có lỗi:";
                        if (BusStop.Errors.ContainsKey(nameof(BusStop.Id).Camelize()))
                            Error += BusStop.Errors[nameof(BusStop.Id)];
                        if (BusStop.Errors.ContainsKey(nameof(BusStop.Name).Camelize()))
                            Error += BusStop.Errors[nameof(BusStop.Name)];
                        if (BusStop.Errors.ContainsKey(nameof(BusStop.Latitude).Camelize()))
                            Error += BusStop.Errors[nameof(BusStop.Latitude)];
                        if (BusStop.Errors.ContainsKey(nameof(BusStop.Longtitude).Camelize()))
                            Error += BusStop.Errors[nameof(BusStop.Longtitude)];
                        Errors.Add(Error);
                    }
                }
                return BadRequest(Errors);
            }
        }
        
        [Route(BusStopRoute.Export), HttpPost]
        public async Task<ActionResult> Export([FromBody] BusStop_BusStopFilterDTO BusStop_BusStopFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            var BusStopFilter = ConvertFilterDTOToFilterEntity(BusStop_BusStopFilterDTO);
            BusStopFilter.Skip = 0;
            BusStopFilter.Take = int.MaxValue;
            BusStopFilter = await BusStopService.ToFilter(BusStopFilter);
            List<BusStop> BusStops = await BusStopService.List(BusStopFilter);
            List<BusStop_BusStopExportDTO> BusStop_BusStopExportDTOs = BusStops.Select(x => new BusStop_BusStopExportDTO(x)).ToList();    

            string path = "Templates/BusStop_Export.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            Data.Data = BusStop_BusStopExportDTOs;

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
            return File(output.ToArray(), "application/octet-stream", "BusStop.xlsx");
        }

        [Route(BusStopRoute.ExportTemplate), HttpPost]
        public async Task<ActionResult> ExportTemplate([FromBody] BusStop_BusStopFilterDTO BusStop_BusStopFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            string path = "Templates/BusStop_Template.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {
                document.Process(Data);
            };
            return File(output.ToArray(), "application/octet-stream", "BusStop.xlsx");
        }

        private async Task<bool> HasPermission(long Id)
        {
            BusStopFilter BusStopFilter = new BusStopFilter();
            BusStopFilter = await BusStopService.ToFilter(BusStopFilter);
            if (Id == 0)
            {

            }
            else
            {
                BusStopFilter.Id = new IdFilter { Equal = Id };
                int count = await BusStopService.Count(BusStopFilter);
                if (count == 0)
                    return false;
            }
            return true;
        }

        private BusStop ConvertDTOToEntity(BusStop_BusStopDTO BusStop_BusStopDTO)
        {
            BusStop_BusStopDTO.TrimString();
            BusStop BusStop = new BusStop();
            BusStop.Id = BusStop_BusStopDTO.Id;
            BusStop.Name = BusStop_BusStopDTO.Name;
            BusStop.Latitude = BusStop_BusStopDTO.Latitude;
            BusStop.Longtitude = BusStop_BusStopDTO.Longtitude;
            BusStop.BaseLanguage = CurrentContext.Language;
            return BusStop;
        }

        private BusStopFilter ConvertFilterDTOToFilterEntity(BusStop_BusStopFilterDTO BusStop_BusStopFilterDTO)
        {
            BusStopFilter BusStopFilter = new BusStopFilter();
            BusStopFilter.Selects = BusStopSelect.ALL;
            BusStopFilter.Skip = BusStop_BusStopFilterDTO.Skip;
            BusStopFilter.Take = BusStop_BusStopFilterDTO.Take;
            BusStopFilter.OrderBy = BusStop_BusStopFilterDTO.OrderBy;
            BusStopFilter.OrderType = BusStop_BusStopFilterDTO.OrderType;

            BusStopFilter.Id = BusStop_BusStopFilterDTO.Id;
            BusStopFilter.Name = BusStop_BusStopFilterDTO.Name;
            BusStopFilter.Latitude = BusStop_BusStopFilterDTO.Latitude;
            BusStopFilter.Longtitude = BusStop_BusStopFilterDTO.Longtitude;
            BusStopFilter.CreatedAt = BusStop_BusStopFilterDTO.CreatedAt;
            BusStopFilter.UpdatedAt = BusStop_BusStopFilterDTO.UpdatedAt;
            return BusStopFilter;
        }
    }
}

