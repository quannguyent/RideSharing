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
using RideSharing.Services.MNode;

namespace RideSharing.Rpc.node
{
    public partial class NodeController : RpcController
    {
        private INodeService NodeService;
        private ICurrentContext CurrentContext;
        public NodeController(
            INodeService NodeService,
            ICurrentContext CurrentContext
        )
        {
            this.NodeService = NodeService;
            this.CurrentContext = CurrentContext;
        }

        [Route(NodeRoute.Count), HttpPost]
        public async Task<ActionResult<int>> Count([FromBody] Node_NodeFilterDTO Node_NodeFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            NodeFilter NodeFilter = ConvertFilterDTOToFilterEntity(Node_NodeFilterDTO);
            NodeFilter = await NodeService.ToFilter(NodeFilter);
            int count = await NodeService.Count(NodeFilter);
            return count;
        }

        [Route(NodeRoute.List), HttpPost]
        public async Task<ActionResult<List<Node_NodeDTO>>> List([FromBody] Node_NodeFilterDTO Node_NodeFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            NodeFilter NodeFilter = ConvertFilterDTOToFilterEntity(Node_NodeFilterDTO);
            NodeFilter = await NodeService.ToFilter(NodeFilter);
            List<Node> Nodes = await NodeService.List(NodeFilter);
            List<Node_NodeDTO> Node_NodeDTOs = Nodes
                .Select(c => new Node_NodeDTO(c)).ToList();
            return Node_NodeDTOs;
        }

        [Route(NodeRoute.Get), HttpPost]
        public async Task<ActionResult<Node_NodeDTO>> Get([FromBody]Node_NodeDTO Node_NodeDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(Node_NodeDTO.Id))
                return Forbid();

            Node Node = await NodeService.Get(Node_NodeDTO.Id);
            return new Node_NodeDTO(Node);
        }

        [Route(NodeRoute.Create), HttpPost]
        public async Task<ActionResult<Node_NodeDTO>> Create([FromBody] Node_NodeDTO Node_NodeDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(Node_NodeDTO.Id))
                return Forbid();

            Node Node = ConvertDTOToEntity(Node_NodeDTO);
            Node = await NodeService.Create(Node);
            Node_NodeDTO = new Node_NodeDTO(Node);
            if (Node.IsValidated)
                return Node_NodeDTO;
            else
                return BadRequest(Node_NodeDTO);
        }

        [Route(NodeRoute.Update), HttpPost]
        public async Task<ActionResult<Node_NodeDTO>> Update([FromBody] Node_NodeDTO Node_NodeDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            if (!await HasPermission(Node_NodeDTO.Id))
                return Forbid();

            Node Node = ConvertDTOToEntity(Node_NodeDTO);
            Node = await NodeService.Update(Node);
            Node_NodeDTO = new Node_NodeDTO(Node);
            if (Node.IsValidated)
                return Node_NodeDTO;
            else
                return BadRequest(Node_NodeDTO);
        }

        [Route(NodeRoute.Delete), HttpPost]
        public async Task<ActionResult<Node_NodeDTO>> Delete([FromBody] Node_NodeDTO Node_NodeDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            if (!await HasPermission(Node_NodeDTO.Id))
                return Forbid();

            Node Node = ConvertDTOToEntity(Node_NodeDTO);
            Node = await NodeService.Delete(Node);
            Node_NodeDTO = new Node_NodeDTO(Node);
            if (Node.IsValidated)
                return Node_NodeDTO;
            else
                return BadRequest(Node_NodeDTO);
        }
        
        [Route(NodeRoute.BulkDelete), HttpPost]
        public async Task<ActionResult<bool>> BulkDelete([FromBody] List<long> Ids)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);

            NodeFilter NodeFilter = new NodeFilter();
            NodeFilter = await NodeService.ToFilter(NodeFilter);
            NodeFilter.Id = new IdFilter { In = Ids };
            NodeFilter.Selects = NodeSelect.Id;
            NodeFilter.Skip = 0;
            NodeFilter.Take = int.MaxValue;

            List<Node> Nodes = await NodeService.List(NodeFilter);
            Nodes = await NodeService.BulkDelete(Nodes);
            if (Nodes.Any(x => !x.IsValidated))
                return BadRequest(Nodes.Where(x => !x.IsValidated));
            return true;
        }
        
        [Route(NodeRoute.Import), HttpPost]
        public async Task<ActionResult> Import(IFormFile file)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            List<Node> Nodes = new List<Node>();
            using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return Ok(Nodes);
                int StartColumn = 1;
                int StartRow = 1;
                int IdColumn = 0 + StartColumn;
                int CodeColumn = 1 + StartColumn;
                int LongtitudeColumn = 2 + StartColumn;
                int LatitudeColumn = 3 + StartColumn;

                for (int i = StartRow; i <= worksheet.Dimension.End.Row; i++)
                {
                    if (string.IsNullOrEmpty(worksheet.Cells[i, StartColumn].Value?.ToString()))
                        break;
                    string IdValue = worksheet.Cells[i, IdColumn].Value?.ToString();
                    string CodeValue = worksheet.Cells[i, CodeColumn].Value?.ToString();
                    string LongtitudeValue = worksheet.Cells[i, LongtitudeColumn].Value?.ToString();
                    string LatitudeValue = worksheet.Cells[i, LatitudeColumn].Value?.ToString();
                    
                    Node Node = new Node();
                    Node.Code = CodeValue;
                    Node.Longtitude = decimal.TryParse(LongtitudeValue, out decimal Longtitude) ? Longtitude : 0;
                    Node.Latitude = decimal.TryParse(LatitudeValue, out decimal Latitude) ? Latitude : 0;
                    
                    Nodes.Add(Node);
                }
            }
            Nodes = await NodeService.BulkMerge(Nodes);
            if (Nodes.All(x => x.IsValidated))
                return Ok(true);
            else
            {
                List<string> Errors = new List<string>();
                for (int i = 0; i < Nodes.Count; i++)
                {
                    Node Node = Nodes[i];
                    if (!Node.IsValidated)
                    {
                        string Error = $"Dòng {i + 2} có lỗi:";
                        if (Node.Errors.ContainsKey(nameof(Node.Id).Camelize()))
                            Error += Node.Errors[nameof(Node.Id)];
                        if (Node.Errors.ContainsKey(nameof(Node.Code).Camelize()))
                            Error += Node.Errors[nameof(Node.Code)];
                        if (Node.Errors.ContainsKey(nameof(Node.Longtitude).Camelize()))
                            Error += Node.Errors[nameof(Node.Longtitude)];
                        if (Node.Errors.ContainsKey(nameof(Node.Latitude).Camelize()))
                            Error += Node.Errors[nameof(Node.Latitude)];
                        Errors.Add(Error);
                    }
                }
                return BadRequest(Errors);
            }
        }
        
        [Route(NodeRoute.Export), HttpPost]
        public async Task<ActionResult> Export([FromBody] Node_NodeFilterDTO Node_NodeFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            var NodeFilter = ConvertFilterDTOToFilterEntity(Node_NodeFilterDTO);
            NodeFilter.Skip = 0;
            NodeFilter.Take = int.MaxValue;
            NodeFilter = await NodeService.ToFilter(NodeFilter);
            List<Node> Nodes = await NodeService.List(NodeFilter);
            List<Node_NodeExportDTO> Node_NodeExportDTOs = Nodes.Select(x => new Node_NodeExportDTO(x)).ToList();    

            string path = "Templates/Node_Export.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            Data.Data = Node_NodeExportDTOs;

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
            return File(output.ToArray(), "application/octet-stream", "Node.xlsx");
        }

        [Route(NodeRoute.ExportTemplate), HttpPost]
        public async Task<ActionResult> ExportTemplate([FromBody] Node_NodeFilterDTO Node_NodeFilterDTO)
        {
            if (!ModelState.IsValid)
                throw new BindException(ModelState);
            
            string path = "Templates/Node_Template.xlsx";
            byte[] arr = System.IO.File.ReadAllBytes(path);
            MemoryStream input = new MemoryStream(arr);
            MemoryStream output = new MemoryStream();
            dynamic Data = new ExpandoObject();
            using (var document = StaticParams.DocumentFactory.Open(input, output, "xlsx"))
            {
                document.Process(Data);
            };
            return File(output.ToArray(), "application/octet-stream", "Node.xlsx");
        }

        private async Task<bool> HasPermission(long Id)
        {
            NodeFilter NodeFilter = new NodeFilter();
            NodeFilter = await NodeService.ToFilter(NodeFilter);
            if (Id == 0)
            {

            }
            else
            {
                NodeFilter.Id = new IdFilter { Equal = Id };
                int count = await NodeService.Count(NodeFilter);
                if (count == 0)
                    return false;
            }
            return true;
        }

        private Node ConvertDTOToEntity(Node_NodeDTO Node_NodeDTO)
        {
            Node_NodeDTO.TrimString();
            Node Node = new Node();
            Node.Id = Node_NodeDTO.Id;
            Node.Code = Node_NodeDTO.Code;
            Node.Longtitude = Node_NodeDTO.Longtitude;
            Node.Latitude = Node_NodeDTO.Latitude;
            Node.BaseLanguage = CurrentContext.Language;
            return Node;
        }

        private NodeFilter ConvertFilterDTOToFilterEntity(Node_NodeFilterDTO Node_NodeFilterDTO)
        {
            NodeFilter NodeFilter = new NodeFilter();
            NodeFilter.Selects = NodeSelect.ALL;
            NodeFilter.Skip = Node_NodeFilterDTO.Skip;
            NodeFilter.Take = Node_NodeFilterDTO.Take;
            NodeFilter.OrderBy = Node_NodeFilterDTO.OrderBy;
            NodeFilter.OrderType = Node_NodeFilterDTO.OrderType;

            NodeFilter.Id = Node_NodeFilterDTO.Id;
            NodeFilter.Code = Node_NodeFilterDTO.Code;
            NodeFilter.Longtitude = Node_NodeFilterDTO.Longtitude;
            NodeFilter.Latitude = Node_NodeFilterDTO.Latitude;
            return NodeFilter;
        }
    }
}

