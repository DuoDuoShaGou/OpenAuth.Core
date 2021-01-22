using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.Mvc.Models
{
    public class OrderView
    {
       // PO_NO ITEM    Customer Buyer   PO_Date Name    Description Type    Project Qty Price Amount 
        //Required_Shipping_Date Delivery_Point
        public string PO_NO { get; set; }
        public string ITEM { get; set; }
        public string Customer { get; set; }
        public string Buyer { get; set; }
        public string PO_Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Project { get; set; }
        public string Qty { get; set; }
        public string Price { get; set; }
        public string Required_Shipping_Date { get; set; }
        public string Delivery_Point { get; set; }
        public string Status { get; set; }
        public string Product_Qty { get; set; }
        public string Shipment_Qty { get; set; }
        public string UNShipment_Qty { get; set; }
        public string Remark { get; set; }
    }
}
