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
using RideSharing.Services.MDeliveryRoute;
using RideSharing.Services.MCityFreighter;

namespace RideSharing.Rpc.delivery_route
{
    public partial class DeliveryRouteController : RpcController
    {
        private ICityFreighterService CityFreighterService;
        private IDeliveryRouteService DeliveryRouteService;
        private ICurrentContext CurrentContext;
        public DeliveryRouteController(
            ICityFreighterService CityFreighterService,
            IDeliveryRouteService DeliveryRouteService,
            ICurrentContext CurrentContext
        )
        {
            this.CityFreighterService = CityFreighterService;
            this.DeliveryRouteService = DeliveryRouteService;
            this.CurrentContext = CurrentContext;
        }

        [Route(DeliveryRouteRoute.Count), HttpPost]
        public async Task<ActionResult<int>> Count([FromBody] DeliveryRoute_DeliveryRouteFilterDTO DeliveryRoute_DeliveryRouteFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryRouteFilter DeliveryRouteFilter = ConvertFilterDTOToFilterEntity(DeliveryRoute_DeliveryRouteFilterDTO);
            DeliveryRouteFilter = await DeliveryRouteService.ToFilter(DeliveryRouteFilter);
            int count = await DeliveryRouteService.Count(DeliveryRouteFilter);
            return count;
        }

        [Route(DeliveryRouteRoute.List), HttpPost]
        public async Task<ActionResult<List<DeliveryRoute_DeliveryRouteDTO>>> List([FromBody] DeliveryRoute_DeliveryRouteFilterDTO DeliveryRoute_DeliveryRouteFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryRouteFilter DeliveryRouteFilter = ConvertFilterDTOToFilterEntity(DeliveryRoute_DeliveryRouteFilterDTO);
            DeliveryRouteFilter = await DeliveryRouteService.ToFilter(DeliveryRouteFilter);
            List<DeliveryRoute> DeliveryRoutes = await DeliveryRouteService.List(DeliveryRouteFilter);
            List<DeliveryRoute_DeliveryRouteDTO> DeliveryRoute_DeliveryRouteDTOs = DeliveryRoutes
                .Select(c => new DeliveryRoute_DeliveryRouteDTO(c)).ToList();
            return DeliveryRoute_DeliveryRouteDTOs;
        }

        [Route(DeliveryRouteRoute.Get), HttpPost]
        public async Task<ActionResult<DeliveryRoute_DeliveryRouteDTO>> Get([FromBody]DeliveryRoute_DeliveryRouteDTO DeliveryRoute_DeliveryRouteDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(DeliveryRoute_DeliveryRouteDTO.Id))
                return Forbid();

            DeliveryRoute DeliveryRoute = await DeliveryRouteService.Get(DeliveryRoute_DeliveryRouteDTO.Id);
            return new DeliveryRoute_DeliveryRouteDTO(DeliveryRoute);
        }

        [Route(DeliveryRouteRoute.Create), HttpPost]
        public async Task<ActionResult<DeliveryRoute_DeliveryRouteDTO>> Create([FromBody] DeliveryRoute_DeliveryRouteDTO DeliveryRoute_DeliveryRouteDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(DeliveryRoute_DeliveryRouteDTO.Id))
                return Forbid();

            DeliveryRoute DeliveryRoute = ConvertDTOToEntity(DeliveryRoute_DeliveryRouteDTO);
            DeliveryRoute = await DeliveryRouteService.Create(DeliveryRoute);
            DeliveryRoute_DeliveryRouteDTO = new DeliveryRoute_DeliveryRouteDTO(DeliveryRoute);
            if (DeliveryRoute.IsValidated)
                return DeliveryRoute_DeliveryRouteDTO;
            else
                return BadRequest(DeliveryRoute_DeliveryRouteDTO);
        }

        [Route(DeliveryRouteRoute.Update), HttpPost]
        public async Task<ActionResult<DeliveryRoute_DeliveryRouteDTO>> Update([FromBody] DeliveryRoute_DeliveryRouteDTO DeliveryRoute_DeliveryRouteDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(DeliveryRoute_DeliveryRouteDTO.Id))
                return Forbid();

            DeliveryRoute DeliveryRoute = ConvertDTOToEntity(DeliveryRoute_DeliveryRouteDTO);
            DeliveryRoute = await DeliveryRouteService.Update(DeliveryRoute);
            DeliveryRoute_DeliveryRouteDTO = new DeliveryRoute_DeliveryRouteDTO(DeliveryRoute);
            if (DeliveryRoute.IsValidated)
                return DeliveryRoute_DeliveryRouteDTO;
            else
                return BadRequest(DeliveryRoute_DeliveryRouteDTO);
        }

        [Route(DeliveryRouteRoute.Delete), HttpPost]
        public async Task<ActionResult<DeliveryRoute_DeliveryRouteDTO>> Delete([FromBody] DeliveryRoute_DeliveryRouteDTO DeliveryRoute_DeliveryRouteDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(DeliveryRoute_DeliveryRouteDTO.Id))
                return Forbid();

            DeliveryRoute DeliveryRoute = ConvertDTOToEntity(DeliveryRoute_DeliveryRouteDTO);
            DeliveryRoute = await DeliveryRouteService.Delete(DeliveryRoute);
            DeliveryRoute_DeliveryRouteDTO = new DeliveryRoute_DeliveryRouteDTO(DeliveryRoute);
            if (DeliveryRoute.IsValidated)
                return DeliveryRoute_DeliveryRouteDTO;
            else
                return BadRequest(DeliveryRoute_DeliveryRouteDTO);
        }
        
        [Route(DeliveryRouteRoute.BulkDelete), HttpPost]
        public async Task<ActionResult<bool>> BulkDelete([FromBody] List<long> Ids)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            DeliveryRouteFilter DeliveryRouteFilter = new DeliveryRouteFilter();
            DeliveryRouteFilter = await DeliveryRouteService.ToFilter(DeliveryRouteFilter);
            DeliveryRouteFilter.Id = new IdFilter { In = Ids };
            DeliveryRouteFilter.Selects = DeliveryRouteSelect.Id;
            DeliveryRouteFilter.Skip = 0;
            DeliveryRouteFilter.Take = int.MaxValue;

            List<DeliveryRoute> DeliveryRoutes = await DeliveryRouteService.List(DeliveryRouteFilter);
            DeliveryRoutes = await DeliveryRouteService.BulkDelete(DeliveryRoutes);
            if (DeliveryRoutes.Any(x => !x.IsValidated))
                return BadRequest(DeliveryRoutes.Where(x => !x.IsValidated));
            return true;
        }
        
        [Route(DeliveryRouteRoute.Import), HttpPost]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            CityFreighterFilter CityFreighterFilter = new CityFreighterFilter
            {
                Skip = 0,
                Take = int.MaxValue,
                Selects = CityFreighterSelect.ALL
            };
            List<CityFreighter> CityFreighters = await CityFreighterService.List(CityFreighterFilter);
            List<DeliveryRoute> DeliveryRoutes = new List<DeliveryRoute>();
            using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return Ok(DeliveryRoutes);
                int StartColumn = 1;
                int StartRow = 1;
                int IdColumn = 0 + StartColumn;
                int PathColumn = 1 + StartColumn;
                int CityFreighterIdColumn = 2 + StartColumn;
                int TotalTravelDistanceColumn = 3 + StartColumn;
                int TotalEmptyRunDistanceColumn = 4 + StartColumn;

                for (int i = StartRow; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(worksheet.Cells[i, StartColumn].Value?.ToString()))
                        break;
                    string IdValue = worksheet.Cells[i, IdColumn].Value?.ToString();
                    string PathValue = worksheet.Cells[i, PathColumn].Value?.ToString();
                    string CityFreighterIdValue = worksheet.Cells[i, CityFreighterIdColumn].Value?.ToString();
                    string TotalTravelDistanceValue = worksheet.Cells[i, TotalTravelDistanceColumn].Value?.ToString();
                    string TotalEmptyRunDistanceValue = worksheet.Cells[i, TotalEmptyRunDistanceColumn].Value?.ToString();
                    
                    DeliveryRoute DeliveryRoute = new DeliveryRoute();
                    DeliveryRoute.Path = PathValue;
                    DeliveryRoute.TotalTravelDistance = decimal.TryParse(TotalTravelDistanceValue, out decimal TotalTravelDistance) ? TotalTravelDistance : 0;
                    DeliveryRoute.TotalEmptyRunDistance = decimal.TryParse(TotalEmptyRunDistanceValue, out decimal TotalEmptyRunDistance) ? TotalEmptyRunDistance : 0;
                    CityFreighter CityFreighter = CityFreighters.Where(x => x.Id.ToString() == CityFreighterIdValue).FirstOrDefault();
                    DeliveryRoute.CityFreighterId = CityFreighter == null ? 0 : CityFreighter.Id;
                    DeliveryRoute.CityFreighter = CityFreighter;
                    
                    DeliveryRoutes.Add(DeliveryRoute);
                }
            }
            DeliveryRoutes = await DeliveryRouteService.BulkMerge(DeliveryRoutes);
            if (DeliveryRoutes.All(x => x.IsValidated))
                return Ok(true);
            else
            {
                List<string> Errors = new List<string>();
                for (int i = 0; i < DeliveryRoutes.Count; i++)
                {
                    DeliveryRoute DeliveryRoute = DeliveryRoutes[i];
                    if (!DeliveryRoute.IsValidated)
                    {
                        string Error = $"Dòng {i + 2} có lỗi:";
                        if (DeliveryRoute.Errors.ContainsKey(nameof(DeliveryRoute.Id).Camelize()))
                            Error += DeliveryRoute.Errors[nameof(DeliveryRoute.Id)];
                        if (DeliveryRoute.Errors.ContainsKey(nameof(DeliveryRoute.Path).Camelize()))
                            Error += DeliveryRoute.Errors[nameof(DeliveryRoute.Path)];
                        if (DeliveryRoute.Errors.ContainsKey(nameof(DeliveryRoute.CityFreighterId).Camelize()))
                            Error += DeliveryRoute.Errors[nameof(DeliveryRoute.CityFreighterId)];
                        if (DeliveryRoute.Errors.ContainsKey(nameof(DeliveryRoute.TotalTravelDistance).Camelize()))
                            Error += DeliveryRoute.Errors[nameof(DeliveryRoute.TotalTravelDistance)];
                        if (DeliveryRoute.Errors.ContainsKey(nameof(DeliveryRoute.TotalEmptyRunDistance).Camelize()))
                            Error += DeliveryRoute.Errors[nameof(DeliveryRoute.TotalEmptyRunDistance)];
                        Errors.Add(Error);
                    }
                }
                return BadRequest(Errors);
            }
        }
        
        [Route(DeliveryRouteRoute.Export), HttpPost]
        public async Task<ActionResult> Export([FromBody] DeliveryRoute_DeliveryRouteFilterDTO DeliveryRoute_DeliveryRouteFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            var DeliveryRouteFilter = ConvertFilterDTOToFilterEntity(DeliveryRoute_DeliveryRouteFilterDTO);
            DeliveryRouteFilter.Skip = 0;
            DeliveryRouteFilter.Take = int.MaxValue;
            DeliveryRouteFilter = await DeliveryRouteService.ToFilter(DeliveryRouteFilter);
            List<DeliveryRoute> DeliveryRoutes = await DeliveryRouteService.List(DeliveryRouteFilter);
            List<DeliveryRoute_DeliveryRouteExportDTO> DeliveryRoute_DeliveryRouteExportDTOs = DeliveryRoutes.Select(x => new DeliveryRoute_DeliveryRouteExportDTO(x)).ToList();    

            string path = "Templates/DeliveryRoute_Export.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            Data.Data = DeliveryRoute_DeliveryRouteExportDTOs;

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
            return File(output.ToArray(), "application/octet-stream", "DeliveryRoute.xlsx");
        }

        [Route(DeliveryRouteRoute.ExportTemplate), HttpPost]
        public async Task<ActionResult> ExportTemplate([FromBody] DeliveryRoute_DeliveryRouteFilterDTO DeliveryRoute_DeliveryRouteFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            string path = "Templates/DeliveryRoute_Template.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {
                document.Process(Data);
            };
            return File(output.ToArray(), "application/octet-stream", "DeliveryRoute.xlsx");
        }

        private async Task<bool> HasPermission(long Id)
        {
            DeliveryRouteFilter DeliveryRouteFilter = new DeliveryRouteFilter();
            DeliveryRouteFilter = await DeliveryRouteService.ToFilter(DeliveryRouteFilter);
            if (Id == 0)
            {

            }
            else
            {
                DeliveryRouteFilter.Id = new IdFilter { Equal = Id };
                int count = await DeliveryRouteService.Count(DeliveryRouteFilter);
                if (count == 0)
                    return false;
            }
            return true;
        }

        private DeliveryRoute ConvertDTOToEntity(DeliveryRoute_DeliveryRouteDTO DeliveryRoute_DeliveryRouteDTO)
        {
            DeliveryRoute_DeliveryRouteDTO.TrimString();
            DeliveryRoute DeliveryRoute = new DeliveryRoute();
            DeliveryRoute.Id = DeliveryRoute_DeliveryRouteDTO.Id;
            DeliveryRoute.Path = DeliveryRoute_DeliveryRouteDTO.Path;
            DeliveryRoute.CityFreighterId = DeliveryRoute_DeliveryRouteDTO.CityFreighterId;
            DeliveryRoute.TotalTravelDistance = DeliveryRoute_DeliveryRouteDTO.TotalTravelDistance;
            DeliveryRoute.TotalEmptyRunDistance = DeliveryRoute_DeliveryRouteDTO.TotalEmptyRunDistance;
            DeliveryRoute.CityFreighter = DeliveryRoute_DeliveryRouteDTO.CityFreighter == null ? null : new CityFreighter
            {
                Id = DeliveryRoute_DeliveryRouteDTO.CityFreighter.Id,
                Name = DeliveryRoute_DeliveryRouteDTO.CityFreighter.Name,
                Capacity = DeliveryRoute_DeliveryRouteDTO.CityFreighter.Capacity,
                Latitude = DeliveryRoute_DeliveryRouteDTO.CityFreighter.Latitude,
                Longtitude = DeliveryRoute_DeliveryRouteDTO.CityFreighter.Longtitude,
            };
            DeliveryRoute.BaseLanguage = CurrentContext.Language;
            return DeliveryRoute;
        }

        private DeliveryRouteFilter ConvertFilterDTOToFilterEntity(DeliveryRoute_DeliveryRouteFilterDTO DeliveryRoute_DeliveryRouteFilterDTO)
        {
            DeliveryRouteFilter DeliveryRouteFilter = new DeliveryRouteFilter();
            DeliveryRouteFilter.Selects = DeliveryRouteSelect.ALL;
            DeliveryRouteFilter.Skip = DeliveryRoute_DeliveryRouteFilterDTO.Skip;
            DeliveryRouteFilter.Take = DeliveryRoute_DeliveryRouteFilterDTO.Take;
            DeliveryRouteFilter.OrderBy = DeliveryRoute_DeliveryRouteFilterDTO.OrderBy;
            DeliveryRouteFilter.OrderType = DeliveryRoute_DeliveryRouteFilterDTO.OrderType;

            DeliveryRouteFilter.Id = DeliveryRoute_DeliveryRouteFilterDTO.Id;
            DeliveryRouteFilter.Path = DeliveryRoute_DeliveryRouteFilterDTO.Path;
            DeliveryRouteFilter.CityFreighterId = DeliveryRoute_DeliveryRouteFilterDTO.CityFreighterId;
            DeliveryRouteFilter.TotalTravelDistance = DeliveryRoute_DeliveryRouteFilterDTO.TotalTravelDistance;
            DeliveryRouteFilter.TotalEmptyRunDistance = DeliveryRoute_DeliveryRouteFilterDTO.TotalEmptyRunDistance;
            DeliveryRouteFilter.CreatedAt = DeliveryRoute_DeliveryRouteFilterDTO.CreatedAt;
            DeliveryRouteFilter.UpdatedAt = DeliveryRoute_DeliveryRouteFilterDTO.UpdatedAt;
            return DeliveryRouteFilter;
        }
    }
}

