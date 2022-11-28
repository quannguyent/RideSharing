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
using RideSharing.Services.MSystemConfig;

namespace RideSharing.Rpc.system_config
{
    public partial class SystemConfigController : RpcController
    {
        private ISystemConfigService SystemConfigService;
        private ICurrentContext CurrentContext;
        public SystemConfigController(
            ISystemConfigService SystemConfigService,
            ICurrentContext CurrentContext
        )
        {
            this.SystemConfigService = SystemConfigService;
            this.CurrentContext = CurrentContext;
        }

        [Route(SystemConfigRoute.Count), HttpPost]
        public async Task<ActionResult<int>> Count([FromBody] SystemConfig_SystemConfigFilterDTO SystemConfig_SystemConfigFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            SystemConfigFilter SystemConfigFilter = ConvertFilterDTOToFilterEntity(SystemConfig_SystemConfigFilterDTO);
            SystemConfigFilter = await SystemConfigService.ToFilter(SystemConfigFilter);
            int count = await SystemConfigService.Count(SystemConfigFilter);
            return count;
        }

        [Route(SystemConfigRoute.List), HttpPost]
        public async Task<ActionResult<List<SystemConfig_SystemConfigDTO>>> List([FromBody] SystemConfig_SystemConfigFilterDTO SystemConfig_SystemConfigFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            SystemConfigFilter SystemConfigFilter = ConvertFilterDTOToFilterEntity(SystemConfig_SystemConfigFilterDTO);
            SystemConfigFilter = await SystemConfigService.ToFilter(SystemConfigFilter);
            List<SystemConfig> SystemConfigs = await SystemConfigService.List(SystemConfigFilter);
            List<SystemConfig_SystemConfigDTO> SystemConfig_SystemConfigDTOs = SystemConfigs
                .Select(c => new SystemConfig_SystemConfigDTO(c)).ToList();
            return SystemConfig_SystemConfigDTOs;
        }

        [Route(SystemConfigRoute.Get), HttpPost]
        public async Task<ActionResult<SystemConfig_SystemConfigDTO>> Get([FromBody]SystemConfig_SystemConfigDTO SystemConfig_SystemConfigDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(SystemConfig_SystemConfigDTO.Id))
                return Forbid();

            SystemConfig SystemConfig = await SystemConfigService.Get(SystemConfig_SystemConfigDTO.Id);
            return new SystemConfig_SystemConfigDTO(SystemConfig);
        }

        [Route(SystemConfigRoute.Create), HttpPost]
        public async Task<ActionResult<SystemConfig_SystemConfigDTO>> Create([FromBody] SystemConfig_SystemConfigDTO SystemConfig_SystemConfigDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(SystemConfig_SystemConfigDTO.Id))
                return Forbid();

            SystemConfig SystemConfig = ConvertDTOToEntity(SystemConfig_SystemConfigDTO);
            SystemConfig = await SystemConfigService.Create(SystemConfig);
            SystemConfig_SystemConfigDTO = new SystemConfig_SystemConfigDTO(SystemConfig);
            if (SystemConfig.IsValidated)
                return SystemConfig_SystemConfigDTO;
            else
                return BadRequest(SystemConfig_SystemConfigDTO);
        }

        [Route(SystemConfigRoute.Update), HttpPost]
        public async Task<ActionResult<SystemConfig_SystemConfigDTO>> Update([FromBody] SystemConfig_SystemConfigDTO SystemConfig_SystemConfigDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(SystemConfig_SystemConfigDTO.Id))
                return Forbid();

            SystemConfig SystemConfig = ConvertDTOToEntity(SystemConfig_SystemConfigDTO);
            SystemConfig = await SystemConfigService.Update(SystemConfig);
            SystemConfig_SystemConfigDTO = new SystemConfig_SystemConfigDTO(SystemConfig);
            if (SystemConfig.IsValidated)
                return SystemConfig_SystemConfigDTO;
            else
                return BadRequest(SystemConfig_SystemConfigDTO);
        }

        [Route(SystemConfigRoute.Delete), HttpPost]
        public async Task<ActionResult<SystemConfig_SystemConfigDTO>> Delete([FromBody] SystemConfig_SystemConfigDTO SystemConfig_SystemConfigDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(SystemConfig_SystemConfigDTO.Id))
                return Forbid();

            SystemConfig SystemConfig = ConvertDTOToEntity(SystemConfig_SystemConfigDTO);
            SystemConfig = await SystemConfigService.Delete(SystemConfig);
            SystemConfig_SystemConfigDTO = new SystemConfig_SystemConfigDTO(SystemConfig);
            if (SystemConfig.IsValidated)
                return SystemConfig_SystemConfigDTO;
            else
                return BadRequest(SystemConfig_SystemConfigDTO);
        }
        
        [Route(SystemConfigRoute.BulkDelete), HttpPost]
        public async Task<ActionResult<bool>> BulkDelete([FromBody] List<long> Ids)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            SystemConfigFilter SystemConfigFilter = new SystemConfigFilter();
            SystemConfigFilter = await SystemConfigService.ToFilter(SystemConfigFilter);
            SystemConfigFilter.Id = new IdFilter { In = Ids };
            SystemConfigFilter.Selects = SystemConfigSelect.Id;
            SystemConfigFilter.Skip = 0;
            SystemConfigFilter.Take = int.MaxValue;

            List<SystemConfig> SystemConfigs = await SystemConfigService.List(SystemConfigFilter);
            SystemConfigs = await SystemConfigService.BulkDelete(SystemConfigs);
            if (SystemConfigs.Any(x => !x.IsValidated))
                return BadRequest(SystemConfigs.Where(x => !x.IsValidated));
            return true;
        }
        
        [Route(SystemConfigRoute.Import), HttpPost]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            List<SystemConfig> SystemConfigs = new List<SystemConfig>();
            using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return Ok(SystemConfigs);
                int StartColumn = 1;
                int StartRow = 1;
                int IdColumn = 0 + StartColumn;
                int CodeColumn = 1 + StartColumn;
                int FreighterQuotientCostColumn = 2 + StartColumn;
                int DeliveryRadiusColumn = 3 + StartColumn;
                int DeliveryServiceDurationColumn = 4 + StartColumn;

                for (int i = StartRow; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(worksheet.Cells[i, StartColumn].Value?.ToString()))
                        break;
                    string IdValue = worksheet.Cells[i, IdColumn].Value?.ToString();
                    string CodeValue = worksheet.Cells[i, CodeColumn].Value?.ToString();
                    string FreighterQuotientCostValue = worksheet.Cells[i, FreighterQuotientCostColumn].Value?.ToString();
                    string DeliveryRadiusValue = worksheet.Cells[i, DeliveryRadiusColumn].Value?.ToString();
                    string DeliveryServiceDurationValue = worksheet.Cells[i, DeliveryServiceDurationColumn].Value?.ToString();
                    
                    SystemConfig SystemConfig = new SystemConfig();
                    SystemConfig.Code = CodeValue;
                    SystemConfig.FreighterQuotientCost = decimal.TryParse(FreighterQuotientCostValue, out decimal FreighterQuotientCost) ? FreighterQuotientCost : 0;
                    SystemConfig.DeliveryRadius = decimal.TryParse(DeliveryRadiusValue, out decimal DeliveryRadius) ? DeliveryRadius : 0;
                    SystemConfig.DeliveryServiceDuration = long.TryParse(DeliveryServiceDurationValue, out long DeliveryServiceDuration) ? DeliveryServiceDuration : 0;
                    
                    SystemConfigs.Add(SystemConfig);
                }
            }
            SystemConfigs = await SystemConfigService.BulkMerge(SystemConfigs);
            if (SystemConfigs.All(x => x.IsValidated))
                return Ok(true);
            else
            {
                List<string> Errors = new List<string>();
                for (int i = 0; i < SystemConfigs.Count; i++)
                {
                    SystemConfig SystemConfig = SystemConfigs[i];
                    if (!SystemConfig.IsValidated)
                    {
                        string Error = $"Dòng {i + 2} có lỗi:";
                        if (SystemConfig.Errors.ContainsKey(nameof(SystemConfig.Id).Camelize()))
                            Error += SystemConfig.Errors[nameof(SystemConfig.Id)];
                        if (SystemConfig.Errors.ContainsKey(nameof(SystemConfig.Code).Camelize()))
                            Error += SystemConfig.Errors[nameof(SystemConfig.Code)];
                        if (SystemConfig.Errors.ContainsKey(nameof(SystemConfig.FreighterQuotientCost).Camelize()))
                            Error += SystemConfig.Errors[nameof(SystemConfig.FreighterQuotientCost)];
                        if (SystemConfig.Errors.ContainsKey(nameof(SystemConfig.DeliveryRadius).Camelize()))
                            Error += SystemConfig.Errors[nameof(SystemConfig.DeliveryRadius)];
                        if (SystemConfig.Errors.ContainsKey(nameof(SystemConfig.DeliveryServiceDuration).Camelize()))
                            Error += SystemConfig.Errors[nameof(SystemConfig.DeliveryServiceDuration)];
                        Errors.Add(Error);
                    }
                }
                return BadRequest(Errors);
            }
        }
        
        [Route(SystemConfigRoute.Export), HttpPost]
        public async Task<ActionResult> Export([FromBody] SystemConfig_SystemConfigFilterDTO SystemConfig_SystemConfigFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            var SystemConfigFilter = ConvertFilterDTOToFilterEntity(SystemConfig_SystemConfigFilterDTO);
            SystemConfigFilter.Skip = 0;
            SystemConfigFilter.Take = int.MaxValue;
            SystemConfigFilter = await SystemConfigService.ToFilter(SystemConfigFilter);
            List<SystemConfig> SystemConfigs = await SystemConfigService.List(SystemConfigFilter);
            List<SystemConfig_SystemConfigExportDTO> SystemConfig_SystemConfigExportDTOs = SystemConfigs.Select(x => new SystemConfig_SystemConfigExportDTO(x)).ToList();    

            string path = "Templates/SystemConfig_Export.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            Data.Data = SystemConfig_SystemConfigExportDTOs;

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
            return File(output.ToArray(), "application/octet-stream", "SystemConfig.xlsx");
        }

        [Route(SystemConfigRoute.ExportTemplate), HttpPost]
        public async Task<ActionResult> ExportTemplate([FromBody] SystemConfig_SystemConfigFilterDTO SystemConfig_SystemConfigFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            string path = "Templates/SystemConfig_Template.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {
                document.Process(Data);
            };
            return File(output.ToArray(), "application/octet-stream", "SystemConfig.xlsx");
        }

        private async Task<bool> HasPermission(long Id)
        {
            SystemConfigFilter SystemConfigFilter = new SystemConfigFilter();
            SystemConfigFilter = await SystemConfigService.ToFilter(SystemConfigFilter);
            if (Id == 0)
            {

            }
            else
            {
                SystemConfigFilter.Id = new IdFilter { Equal = Id };
                int count = await SystemConfigService.Count(SystemConfigFilter);
                if (count == 0)
                    return false;
            }
            return true;
        }

        private SystemConfig ConvertDTOToEntity(SystemConfig_SystemConfigDTO SystemConfig_SystemConfigDTO)
        {
            SystemConfig_SystemConfigDTO.TrimString();
            SystemConfig SystemConfig = new SystemConfig();
            SystemConfig.Id = SystemConfig_SystemConfigDTO.Id;
            SystemConfig.Code = SystemConfig_SystemConfigDTO.Code;
            SystemConfig.FreighterQuotientCost = SystemConfig_SystemConfigDTO.FreighterQuotientCost;
            SystemConfig.DeliveryRadius = SystemConfig_SystemConfigDTO.DeliveryRadius;
            SystemConfig.DeliveryServiceDuration = SystemConfig_SystemConfigDTO.DeliveryServiceDuration;
            SystemConfig.BaseLanguage = CurrentContext.Language;
            return SystemConfig;
        }

        private SystemConfigFilter ConvertFilterDTOToFilterEntity(SystemConfig_SystemConfigFilterDTO SystemConfig_SystemConfigFilterDTO)
        {
            SystemConfigFilter SystemConfigFilter = new SystemConfigFilter();
            SystemConfigFilter.Selects = SystemConfigSelect.ALL;
            SystemConfigFilter.Skip = SystemConfig_SystemConfigFilterDTO.Skip;
            SystemConfigFilter.Take = SystemConfig_SystemConfigFilterDTO.Take;
            SystemConfigFilter.OrderBy = SystemConfig_SystemConfigFilterDTO.OrderBy;
            SystemConfigFilter.OrderType = SystemConfig_SystemConfigFilterDTO.OrderType;

            SystemConfigFilter.Id = SystemConfig_SystemConfigFilterDTO.Id;
            SystemConfigFilter.Code = SystemConfig_SystemConfigFilterDTO.Code;
            SystemConfigFilter.FreighterQuotientCost = SystemConfig_SystemConfigFilterDTO.FreighterQuotientCost;
            SystemConfigFilter.DeliveryRadius = SystemConfig_SystemConfigFilterDTO.DeliveryRadius;
            SystemConfigFilter.DeliveryServiceDuration = SystemConfig_SystemConfigFilterDTO.DeliveryServiceDuration;
            SystemConfigFilter.CreatedAt = SystemConfig_SystemConfigFilterDTO.CreatedAt;
            SystemConfigFilter.UpdatedAt = SystemConfig_SystemConfigFilterDTO.UpdatedAt;
            return SystemConfigFilter;
        }
    }
}

