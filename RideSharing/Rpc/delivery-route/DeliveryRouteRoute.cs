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
using RideSharing.Services.MDeliveryRoute;
using System.ComponentModel;
using RideSharing.Services.MCityFreighter;

namespace RideSharing.Rpc.delivery_route
{
    [DisplayName("DeliveryRoute")]
    public class DeliveryRouteRoute : Root
    {
        public const string Parent = Module + "/delivery-route";
        public const string Master = Module + "/delivery-route/delivery-route-master";
        public const string Detail = Module + "/delivery-route/delivery-route-detail";
        public const string Preview = Module + "/delivery-route/delivery-route-preview";
        private const string Default = Rpc + Module + "/delivery-route";
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
        
        public const string FilterListCityFreighter = Default + "/filter-list-city-freighter";

        public const string SingleListCityFreighter = Default + "/single-list-city-freighter";


        public static Dictionary<string, long> Filters = new Dictionary<string, long>
        {
            { "DeliveryRouteId", FieldTypeEnum.ID.Id },
            { nameof(DeliveryRouteFilter.Path), FieldTypeEnum.STRING.Id },
            { nameof(DeliveryRouteFilter.CityFreighterId), FieldTypeEnum.ID.Id },
            { nameof(DeliveryRouteFilter.TotalTravelDistance), FieldTypeEnum.DECIMAL.Id },
            { nameof(DeliveryRouteFilter.TotalEmptyRunDistance), FieldTypeEnum.DECIMAL.Id },
        };

        private static List<string> FilterList = new List<string> { 
            FilterListCityFreighter,
        };
        private static List<string> SingleList = new List<string> { 
            SingleListCityFreighter, 
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
