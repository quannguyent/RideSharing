using TrueSight.Common;
using RideSharing.Common;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using RideSharing.Models;
using RideSharing.Repositories;
using System;

namespace RideSharing.Repositories
{
    public interface IUOW : IServiceScoped, IDisposable
    {
        Task Begin();
        Task Commit();
        Task Rollback();

        IBusStopRepository BusStopRepository { get; }
        ICityFreighterRepository CityFreighterRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        IDeliveryOrderRepository DeliveryOrderRepository { get; }
        IDeliveryRouteRepository DeliveryRouteRepository { get; }
        IDeliveryTripRepository DeliveryTripRepository { get; }
        INodeRepository NodeRepository { get; }
        ISystemConfigRepository SystemConfigRepository { get; }
    }

    public class UOW : IUOW
    {
        private DataContext DataContext;

        public IBusStopRepository BusStopRepository { get; private set; }
        public ICityFreighterRepository CityFreighterRepository { get; private set; }
        public ICustomerRepository CustomerRepository { get; private set; }
        public IDeliveryOrderRepository DeliveryOrderRepository { get; private set; }
        public IDeliveryRouteRepository DeliveryRouteRepository { get; private set; }
        public IDeliveryTripRepository DeliveryTripRepository { get; private set; }
        public INodeRepository NodeRepository { get; private set; }
        public ISystemConfigRepository SystemConfigRepository { get; private set; }

        public UOW(DataContext DataContext, IRedisStore RedisStore, IConfiguration Configuration)
        {
            this.DataContext = DataContext;

            BusStopRepository = new BusStopRepository(DataContext);
            CityFreighterRepository = new CityFreighterRepository(DataContext);
            CustomerRepository = new CustomerRepository(DataContext);
            DeliveryOrderRepository = new DeliveryOrderRepository(DataContext);
            DeliveryRouteRepository = new DeliveryRouteRepository(DataContext);
            DeliveryTripRepository = new DeliveryTripRepository(DataContext);
            NodeRepository = new NodeRepository(DataContext);
            SystemConfigRepository = new SystemConfigRepository(DataContext);
        }
        public async Task Begin()
        {
            return;
            await DataContext.Database.BeginTransactionAsync();
        }

        public Task Commit()
        {
            //DataContext.Database.CommitTransaction();
            return Task.CompletedTask;
        }

        public Task Rollback()
        {
            //DataContext.Database.RollbackTransaction();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this.DataContext == null)
            {
                return;
            }

            this.DataContext.Dispose();
            this.DataContext = null;
        }
    }
}