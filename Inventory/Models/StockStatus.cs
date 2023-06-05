using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("StockStatus")]
    public class StockStatus
    {
        [Key]
        public int ID { get; set; }

        public string ProductionOrderID { get; set; }

        public string Product { get; set; }

        public decimal? Qty { get; set; }
        public decimal? OrderQty { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}