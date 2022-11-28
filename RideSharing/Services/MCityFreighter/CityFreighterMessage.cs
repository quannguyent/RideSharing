using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideSharing.Services.MCityFreighter
{
    public class CityFreighterMessage
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
