using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("MRNDetails")]
    public class MRNDetails
    {
        [Key]
        public int ID { get; set; }
        public string ProductionOrderNo { get; set; }
        public string MRNNo { get; set; }
        public string OrderedProduct { get; set; }
        public decimal? OrderedQty { get; set; }
        public string RequiredItems { get; set; }
        public decimal? RequiredQty { get; set; }

        public bool PoGenerated { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int ? CurrentAvailableStock { get; set; } 
    }
}