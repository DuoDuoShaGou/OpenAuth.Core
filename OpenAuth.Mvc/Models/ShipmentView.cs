using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.Mvc.Models
{
    public class ShipmentView
    {
        public string ID { get; set; }
        public string SIDR_NO { get; set; }
        public string PO_NO { get; set; }
        public string Customer { get; set; }
        public string Name { get; set; }
        public string ITEM { get; set; }
        public string PO_ITEM { get; set; }
        public string UNShipment_Qty { get; set; }
        public string Qty { get; set; }
        public string BILL_DATE { get; set; }
        public string REMARK { get; set; }
        public string MATERIAL { get; set; }
        public string CONTAINER_NO { get; set; }
        public string Seal_NO { get; set; }
        public string BL_NO { get; set; }
        public string VESSEL { get; set; }
        public string ETD { get; set; }
        public string ETA { get; set; }
        public string TERMS_OF_SALE { get; set; }
        public string COUNTRY_OF_ORIGIN { get; set; }
        public string SHIPMENT_FROM { get; set; }
        public string TAX { get; set; }
        public string FREIGHT { get; set; }

    }
}
