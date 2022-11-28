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
using RideSharing.Services.MCityFreighter;

namespace RideSharing.Rpc.city_freighter
{
    public partial class CityFreighterController : RpcController
    {
        private ICityFreighterService CityFreighterService;
        private ICurrentContext CurrentContext;
        public CityFreighterController(
            ICityFreighterService CityFreighterService,
            ICurrentContext CurrentContext
        )
        {
            this.CityFreighterService = CityFreighterService;
            this.CurrentContext = CurrentContext;
        }

        [Route(CityFreighterRoute.Count), HttpPost]
        public async Task<ActionResult<int>> Count([FromBody] CityFreighter_CityFreighterFilterDTO CityFreighter_CityFreighterFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CityFreighterFilter CityFreighterFilter = ConvertFilterDTOToFilterEntity(CityFreighter_CityFreighterFilterDTO);
            CityFreighterFilter = await CityFreighterService.ToFilter(CityFreighterFilter);
            int count = await CityFreighterService.Count(CityFreighterFilter);
            return count;
        }

        [Route(CityFreighterRoute.List), HttpPost]
        public async Task<ActionResult<List<CityFreighter_CityFreighterDTO>>> List([FromBody] CityFreighter_CityFreighterFilterDTO CityFreighter_CityFreighterFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CityFreighterFilter CityFreighterFilter = ConvertFilterDTOToFilterEntity(CityFreighter_CityFreighterFilterDTO);
            CityFreighterFilter = await CityFreighterService.ToFilter(CityFreighterFilter);
            List<CityFreighter> CityFreighters = await CityFreighterService.List(CityFreighterFilter);
            List<CityFreighter_CityFreighterDTO> CityFreighter_CityFreighterDTOs = CityFreighters
                .Select(c => new CityFreighter_CityFreighterDTO(c)).ToList();
            return CityFreighter_CityFreighterDTOs;
        }

        [Route(CityFreighterRoute.Get), HttpPost]
        public async Task<ActionResult<CityFreighter_CityFreighterDTO>> Get([FromBody]CityFreighter_CityFreighterDTO CityFreighter_CityFreighterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(CityFreighter_CityFreighterDTO.Id))
                return Forbid();

            CityFreighter CityFreighter = await CityFreighterService.Get(CityFreighter_CityFreighterDTO.Id);
            return new CityFreighter_CityFreighterDTO(CityFreighter);
        }

        [Route(CityFreighterRoute.Create), HttpPost]
        public async Task<ActionResult<CityFreighter_CityFreighterDTO>> Create([FromBody] CityFreighter_CityFreighterDTO CityFreighter_CityFreighterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(CityFreighter_CityFreighterDTO.Id))
                return Forbid();

            CityFreighter CityFreighter = ConvertDTOToEntity(CityFreighter_CityFreighterDTO);
            CityFreighter = await CityFreighterService.Create(CityFreighter);
            CityFreighter_CityFreighterDTO = new CityFreighter_CityFreighterDTO(CityFreighter);
            if (CityFreighter.IsValidated)
                return CityFreighter_CityFreighterDTO;
            else
                return BadRequest(CityFreighter_CityFreighterDTO);
        }

        [Route(CityFreighterRoute.Update), HttpPost]
        public async Task<ActionResult<CityFreighter_CityFreighterDTO>> Update([FromBody] CityFreighter_CityFreighterDTO CityFreighter_CityFreighterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(CityFreighter_CityFreighterDTO.Id))
                return Forbid();

            CityFreighter CityFreighter = ConvertDTOToEntity(CityFreighter_CityFreighterDTO);
            CityFreighter = await CityFreighterService.Update(CityFreighter);
            CityFreighter_CityFreighterDTO = new CityFreighter_CityFreighterDTO(CityFreighter);
            if (CityFreighter.IsValidated)
                return CityFreighter_CityFreighterDTO;
            else
                return BadRequest(CityFreighter_CityFreighterDTO);
        }

        [Route(CityFreighterRoute.Delete), HttpPost]
        public async Task<ActionResult<CityFreighter_CityFreighterDTO>> Delete([FromBody] CityFreighter_CityFreighterDTO CityFreighter_CityFreighterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(CityFreighter_CityFreighterDTO.Id))
                return Forbid();

            CityFreighter CityFreighter = ConvertDTOToEntity(CityFreighter_CityFreighterDTO);
            CityFreighter = await CityFreighterService.Delete(CityFreighter);
            CityFreighter_CityFreighterDTO = new CityFreighter_CityFreighterDTO(CityFreighter);
            if (CityFreighter.IsValidated)
                return CityFreighter_CityFreighterDTO;
            else
                return BadRequest(CityFreighter_CityFreighterDTO);
        }
        
        [Route(CityFreighterRoute.BulkDelete), HttpPost]
        public async Task<ActionResult<bool>> BulkDelete([FromBody] List<long> Ids)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            CityFreighterFilter CityFreighterFilter = new CityFreighterFilter();
            CityFreighterFilter = await CityFreighterService.ToFilter(CityFreighterFilter);
            CityFreighterFilter.Id = new IdFilter { In = Ids };
            CityFreighterFilter.Selects = CityFreighterSelect.Id;
            CityFreighterFilter.Skip = 0;
            CityFreighterFilter.Take = int.MaxValue;

            List<CityFreighter> CityFreighters = await CityFreighterService.List(CityFreighterFilter);
            CityFreighters = await CityFreighterService.BulkDelete(CityFreighters);
            if (CityFreighters.Any(x => !x.IsValidated))
                return BadRequest(CityFreighters.Where(x => !x.IsValidated));
            return true;
        }
        
        [Route(CityFreighterRoute.Import), HttpPost]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            List<CityFreighter> CityFreighters = new List<CityFreighter>();
            using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return Ok(CityFreighters);
                int StartColumn = 1;
                int StartRow = 1;
                int IdColumn = 0 + StartColumn;
                int NameColumn = 1 + StartColumn;
                int CapacityColumn = 2 + StartColumn;
                int LatitudeColumn = 3 + StartColumn;
                int LongtitudeColumn = 4 + StartColumn;

                for (int i = StartRow; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(worksheet.Cells[i, StartColumn].Value?.ToString()))
                        break;
                    string IdValue = worksheet.Cells[i, IdColumn].Value?.ToString();
                    string NameValue = worksheet.Cells[i, NameColumn].Value?.ToString();
                    string CapacityValue = worksheet.Cells[i, CapacityColumn].Value?.ToString();
                    string LatitudeValue = worksheet.Cells[i, LatitudeColumn].Value?.ToString();
                    string LongtitudeValue = worksheet.Cells[i, LongtitudeColumn].Value?.ToString();
                    
                    CityFreighter CityFreighter = new CityFreighter();
                    CityFreighter.Name = NameValue;
                    CityFreighter.Capacity = decimal.TryParse(CapacityValue, out decimal Capacity) ? Capacity : 0;
                    CityFreighter.Latitude = decimal.TryParse(LatitudeValue, out decimal Latitude) ? Latitude : 0;
                    CityFreighter.Longtitude = decimal.TryParse(LongtitudeValue, out decimal Longtitude) ? Longtitude : 0;
                    
                    CityFreighters.Add(CityFreighter);
                }
            }
            CityFreighters = await CityFreighterService.BulkMerge(CityFreighters);
            if (CityFreighters.All(x => x.IsValidated))
                return Ok(true);
            else
            {
                List<string> Errors = new List<string>();
                for (int i = 0; i < CityFreighters.Count; i++)
                {
                    CityFreighter CityFreighter = CityFreighters[i];
                    if (!CityFreighter.IsValidated)
                    {
                        string Error = $"Dòng {i + 2} có lỗi:";
                        if (CityFreighter.Errors.ContainsKey(nameof(CityFreighter.Id).Camelize()))
                            Error += CityFreighter.Errors[nameof(CityFreighter.Id)];
                        if (CityFreighter.Errors.ContainsKey(nameof(CityFreighter.Name).Camelize()))
                            Error += CityFreighter.Errors[nameof(CityFreighter.Name)];
                        if (CityFreighter.Errors.ContainsKey(nameof(CityFreighter.Capacity).Camelize()))
                            Error += CityFreighter.Errors[nameof(CityFreighter.Capacity)];
                        if (CityFreighter.Errors.ContainsKey(nameof(CityFreighter.Latitude).Camelize()))
                            Error += CityFreighter.Errors[nameof(CityFreighter.Latitude)];
                        if (CityFreighter.Errors.ContainsKey(nameof(CityFreighter.Longtitude).Camelize()))
                            Error += CityFreighter.Errors[nameof(CityFreighter.Longtitude)];
                        Errors.Add(Error);
                    }
                }
                return BadRequest(Errors);
            }
        }
        
        [Route(CityFreighterRoute.Export), HttpPost]
        public async Task<ActionResult> Export([FromBody] CityFreighter_CityFreighterFilterDTO CityFreighter_CityFreighterFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            var CityFreighterFilter = ConvertFilterDTOToFilterEntity(CityFreighter_CityFreighterFilterDTO);
            CityFreighterFilter.Skip = 0;
            CityFreighterFilter.Take = int.MaxValue;
            CityFreighterFilter = await CityFreighterService.ToFilter(CityFreighterFilter);
            List<CityFreighter> CityFreighters = await CityFreighterService.List(CityFreighterFilter);
            List<CityFreighter_CityFreighterExportDTO> CityFreighter_CityFreighterExportDTOs = CityFreighters.Select(x => new CityFreighter_CityFreighterExportDTO(x)).ToList();    

            string path = "Templates/CityFreighter_Export.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            Data.Data = CityFreighter_CityFreighterExportDTOs;

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
            return File(output.ToArray(), "application/octet-stream", "CityFreighter.xlsx");
        }

        [Route(CityFreighterRoute.ExportTemplate), HttpPost]
        public async Task<ActionResult> ExportTemplate([FromBody] CityFreighter_CityFreighterFilterDTO CityFreighter_CityFreighterFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            string path = "Templates/CityFreighter_Template.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {
                document.Process(Data);
            };
            return File(output.ToArray(), "application/octet-stream", "CityFreighter.xlsx");
        }

        private async Task<bool> HasPermission(long Id)
        {
            CityFreighterFilter CityFreighterFilter = new CityFreighterFilter();
            CityFreighterFilter = await CityFreighterService.ToFilter(CityFreighterFilter);
            if (Id == 0)
            {

            }
            else
            {
                CityFreighterFilter.Id = new IdFilter { Equal = Id };
                int count = await CityFreighterService.Count(CityFreighterFilter);
                if (count == 0)
                    return false;
            }
            return true;
        }

        private CityFreighter ConvertDTOToEntity(CityFreighter_CityFreighterDTO CityFreighter_CityFreighterDTO)
        {
            CityFreighter_CityFreighterDTO.TrimString();
            CityFreighter CityFreighter = new CityFreighter();
            CityFreighter.Id = CityFreighter_CityFreighterDTO.Id;
            CityFreighter.Name = CityFreighter_CityFreighterDTO.Name;
            CityFreighter.Capacity = CityFreighter_CityFreighterDTO.Capacity;
            CityFreighter.Latitude = CityFreighter_CityFreighterDTO.Latitude;
            CityFreighter.Longtitude = CityFreighter_CityFreighterDTO.Longtitude;
            CityFreighter.BaseLanguage = CurrentContext.Language;
            return CityFreighter;
        }

        private CityFreighterFilter ConvertFilterDTOToFilterEntity(CityFreighter_CityFreighterFilterDTO CityFreighter_CityFreighterFilterDTO)
        {
            CityFreighterFilter CityFreighterFilter = new CityFreighterFilter();
            CityFreighterFilter.Selects = CityFreighterSelect.ALL;
            CityFreighterFilter.Skip = CityFreighter_CityFreighterFilterDTO.Skip;
            CityFreighterFilter.Take = CityFreighter_CityFreighterFilterDTO.Take;
            CityFreighterFilter.OrderBy = CityFreighter_CityFreighterFilterDTO.OrderBy;
            CityFreighterFilter.OrderType = CityFreighter_CityFreighterFilterDTO.OrderType;

            CityFreighterFilter.Id = CityFreighter_CityFreighterFilterDTO.Id;
            CityFreighterFilter.Name = CityFreighter_CityFreighterFilterDTO.Name;
            CityFreighterFilter.Capacity = CityFreighter_CityFreighterFilterDTO.Capacity;
            CityFreighterFilter.Latitude = CityFreighter_CityFreighterFilterDTO.Latitude;
            CityFreighterFilter.Longtitude = CityFreighter_CityFreighterFilterDTO.Longtitude;
            CityFreighterFilter.CreatedAt = CityFreighter_CityFreighterFilterDTO.CreatedAt;
            CityFreighterFilter.UpdatedAt = CityFreighter_CityFreighterFilterDTO.UpdatedAt;
            return CityFreighterFilter;
        }
    }
}

