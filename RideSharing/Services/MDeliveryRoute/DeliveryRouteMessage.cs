using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideSharing.Services.MDeliveryRoute
{
    public class DeliveryRouteMessage
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
            CityFreighterEmpty,
            CityFreighterNotExisted,
        }
    }
}
