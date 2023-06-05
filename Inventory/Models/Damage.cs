using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("Damage")]
    public partial class Damage
    {
        [Key]
        public int DamageId { get; set; }
        public int? GRNId { get; set; }
        public int? PODetailsId { get; set; }

        public DateTime? DamageDate { get; set; }

        [StringLength(20)]
        public string GRNNo { get; set; }

        [StringLength(20)]
        public string PONO { get; set; }
        public String ProductCode { get; set; }

        [StringLength(20)]
        public string WarehouseId { get; set; }
        public int? StoreLocationId { get; set; }     
        public int? SupplierId { get; set; }

        [StringLength(20)]
        public string BatchNo { get; set; }
        public decimal? DamageQty { get; set; }

        public string DamageNo { get; set; }

        [StringLength(20)]
        public string DamageReason { get; set; }
        [NotMapped]
        public string SerialNo { get; set; }

        [NotMapped]
        public int? SerialNoId { get; set; }


    }
}