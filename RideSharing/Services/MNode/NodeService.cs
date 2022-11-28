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

namespace RideSharing.Services.MNode
{
    public interface INodeService :  IServiceScoped
    {
        Task<int> Count(NodeFilter NodeFilter);
        Task<List<Node>> List(NodeFilter NodeFilter);
        Task<Node> Get(long Id);
        Task<Node> Create(Node Node);
        Task<Node> Update(Node Node);
        Task<Node> Delete(Node Node);
        Task<List<Node>> BulkDelete(List<Node> Nodes);
        Task<List<Node>> BulkMerge(List<Node> Nodes);
        Task<NodeFilter> ToFilter(NodeFilter NodeFilter);
    }

    public class NodeService : BaseService, INodeService
    {
        private IUOW UOW;
        private IRabbitManager RabbitManager;
        private ILogging Logging;
        private ICurrentContext CurrentContext;
        
        private INodeValidator NodeValidator;

        public NodeService(
            IUOW UOW,
            ICurrentContext CurrentContext,
            IRabbitManager RabbitManager,
            INodeValidator NodeValidator,
            ILogging Logging
        )
        {
            this.UOW = UOW;
            this.RabbitManager = RabbitManager;
            this.CurrentContext = CurrentContext;
            this.Logging = Logging;
           
            this.NodeValidator = NodeValidator;
        }

        public async Task<int> Count(NodeFilter NodeFilter)
        {
            try
            {
                int result = await UOW.NodeRepository.Count(NodeFilter);
                return result;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(NodeService));
            }
            return 0;
        }

        public async Task<List<Node>> List(NodeFilter NodeFilter)
        {
            try
            {
                List<Node> Nodes = await UOW.NodeRepository.List(NodeFilter);
                return Nodes;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(NodeService));
            }
            return null;
        }

        public async Task<Node> Get(long Id)
        {
            Node Node = await UOW.NodeRepository.Get(Id);
            if (Node == null)
                return null;
            await NodeValidator.Get(Node);
            return Node;
        }
        
        public async Task<Node> Create(Node Node)
        {
            if (!await NodeValidator.Create(Node))
                return Node;

            try
            {
                await UOW.NodeRepository.Create(Node);
                Node = await UOW.NodeRepository.Get(Node.Id);
                Sync(new List<Node> { Node });
                return Node;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(NodeService));
            }
            return null;
        }

        public async Task<Node> Update(Node Node)
        {
            if (!await NodeValidator.Update(Node))
                return Node;
            try
            {
                var oldData = await UOW.NodeRepository.Get(Node.Id);

                await UOW.NodeRepository.Update(Node);

                Node = await UOW.NodeRepository.Get(Node.Id);
                Sync(new List<Node> { Node });
                return Node;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(NodeService));
            }
            return null;
        }

        public async Task<Node> Delete(Node Node)
        {
            if (!await NodeValidator.Delete(Node))
                return Node;

            try
            {
                await UOW.NodeRepository.Delete(Node);
                return Node;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(NodeService));
            }
            return null;
        }

        public async Task<List<Node>> BulkDelete(List<Node> Nodes)
        {
            if (!await NodeValidator.BulkDelete(Nodes))
                return Nodes;

            try
            {
                await UOW.NodeRepository.BulkDelete(Nodes);
                return Nodes;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(NodeService));
            }
            return null;
        }

        public async Task<List<Node>> BulkMerge(List<Node> Nodes)
        {
            if (!await NodeValidator.Import(Nodes))
                return Nodes;
            try
            {
                var Ids = await UOW.NodeRepository.BulkMerge(Nodes);
                Nodes = await UOW.NodeRepository.List(Ids);
                Sync(Nodes);
                return Nodes;
            }
            catch (Exception ex)
            {
                Logging.CreateSystemLog(ex, nameof(NodeService));
            }
            return null;
        }     
        
        public async Task<NodeFilter> ToFilter(NodeFilter filter)
        {
            if (filter.OrFilter == null) filter.OrFilter = new List<NodeFilter>();
            if (CurrentContext.Filters == null || CurrentContext.Filters.Count == 0) return filter;
            foreach (var currentFilter in CurrentContext.Filters)
            {
                NodeFilter subFilter = new NodeFilter();
                filter.OrFilter.Add(subFilter);
                List<FilterPermissionDefinition> FilterPermissionDefinitions = currentFilter.Value;
                foreach (FilterPermissionDefinition FilterPermissionDefinition in FilterPermissionDefinitions)
                {
                    if (FilterPermissionDefinition.Name == "NodeId")
                        subFilter.Id = FilterBuilder.Merge(subFilter.Id, FilterPermissionDefinition.IdFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Code))
                        subFilter.Code = FilterBuilder.Merge(subFilter.Code, FilterPermissionDefinition.StringFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Longtitude))
                        subFilter.Longtitude = FilterBuilder.Merge(subFilter.Longtitude, FilterPermissionDefinition.DecimalFilter);
                    if (FilterPermissionDefinition.Name == nameof(subFilter.Latitude))
                        subFilter.Latitude = FilterBuilder.Merge(subFilter.Latitude, FilterPermissionDefinition.DecimalFilter);
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

        private void Sync(List<Node> Nodes)
        {


            
        }
    }
}
