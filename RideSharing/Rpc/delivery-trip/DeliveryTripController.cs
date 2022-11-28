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
using RideSharing.Services.MDeliveryTrip;
using RideSharing.Services.MBusStop;
using RideSharing.Services.MCityFreighter;

namespace RideSharing.Rpc.delivery_trip
{
    public partial class DeliveryTripController : RpcController
    {
        private IBusStopService BusStopService;
        private ICityFreighterService CityFreighterService;
        private IDeliveryTripService DeliveryTripService;
        private ICurrentContext CurrentContext;
        public DeliveryTripController(
            IBusStopService BusStopService,
            ICityFreighterService CityFreighterService,
            IDeliveryTripService DeliveryTripService,
            ICurrentContext CurrentContext
        )
        {
            this.BusStopService = BusStopService;
            this.CityFreighterService = CityFreighterService;
            this.DeliveryTripService = DeliveryTripService;
            this.CurrentContext = CurrentContext;
        }

        [Route(DeliveryTripRoute.Count), HttpPost]
        public async Task<ActionResult<int>> Count([FromBody] DeliveryTrip_DeliveryTripFilterDTO DeliveryTrip_DeliveryTripFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryTripFilter DeliveryTripFilter = ConvertFilterDTOToFilterEntity(DeliveryTrip_DeliveryTripFilterDTO);
            DeliveryTripFilter = await DeliveryTripService.ToFilter(DeliveryTripFilter);
            int count = await DeliveryTripService.Count(DeliveryTripFilter);
            return count;
        }

        [Route(DeliveryTripRoute.List), HttpPost]
        public async Task<ActionResult<List<DeliveryTrip_DeliveryTripDTO>>> List([FromBody] DeliveryTrip_DeliveryTripFilterDTO DeliveryTrip_DeliveryTripFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryTripFilter DeliveryTripFilter = ConvertFilterDTOToFilterEntity(DeliveryTrip_DeliveryTripFilterDTO);
            DeliveryTripFilter = await DeliveryTripService.ToFilter(DeliveryTripFilter);
            List<DeliveryTrip> DeliveryTrips = await DeliveryTripService.List(DeliveryTripFilter);
            List<DeliveryTrip_DeliveryTripDTO> DeliveryTrip_DeliveryTripDTOs = DeliveryTrips
                .Select(c => new DeliveryTrip_DeliveryTripDTO(c)).ToList();
            return DeliveryTrip_DeliveryTripDTOs;
        }

        [Route(DeliveryTripRoute.Get), HttpPost]
        public async Task<ActionResult<DeliveryTrip_DeliveryTripDTO>> Get([FromBody]DeliveryTrip_DeliveryTripDTO DeliveryTrip_DeliveryTripDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(DeliveryTrip_DeliveryTripDTO.Id))
                return Forbid();

            DeliveryTrip DeliveryTrip = await DeliveryTripService.Get(DeliveryTrip_DeliveryTripDTO.Id);
            return new DeliveryTrip_DeliveryTripDTO(DeliveryTrip);
        }

        [Route(DeliveryTripRoute.Create), HttpPost]
        public async Task<ActionResult<DeliveryTrip_DeliveryTripDTO>> Create([FromBody] DeliveryTrip_DeliveryTripDTO DeliveryTrip_DeliveryTripDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(DeliveryTrip_DeliveryTripDTO.Id))
                return Forbid();

            DeliveryTrip DeliveryTrip = ConvertDTOToEntity(DeliveryTrip_DeliveryTripDTO);
            DeliveryTrip = await DeliveryTripService.Create(DeliveryTrip);
            DeliveryTrip_DeliveryTripDTO = new DeliveryTrip_DeliveryTripDTO(DeliveryTrip);
            if (DeliveryTrip.IsValidated)
                return DeliveryTrip_DeliveryTripDTO;
            else
                return BadRequest(DeliveryTrip_DeliveryTripDTO);
        }

        [Route(DeliveryTripRoute.Update), HttpPost]
        public async Task<ActionResult<DeliveryTrip_DeliveryTripDTO>> Update([FromBody] DeliveryTrip_DeliveryTripDTO DeliveryTrip_DeliveryTripDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(DeliveryTrip_DeliveryTripDTO.Id))
                return Forbid();

            DeliveryTrip DeliveryTrip = ConvertDTOToEntity(DeliveryTrip_DeliveryTripDTO);
            DeliveryTrip = await DeliveryTripService.Update(DeliveryTrip);
            DeliveryTrip_DeliveryTripDTO = new DeliveryTrip_DeliveryTripDTO(DeliveryTrip);
            if (DeliveryTrip.IsValidated)
                return DeliveryTrip_DeliveryTripDTO;
            else
                return BadRequest(DeliveryTrip_DeliveryTripDTO);
        }

        [Route(DeliveryTripRoute.Delete), HttpPost]
        public async Task<ActionResult<DeliveryTrip_DeliveryTripDTO>> Delete([FromBody] DeliveryTrip_DeliveryTripDTO DeliveryTrip_DeliveryTripDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(DeliveryTrip_DeliveryTripDTO.Id))
                return Forbid();

            DeliveryTrip DeliveryTrip = ConvertDTOToEntity(DeliveryTrip_DeliveryTripDTO);
            DeliveryTrip = await DeliveryTripService.Delete(DeliveryTrip);
            DeliveryTrip_DeliveryTripDTO = new DeliveryTrip_DeliveryTripDTO(DeliveryTrip);
            if (DeliveryTrip.IsValidated)
                return DeliveryTrip_DeliveryTripDTO;
            else
                return BadRequest(DeliveryTrip_DeliveryTripDTO);
        }
        
        [Route(DeliveryTripRoute.BulkDelete), HttpPost]
        public async Task<ActionResult<bool>> BulkDelete([FromBody] List<long> Ids)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryTripFilter DeliveryTripFilter = new DeliveryTripFilter();
            DeliveryTripFilter = await DeliveryTripService.ToFilter(DeliveryTripFilter);
            DeliveryTripFilter.Id = new IdFilter { In = Ids };
            DeliveryTripFilter.Selects = DeliveryTripSelect.Id;
            DeliveryTripFilter.Skip = 0;
            DeliveryTripFilter.Take = int.MaxValue;

            List<DeliveryTrip> DeliveryTrips = await DeliveryTripService.List(DeliveryTripFilter);
            DeliveryTrips = await DeliveryTripService.BulkDelete(DeliveryTrips);
            if (DeliveryTrips.Any(x => !x.IsValidated))
                return BadRequest(DeliveryTrips.Where(x => !x.IsValidated));
            return true;
        }
        
        [Route(DeliveryTripRoute.Import), HttpPost]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            BusStopFilter BusStopFilter = new BusStopFilter
            {
                Skip = 0,
                Take = int.MaxValue,
                Selects = BusStopSelect.ALL
            };
            List<BusStop> BusStops = await BusStopService.List(BusStopFilter);
            CityFreighterFilter CityFreighterFilter = new CityFreighterFilter
            {
                Skip = 0,
                Take = int.MaxValue,
                Selects = CityFreighterSelect.ALL
            };
            List<CityFreighter> CityFreighters = await CityFreighterService.List(CityFreighterFilter);
            List<DeliveryTrip> DeliveryTrips = new List<DeliveryTrip>();
            using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return Ok(DeliveryTrips);
                int StartColumn = 1;
                int StartRow = 1;
                int IdColumn = 0 + StartColumn;
                int PathColumn = 1 + StartColumn;
                int CityFreighterIdColumn = 2 + StartColumn;
                int BusStopIdColumn = 3 + StartColumn;
                int TravelDistanceColumn = 4 + StartColumn;

                for (int i = StartRow; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(worksheet.Cells[i, StartColumn].Value?.ToString()))
                        break;
                    string IdValue = worksheet.Cells[i, IdColumn].Value?.ToString();
                    string PathValue = worksheet.Cells[i, PathColumn].Value?.ToString();
                    string CityFreighterIdValue = worksheet.Cells[i, CityFreighterIdColumn].Value?.ToString();
                    string BusStopIdValue = worksheet.Cells[i, BusStopIdColumn].Value?.ToString();
                    string TravelDistanceValue = worksheet.Cells[i, TravelDistanceColumn].Value?.ToString();
                    
                    DeliveryTrip DeliveryTrip = new DeliveryTrip();
                    DeliveryTrip.Path = PathValue;
                    DeliveryTrip.TravelDistance = decimal.TryParse(TravelDistanceValue, out decimal TravelDistance) ? TravelDistance : 0;
                    BusStop BusStop = BusStops.Where(x => x.Id.ToString() == BusStopIdValue).FirstOrDefault();
                    DeliveryTrip.BusStopId = BusStop == null ? 0 : BusStop.Id;
                    DeliveryTrip.BusStop = BusStop;
                    CityFreighter CityFreighter = CityFreighters.Where(x => x.Id.ToString() == CityFreighterIdValue).FirstOrDefault();
                    DeliveryTrip.CityFreighterId = CityFreighter == null ? 0 : CityFreighter.Id;
                    DeliveryTrip.CityFreighter = CityFreighter;
                    
                    DeliveryTrips.Add(DeliveryTrip);
                }
            }
            DeliveryTrips = await DeliveryTripService.BulkMerge(DeliveryTrips);
            if (DeliveryTrips.All(x => x.IsValidated))
                return Ok(true);
            else
            {
                List<string> Errors = new List<string>();
                for (int i = 0; i < DeliveryTrips.Count; i++)
                {
                    DeliveryTrip DeliveryTrip = DeliveryTrips[i];
                    if (!DeliveryTrip.IsValidated)
                    {
                        string Error = $"Dòng {i + 2} có lỗi:";
                        if (DeliveryTrip.Errors.ContainsKey(nameof(DeliveryTrip.Id).Camelize()))
                            Error += DeliveryTrip.Errors[nameof(DeliveryTrip.Id)];
                        if (DeliveryTrip.Errors.ContainsKey(nameof(DeliveryTrip.Path).Camelize()))
                            Error += DeliveryTrip.Errors[nameof(DeliveryTrip.Path)];
                        if (DeliveryTrip.Errors.ContainsKey(nameof(DeliveryTrip.CityFreighterId).Camelize()))
                            Error += DeliveryTrip.Errors[nameof(DeliveryTrip.CityFreighterId)];
                        if (DeliveryTrip.Errors.ContainsKey(nameof(DeliveryTrip.BusStopId).Camelize()))
                            Error += DeliveryTrip.Errors[nameof(DeliveryTrip.BusStopId)];
                        if (DeliveryTrip.Errors.ContainsKey(nameof(DeliveryTrip.TravelDistance).Camelize()))
                            Error += DeliveryTrip.Errors[nameof(DeliveryTrip.TravelDistance)];
                        Errors.Add(Error);
                    }
                }
                return BadRequest(Errors);
            }
        }
        
        [Route(DeliveryTripRoute.Export), HttpPost]
        public async Task<ActionResult> Export([FromBody] DeliveryTrip_DeliveryTripFilterDTO DeliveryTrip_DeliveryTripFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            var DeliveryTripFilter = ConvertFilterDTOToFilterEntity(DeliveryTrip_DeliveryTripFilterDTO);
            DeliveryTripFilter.Skip = 0;
            DeliveryTripFilter.Take = int.MaxValue;
            DeliveryTripFilter = await DeliveryTripService.ToFilter(DeliveryTripFilter);
            List<DeliveryTrip> DeliveryTrips = await DeliveryTripService.List(DeliveryTripFilter);
            List<DeliveryTrip_DeliveryTripExportDTO> DeliveryTrip_DeliveryTripExportDTOs = DeliveryTrips.Select(x => new DeliveryTrip_DeliveryTripExportDTO(x)).ToList();    

            string path = "Templates/DeliveryTrip_Export.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            Data.Data = DeliveryTrip_DeliveryTripExportDTOs;

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
            return File(output.ToArray(), "application/octet-stream", "DeliveryTrip.xlsx");
        }

        [Route(DeliveryTripRoute.ExportTemplate), HttpPost]
        public async Task<ActionResult> ExportTemplate([FromBody] DeliveryTrip_DeliveryTripFilterDTO DeliveryTrip_DeliveryTripFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            string path = "Templates/DeliveryTrip_Template.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {
                document.Process(Data);
            };
            return File(output.ToArray(), "application/octet-stream", "DeliveryTrip.xlsx");
        }

        private async Task<bool> HasPermission(long Id)
        {
            DeliveryTripFilter DeliveryTripFilter = new DeliveryTripFilter();
            DeliveryTripFilter = await DeliveryTripService.ToFilter(DeliveryTripFilter);
            if (Id == 0)
            {

            }
            else
            {
                DeliveryTripFilter.Id = new IdFilter { Equal = Id };
                int count = await DeliveryTripService.Count(DeliveryTripFilter);
                if (count == 0)
                    return false;
            }
            return true;
        }

        private DeliveryTrip ConvertDTOToEntity(DeliveryTrip_DeliveryTripDTO DeliveryTrip_DeliveryTripDTO)
        {
            DeliveryTrip_DeliveryTripDTO.TrimString();
            DeliveryTrip DeliveryTrip = new DeliveryTrip();
            DeliveryTrip.Id = DeliveryTrip_DeliveryTripDTO.Id;
            DeliveryTrip.Path = DeliveryTrip_DeliveryTripDTO.Path;
            DeliveryTrip.CityFreighterId = DeliveryTrip_DeliveryTripDTO.CityFreighterId;
            DeliveryTrip.BusStopId = DeliveryTrip_DeliveryTripDTO.BusStopId;
            DeliveryTrip.TravelDistance = DeliveryTrip_DeliveryTripDTO.TravelDistance;
            DeliveryTrip.BusStop = DeliveryTrip_DeliveryTripDTO.BusStop == null ? null : new BusStop
            {
                Id = DeliveryTrip_DeliveryTripDTO.BusStop.Id,
                Name = DeliveryTrip_DeliveryTripDTO.BusStop.Name,
                Latitude = DeliveryTrip_DeliveryTripDTO.BusStop.Latitude,
                Longtitude = DeliveryTrip_DeliveryTripDTO.BusStop.Longtitude,
            };
            DeliveryTrip.CityFreighter = DeliveryTrip_DeliveryTripDTO.CityFreighter == null ? null : new CityFreighter
            {
                Id = DeliveryTrip_DeliveryTripDTO.CityFreighter.Id,
                Name = DeliveryTrip_DeliveryTripDTO.CityFreighter.Name,
                Capacity = DeliveryTrip_DeliveryTripDTO.CityFreighter.Capacity,
                Latitude = DeliveryTrip_DeliveryTripDTO.CityFreighter.Latitude,
                Longtitude = DeliveryTrip_DeliveryTripDTO.CityFreighter.Longtitude,
            };
            DeliveryTrip.BaseLanguage = CurrentContext.Language;
            return DeliveryTrip;
        }

        private DeliveryTripFilter ConvertFilterDTOToFilterEntity(DeliveryTrip_DeliveryTripFilterDTO DeliveryTrip_DeliveryTripFilterDTO)
        {
            DeliveryTripFilter DeliveryTripFilter = new DeliveryTripFilter();
            DeliveryTripFilter.Selects = DeliveryTripSelect.ALL;
            DeliveryTripFilter.Skip = DeliveryTrip_DeliveryTripFilterDTO.Skip;
            DeliveryTripFilter.Take = DeliveryTrip_DeliveryTripFilterDTO.Take;
            DeliveryTripFilter.OrderBy = DeliveryTrip_DeliveryTripFilterDTO.OrderBy;
            DeliveryTripFilter.OrderType = DeliveryTrip_DeliveryTripFilterDTO.OrderType;

            DeliveryTripFilter.Id = DeliveryTrip_DeliveryTripFilterDTO.Id;
            DeliveryTripFilter.Path = DeliveryTrip_DeliveryTripFilterDTO.Path;
            DeliveryTripFilter.CityFreighterId = DeliveryTrip_DeliveryTripFilterDTO.CityFreighterId;
            DeliveryTripFilter.BusStopId = DeliveryTrip_DeliveryTripFilterDTO.BusStopId;
            DeliveryTripFilter.TravelDistance = DeliveryTrip_DeliveryTripFilterDTO.TravelDistance;
            DeliveryTripFilter.CreatedAt = DeliveryTrip_DeliveryTripFilterDTO.CreatedAt;
            DeliveryTripFilter.UpdatedAt = DeliveryTrip_DeliveryTripFilterDTO.UpdatedAt;
            return DeliveryTripFilter;
        }
    }
}

