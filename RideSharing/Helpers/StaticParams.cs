using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NGS.Templater;

namespace RideSharing.Helpers
{
    public class StaticParams
    {
        public static DateTime DateTimeNow => DateTime.UtcNow.AddHours(7);
        public static DateTime DateTimeMin => DateTime.MinValue;
        public static string ExcelFileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public static string ModuleName = "RideSharing";
        public static bool EnableExternalService = true;
        public static IDocumentFactory DocumentFactory => Configuration.Builder.Build("HaTM12@fpt.com.vn", "n8seTdTnm6lKE/bJAtLDfz2+YCES84tPeDpORepZ95ZtffIzEPc83DSzTJ74aUDt7qwochs3JjlyLJ3RAg809IGyiSTttv1iScB1KSdLHNdscIbkang7DV1f3uBA1RKncDBa1Y2UG/EMIhyEcm48COCYWQc1YNAjNBG9L5LsZ5JjEa8B55US5iRdEDPLuCB+Kdchw6+jM+k4vrQTTNbwYMSCSMpzk1APSmZCpsfjliQxsODHReOUwvBUPK8KOD9jqewrfFJ0Nh0ZNYMFHFZf1efi0oN2lG1l/lcEzTdIEi0BcLuUhXHStMQaqJmEf0voFjJfG0I/c+N30E50j0RncIxOKg8rEvrdASzGUkBdEeU4bNlq1FhGOAiUHPVi/B4oqKHEfFOwTsHfnUmSGRpufdffp9zBvg8DYPYaj00xaysZ7y2x7EQoIPX0KRXqY0Pt+AEEOp5KfsQmhsUKV2Ajd2+hb3PqBvD3W8I/miehwRXpNjzIuLCCTZanBtz+8r+me58loFYUY0fnKz5vSwZ1kNsCFFYR5f7ILBp+RdwgduhLQqlHsoTIn24zS3DCKLytxydz/J5O8TgZzwl6cgixZEtB6MVMbsokrhAH6XSs36sCuoxVpEwqIIz4al8AreU65IwZIlz4FZ19g2/Oarej+/qAdFTk3ih29Mwrnbj+dTw=");

        public static decimal CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad((lat2 - lat1));  // deg2rad below
            var dLon = deg2rad((lon2 - lon1));
            var a =
              Math.Sin((double)(dLat / 2)) * Math.Sin((double)(dLat / 2)) +
              Math.Cos((double)deg2rad(lat1)) * Math.Cos((double)deg2rad(lat2)) *
              Math.Sin((double)(dLon / 2)) * Math.Sin((double)(dLon / 2))
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return (decimal)d;
        }

        private static decimal deg2rad(decimal deg)
        {
            return (deg * ((decimal)Math.PI / 180));
}
    }
}
