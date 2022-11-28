using TrueSight.Common;
using RideSharing.Common;
using System;

namespace RideSharing.Entities
{
    public class SystemLog : DataEntity
    {
        public long Id { get; set; }
        public long? AppUserId { get; set; }
        public string AppUser { get; set; }
        public string Exception { get; set; }
        public string ModuleName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public DateTime Time { get; set; }
        public Guid RowId { get; set; }
    }
}
