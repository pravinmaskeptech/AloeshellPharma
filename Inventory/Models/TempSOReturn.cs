using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("TempSOReturn")]
    public partial class TempSOReturn
    {
        [Key]
        public int tempId { get; set; }

        [StringLength(20)]
        public string InvoiceNo { get; set; }

        [StringLength(20)]
        public string WarehouseId { get; set; }

        [StringLength(20)]
        public string BatchNo { get; set; }

        [StringLength(20)]
        public string ProductCode { get; set; }

        public int? GrnId { get; set; }
        public int? OrderDetailsID { get; set; }

        public int? StoreLocationId { get; set; }
        public decimal? ReturnQty { get; set; }
        public decimal? ReceivedQty { get; set; }

    }
}