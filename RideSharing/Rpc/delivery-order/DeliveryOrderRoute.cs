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
using RideSharing.Services.MDeliveryOrder;
using System.ComponentModel;
using RideSharing.Services.MCustomer;

namespace RideSharing.Rpc.delivery_order
{
    [DisplayName("DeliveryOrder")]
    public class DeliveryOrderRoute : Root
    {
        public const string Parent = Module + "/delivery-order";
        public const string Master = Module + "/delivery-order/delivery-order-master";
        public const string Detail = Module + "/delivery-order/delivery-order-detail";
        public const string Preview = Module + "/delivery-order/delivery-order-preview";
        private const string Default = Rpc + Module + "/delivery-order";
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
        
        public const string FilterListCustomer = Default + "/filter-list-customer";

        public const string SingleListCustomer = Default + "/single-list-customer";


        public static Dictionary<string, long> Filters = new Dictionary<string, long>
        {
            { "DeliveryOrderId", FieldTypeEnum.ID.Id },
            { nameof(DeliveryOrderFilter.Code), FieldTypeEnum.STRING.Id },
            { nameof(DeliveryOrderFilter.Weight), FieldTypeEnum.DECIMAL.Id },
            { nameof(DeliveryOrderFilter.CustomerId), FieldTypeEnum.ID.Id },
        };

        private static List<string> FilterList = new List<string> { 
            FilterListCustomer,
        };
        private static List<string> SingleList = new List<string> { 
            SingleListCustomer, 
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
