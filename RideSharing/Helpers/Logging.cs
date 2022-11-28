using TrueSight.Common;
using RideSharing.Common;
using RideSharing.Entities;
using RideSharing.Enums;
using RideSharing.Handlers;
using RideSharing.Repositories;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RideSharing.Handlers.Configuration;

namespace RideSharing.Helpers
{
    public interface ILogging : IServiceScoped
    {
        void CreateAuditLog(object newData, object oldData, string className, [CallerMemberName] string methodName = "");
        void CreateSystemLog(Exception ex, string className, [CallerMemberName] string methodName = "");
    }
    public class Logging : ILogging
    {
        private ICurrentContext CurrentContext;
        private IRabbitManager RabbitManager;
        public Logging(ICurrentContext CurrentContext,IRabbitManager RabbitManager)
        {
            this.CurrentContext = CurrentContext;
            this.RabbitManager = RabbitManager;
        }
        public void CreateAuditLog(object newData, object oldData, string className, [CallerMemberName] string methodName = "")
        {
        }
        public void CreateSystemLog(Exception ex, string className, [CallerMemberName] string methodName = "")
        {
        }
    }
}
