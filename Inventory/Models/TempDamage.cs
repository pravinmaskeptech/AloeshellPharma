using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("TempDamage")]
    public class TempDamage
    {
        [Key]
        public int TempId { get; set; }
        public string DamageNo { get; set; }
        public int? GRNId { get; set; }
        public int? PODetailsId { get; set; }
        public string ProductCode { get; set; }
        public string WarehouseId { get; set; }
        public int? StoreLocationId { get; set; }
        public string BatchNo { get; set; }
        public decimal? DamageQty { get; set; }
        public int? SupplierId { get; set; }
    }
}