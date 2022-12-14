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
using RideSharing.Services.MCityFreighter;
using System.ComponentModel;

namespace RideSharing.Rpc.city_freighter
{
    [DisplayName("CityFreighter")]
    public class CityFreighterRoute : Root
    {
        public const string Parent = Module + "/city-freighter";
        public const string Master = Module + "/city-freighter/city-freighter-master";
        public const string Detail = Module + "/city-freighter/city-freighter-detail";
        public const string Preview = Module + "/city-freighter/city-freighter-preview";
        private const string Default = Rpc + Module + "/city-freighter";
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
            { "CityFreighterId", FieldTypeEnum.ID.Id },
            { nameof(CityFreighterFilter.Name), FieldTypeEnum.STRING.Id },
            { nameof(CityFreighterFilter.Capacity), FieldTypeEnum.DECIMAL.Id },
            { nameof(CityFreighterFilter.Latitude), FieldTypeEnum.DECIMAL.Id },
            { nameof(CityFreighterFilter.Longtitude), FieldTypeEnum.DECIMAL.Id },
        };

        private static List<string> FilterList = new List<string> { 
            
        };
        private static List<string> SingleList = new List<string> { 
            
        };
        private static List<string> CountList = new List<string> { 
            
        };
        
        public static Dictionary<string, IEnumerable<string>> Action = new Dictionary<string, IEnumerable<string>>
        {
            { "T??m ki???m", new List<string> { 
                    Parent,
                    Master, Preview, Count, List,
                    Get,  
                }.Concat(FilterList)
            },
            { "Th??m", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    Detail, Create, 
                }.Concat(SingleList).Concat(FilterList).Concat(CountList)
            },

            { "S???a", new List<string> { 
                    Parent,            
                    Master, Preview, Count, List, Get,
                    Detail, Update, 
                }.Concat(SingleList).Concat(FilterList).Concat(CountList)
            },

            { "Xo??", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    Delete, 
                }.Concat(SingleList).Concat(FilterList) 
            },

            { "Xo?? nhi???u", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    BulkDelete 
                }.Concat(FilterList) 
            },

            { "Xu???t excel", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    Export 
                }.Concat(FilterList) 
            },

            { "Nh???p excel", new List<string> { 
                    Parent,
                    Master, Preview, Count, List, Get,
                    ExportTemplate, Import 
                }.Concat(FilterList) 
            },
        };
    }
}
