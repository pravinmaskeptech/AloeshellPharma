using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("TempSalesSerialNo")]
    public partial class TempSalesSerialNo
    {
        [Key]
        public int TempReturnId { get; set; }
        public string SerialNo { get; set; }
        public string ProductCode { get; set; }
        public string InvoiceNo { get; set; }
        public string  WarehouseId { get; set; }
        public int? StoreLocationId { get; set; }
        public string Status { get; set; }       
         public int? PODetailsId { get; set; }     
        public string BatchNo { get; set; }
        public int? SerialNoId { get; set; }       
        public string GrnNo { get; set; }       
        public String PONO { get; set; }
        public DateTime? GrnDate { get; set; }
        public int? Warrenty { get; set; }
        public DateTime? WarrentyDate { get; set; }
    }
}