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

namespace RideSharing.Services.MNode
{
    public interface INodeValidator : IServiceScoped
    {
        Task Get(Node Node);
        Task<bool> Create(Node Node);
        Task<bool> Update(Node Node);
        Task<bool> Delete(Node Node);
        Task<bool> BulkDelete(List<Node> Nodes);
        Task<bool> Import(List<Node> Nodes);
    }

    public class NodeValidator : INodeValidator
    {
        private IUOW UOW;
        private ICurrentContext CurrentContext;
        private NodeMessage NodeMessage;

        public NodeValidator(IUOW UOW, ICurrentContext CurrentContext)
        {
            this.UOW = UOW;
            this.CurrentContext = CurrentContext;
            this.NodeMessage = new NodeMessage();
        }

        public async Task Get(Node Node)
        {
        }

        public async Task<bool> Create(Node Node)
        {
            await ValidateCode(Node);
            await ValidateLongtitude(Node);
            await ValidateLatitude(Node);
            return Node.IsValidated;
        }

        public async Task<bool> Update(Node Node)
        {
            if (await ValidateId(Node))
            {
                await ValidateCode(Node);
                await ValidateLongtitude(Node);
                await ValidateLatitude(Node);
            }
            return Node.IsValidated;
        }

        public async Task<bool> Delete(Node Node)
        {
            var oldData = await UOW.NodeRepository.Get(Node.Id);
            if (oldData != null)
            {
            }
            else
            {
                Node.AddError(nameof(NodeValidator), nameof(Node.Id), NodeMessage.Error.IdNotExisted, NodeMessage);
            }
            return Node.IsValidated;
        }
        
        public async Task<bool> BulkDelete(List<Node> Nodes)
        {
            return Nodes.All(x => x.IsValidated);
        }

        public async Task<bool> Import(List<Node> Nodes)
        {
            return true;
        }
        
        private async Task<bool> ValidateId(Node Node)
        {
            NodeFilter NodeFilter = new NodeFilter
            {
                Skip = 0,
                Take = 10,
                Id = new IdFilter { Equal = Node.Id },
                Selects = NodeSelect.Id
            };

            int count = await UOW.NodeRepository.CountAll(NodeFilter);
            if (count == 0)
                Node.AddError(nameof(NodeValidator), nameof(Node.Id), NodeMessage.Error.IdNotExisted, NodeMessage);
            return Node.IsValidated;
        }

        private async Task<bool> ValidateCode(Node Node)
        {
            if(string.IsNullOrEmpty(Node.Code))
            {
                Node.AddError(nameof(NodeValidator), nameof(Node.Code), NodeMessage.Error.CodeEmpty, NodeMessage);
            }
            else if(Node.Code.Count() > 500)
            {
                Node.AddError(nameof(NodeValidator), nameof(Node.Code), NodeMessage.Error.CodeOverLength, NodeMessage);
            }
            return Node.IsValidated;
        }
        private async Task<bool> ValidateLongtitude(Node Node)
        {   
            return true;
        }
        private async Task<bool> ValidateLatitude(Node Node)
        {   
            return true;
        }
    }
}
