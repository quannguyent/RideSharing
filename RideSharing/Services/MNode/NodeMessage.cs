using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideSharing.Services.MNode
{
    public class NodeMessage
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
            CodeEmpty,
            CodeOverLength,
        }
    }
}
