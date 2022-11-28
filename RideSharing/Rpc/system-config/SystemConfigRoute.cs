using TrueSight.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RideSharing.Common;
using RideSharing.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using OfficeOpenXml;
using RideSharing.Entities;
using RideSharing.Services.MSystemConfig;
using System.ComponentModel;

namespace RideSharing.Rpc.system_config
{
    [DisplayName("SystemConfig")]
    public class SystemConfigRoute : Root
    {
        public const string Parent = Module + "/system-config";
        public const string Master = Module + "/system-config/system-config-master";
        public const string Detail = Module + "/system-config/system-config-detail";
        public const string Preview = Module + "/system-config/system-config-preview";
        private const string Default = Rpc + Module + "/system-config";
        public const string Count = Default + "/count";
        public const string List = Default + "/list";
        public const string Get = Default + "/get";
        public const string Create = Default + "/create";
        public const string Update = Default + "/update";
        public const string Delete = Default + "/delete";
        public const string Import = Default + "/import";
        public const string Export = Default + "/export";
        public const string ExportTemplate = Default + "/export-template";
        public const string BulkDelete = Default + "/bulk-delete";
        



        public static Dictionary<string, long> Filters = new Dictionary<string, long>
        {
            { "SystemConfigId", FieldTypeEnum.ID.Id },
            { nameof(SystemConfigFilter.Code), FieldTypeEnum.STRING.Id },
            { nameof(SystemConfigFilter.FreighterQuotientCost), FieldTypeEnum.DECIMAL.Id },
            { nameof(SystemConfigFilter.DeliveryRadius), FieldTypeEnum.DECIMAL.Id },
            { nameof(SystemConfigFilter.DeliveryServiceDuration), FieldTypeEnum.LONG.Id },
        };

        private static List<string> FilterList = new List<string> { 
            
        };
        private static List<string> SingleList = new List<string> { 
            
        };
        private static List<string> CountList = new List<string> { 
            
        };
        
        public static Dictionary<string, IEnumerable<string>> Action = new Dictionary<string, IEnumerable<string>>
        {
            { "Tìm kiếm", new List<string> { 
                    Parent,
                    Master, Preview, Count, List,
                    Get,  
                }.Concat(FilterList)
            },
            { "Thêm", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    Detail, Create, 
                }.Concat(SingleList).Concat(FilterList).Concat(CountList)
            },

            { "Sửa", new List<string> { 
                    Parent,            
                    Master, Preview, Count, List, Get,
                    Detail, Update, 
                }.Concat(SingleList).Concat(FilterList).Concat(CountList)
            },

            { "Xoá", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    Delete, 
                }.Concat(SingleList).Concat(FilterList) 
            },

            { "Xoá nhiều", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    BulkDelete 
                }.Concat(FilterList) 
            },

            { "Xuất excel", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    Export 
                }.Concat(FilterList) 
            },

            { "Nhập excel", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    ExportTemplate, Import 
                }.Concat(FilterList) 
            },
        };
    }
}
