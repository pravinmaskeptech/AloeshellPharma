using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("TempTables")]
    public partial class TempTable
    {
         [Key]
        public int TempId { get; set; }       
        public int GRNId { get; set; }
        public int OrderDetailsID { get; set; }
        public int StoreLocationId { get; set; }
        public string WarehouseID { get; set; }
        public string ProductCode { get; set; }
        public string BatchNo { get; set; }
        public decimal? ReceivedQty { get; set; }
        public decimal? SalesQty { get; set; }

        public string SerialFrom { get; set; }
        public string SerialTo { get; set; }
        public string SelectSerialFrom { get; set; }
        public string SelectSerialTo { get; set; }
        public Int64? tempSRNO { get; set; }
    }
}