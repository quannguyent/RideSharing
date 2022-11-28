using RideSharing.Entities;
using RideSharing.Enums;
using RideSharing.Helpers;
using RabbitMQ.Client;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RideSharing.Handlers.Configuration
{
    public interface IHandler
    {
        string Name { get; }
        IServiceProvider ServiceProvider { get; set; }
        string SourceModuleName { get; set; }
        void QueueBind(IModel channel, string queue, string exchange);
        Task Handle(string routingKey, string content);
    }

    public abstract class Handler : IHandler
    {
        public abstract string Name { get; }
        public IServiceProvider ServiceProvider { get; set; }
        public string SourceModuleName { get; set; }

        public abstract Task Handle(string routingKey, string content);

        public abstract void QueueBind(IModel channel, string queue, string exchange);
    }
}
