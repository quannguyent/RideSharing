using TrueSight.Common;
using RideSharing.Common;
using RideSharing.Helpers;
using RideSharing.Entities;
using RideSharing.Models;
using RideSharing.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace RideSharing.Repositories
{
    public interface INodeRepository
    {
        Task<int> CountAll(NodeFilter NodeFilter);
        Task<int> Count(NodeFilter NodeFilter);
        Task<List<Node>> List(NodeFilter NodeFilter);
        Task<List<Node>> List(List<long> Ids);
        Task<Node> Get(long Id);
        Task<bool> Create(Node Node);
        Task<bool> Update(Node Node);
        Task<bool> Delete(Node Node);
        Task<List<long>> BulkMerge(List<Node> Nodes);
        Task<bool> BulkDelete(List<Node> Nodes);
    }
    public class NodeRepository : INodeRepository
    {
        private DataContext DataContext;
        public NodeRepository(DataContext DataContext)
        {
            this.DataContext = DataContext;
        }

        private async Task<IQueryable<NodeDAO>> DynamicFilter(IQueryable<NodeDAO> query, NodeFilter filter)
        {
            if (filter == null)
                return query.Where(q => false);
            query = query.Where(q => q.Id, filter.Id);
            query = query.Where(q => q.Code, filter.Code);
            query = query.Where(q => q.Longtitude, filter.Longtitude);
            query = query.Where(q => q.Latitude, filter.Latitude);
            return query;
        }

        private async Task<IQueryable<NodeDAO>> OrFilter(IQueryable<NodeDAO> query, NodeFilter filter)
        {
            if (filter.OrFilter == null || filter.OrFilter.Count == 0)
                return query;
            IQueryable<NodeDAO> initQuery = query.Where(q => false);
            foreach (NodeFilter NodeFilter in filter.OrFilter)
            {
                IQueryable<NodeDAO> queryable = query;
                queryable = queryable.Where(q => q.Id, NodeFilter.Id);
                queryable = queryable.Where(q => q.Code, NodeFilter.Code);
                queryable = queryable.Where(q => q.Longtitude, NodeFilter.Longtitude);
                queryable = queryable.Where(q => q.Latitude, NodeFilter.Latitude);
                initQuery = initQuery.Union(queryable);
            }
            return initQuery;
        }    

        private IQueryable<NodeDAO> DynamicOrder(IQueryable<NodeDAO> query, NodeFilter filter)
        {
            switch (filter.OrderType)
            {
                case OrderType.ASC:
                    switch (filter.OrderBy)
                    {
                        case NodeOrder.Id:
                            query = query.OrderBy(q => q.Id);
                            break;
                        case NodeOrder.Code:
                            query = query.OrderBy(q => q.Code);
                            break;
                        case NodeOrder.Longtitude:
                            query = query.OrderBy(q => q.Longtitude);
                            break;
                        case NodeOrder.Latitude:
                            query = query.OrderBy(q => q.Latitude);
                            break;
                    }
                    break;
                case OrderType.DESC:
                    switch (filter.OrderBy)
                    {
                        case NodeOrder.Id:
                            query = query.OrderByDescending(q => q.Id);
                            break;
                        case NodeOrder.Code:
                            query = query.OrderByDescending(q => q.Code);
                            break;
                        case NodeOrder.Longtitude:
                            query = query.OrderByDescending(q => q.Longtitude);
                            break;
                        case NodeOrder.Latitude:
                            query = query.OrderByDescending(q => q.Latitude);
                            break;
                    }
                    break;
            }
            query = query.Skip(filter.Skip).Take(filter.Take);
            return query;
        }

        private async Task<List<Node>> DynamicSelect(IQueryable<NodeDAO> query, NodeFilter filter)
        {
            List<Node> Nodes = await query.Select(q => new Node()
            {
                Id = filter.Selects.Contains(NodeSelect.Id) ? q.Id : default(long),
                Code = filter.Selects.Contains(NodeSelect.Code) ? q.Code : default(string),
                Longtitude = filter.Selects.Contains(NodeSelect.Longtitude) ? q.Longtitude : default(decimal),
                Latitude = filter.Selects.Contains(NodeSelect.Latitude) ? q.Latitude : default(decimal),
            }).ToListAsync();
            return Nodes;
        }

        public async Task<int> CountAll(NodeFilter filter)
        {
            IQueryable<NodeDAO> NodeDAOs = DataContext.Node.AsNoTracking();
            NodeDAOs = await DynamicFilter(NodeDAOs, filter);
            return await NodeDAOs.CountAsync();
        }

        public async Task<int> Count(NodeFilter filter)
        {
            IQueryable<NodeDAO> NodeDAOs = DataContext.Node.AsNoTracking();
            NodeDAOs = await DynamicFilter(NodeDAOs, filter);
            NodeDAOs = await OrFilter(NodeDAOs, filter);
            return await NodeDAOs.CountAsync();
        }

        public async Task<List<Node>> List(NodeFilter filter)
        {
            if (filter == null) return new List<Node>();
            IQueryable<NodeDAO> NodeDAOs = DataContext.Node.AsNoTracking();
            NodeDAOs = await DynamicFilter(NodeDAOs, filter);
            NodeDAOs = await OrFilter(NodeDAOs, filter);
            NodeDAOs = DynamicOrder(NodeDAOs, filter);
            List<Node> Nodes = await DynamicSelect(NodeDAOs, filter);
            return Nodes;
        }

        public async Task<List<Node>> List(List<long> Ids)
        {
            IdFilter IdFilter = new IdFilter { In = Ids };

            IQueryable<NodeDAO> query = DataContext.Node.AsNoTracking();
            query = query.Where(q => q.Id, IdFilter);
            List<Node> Nodes = await query.AsNoTracking()
            .Select(x => new Node()
            {
                Id = x.Id,
                Code = x.Code,
                Longtitude = x.Longtitude,
                Latitude = x.Latitude,
            }).ToListAsync();
            

            return Nodes;
        }

        public async Task<Node> Get(long Id)
        {
            Node Node = await DataContext.Node.AsNoTracking()
            .Where(x => x.Id == Id)
            .Select(x => new Node()
            {
                Id = x.Id,
                Code = x.Code,
                Longtitude = x.Longtitude,
                Latitude = x.Latitude,
            }).FirstOrDefaultAsync();

            if (Node == null)
                return null;

            return Node;
        }
        public async Task<bool> Create(Node Node)
        {
            NodeDAO NodeDAO = new NodeDAO();
            NodeDAO.Id = Node.Id;
            NodeDAO.Code = Node.Code;
            NodeDAO.Longtitude = Node.Longtitude;
            NodeDAO.Latitude = Node.Latitude;
            DataContext.Node.Add(NodeDAO);
            await DataContext.SaveChangesAsync();
            Node.Id = NodeDAO.Id;
            await SaveReference(Node);
            return true;
        }

        public async Task<bool> Update(Node Node)
        {
            NodeDAO NodeDAO = DataContext.Node
                .Where(x => x.Id == Node.Id)
                .FirstOrDefault();
            if (NodeDAO == null)
                return false;
            NodeDAO.Id = Node.Id;
            NodeDAO.Code = Node.Code;
            NodeDAO.Longtitude = Node.Longtitude;
            NodeDAO.Latitude = Node.Latitude;
            await DataContext.SaveChangesAsync();
            await SaveReference(Node);
            return true;
        }

        public async Task<bool> Delete(Node Node)
        {
            await DataContext.Node
                .Where(x => x.Id == Node.Id)
                .DeleteFromQueryAsync();
            return true;
        }

        public async Task<List<long>> BulkMerge(List<Node> Nodes)
        {
            IdFilter IdFilter = new IdFilter { In = Nodes.Select(x => x.Id).ToList() };
            List<NodeDAO> Inserts = new List<NodeDAO>();
            List<NodeDAO> Updates = new List<NodeDAO>();
            List<NodeDAO> DbNodeDAOs = await DataContext.Node
                .Where(x => x.Id, IdFilter)
                .ToListAsync();
            foreach (Node Node in Nodes)
            {
                NodeDAO NodeDAO = DbNodeDAOs
                        .Where(x => x.Id == Node.Id)
                        .FirstOrDefault();
                if (NodeDAO == null)
                {
                    NodeDAO = new NodeDAO();
                    Inserts.Add(NodeDAO);
                }
                else
                {
                    Updates.Add(NodeDAO);
                }
                NodeDAO.Code = Node.Code;
                NodeDAO.Longtitude = Node.Longtitude;
                NodeDAO.Latitude = Node.Latitude;
            }
            await DataContext.Node.BulkInsertAsync(Inserts);
            await DataContext.Node.BulkMergeAsync(Updates);
            var Ids = Inserts.Select(x => x.Id).ToList();
            Ids.AddRange(Updates.Select(x => x.Id));
            Ids = Ids.Distinct().ToList();
            return Ids;
        }
        
        public async Task<bool> BulkDelete(List<Node> Nodes)
        {
            List<long> Ids = Nodes.Select(x => x.Id).ToList();
            await DataContext.Node
                .WhereBulkContains(Ids, x => x.Id)
                .DeleteFromQueryAsync();
            return true;
        }

        private async Task SaveReference(Node Node)
        {
        }

    }
}
