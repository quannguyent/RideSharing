using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideSharing.Services.MDeliveryTrip
{
    public class DeliveryTripMessage
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
            PathEmpty,
            PathOverLength,
            BusStopEmpty,
            BusStopNotExisted,
            CityFreighterEmpty,
            CityFreighterNotExisted,
        }
    }
}
