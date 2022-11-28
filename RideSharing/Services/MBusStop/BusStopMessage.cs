using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideSharing.Services.MBusStop
{
    public class BusStopMessage
    {
        public enum Information
        {

        }

        public enum Warning
        {

        }

        public enum Error
        {
            IdNotExisted,
            ObjectUsed,
            NameEmpty,
            NameOverLength,
        }
    }
}
