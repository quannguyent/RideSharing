using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideSharing.Enums
{
    public class CurrentUserEnum
    {
        public static GenericEnum ISNT = new GenericEnum(0, "", "");
        public static GenericEnum IS = new GenericEnum(1, "", "");
        public static List<GenericEnum> CurrentUserEnumList = new List<GenericEnum>
        {
            IS,ISNT,
        };
    }
}
