using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("Consignments")]
    public class Consignments
    {

        public string customer_code { get; set; }
        public string service_type_id { get; set; }
        public string load_type { get; set; }
        public string description { get; set; }
        public string cod_favor_of { get; set; }
        public string cod_amount { get; set; }
        public string cod_collection_mode { get; set; }
        public string consignment_type { get; set; }
        public string dimension_unit { get; set; }
        public string length { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string weight_unit { get; set; }
        public string weight { get; set; }
        public string declared_value { get; set; }
        public string customer_reference_number { get; set; }
        public string commodity_id { get; set; }
        public virtual Origin_details origin_details { get; set; }
        public virtual Destination_details destination_details { get; set; }
    }
}