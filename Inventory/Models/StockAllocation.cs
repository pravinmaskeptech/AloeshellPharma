using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("StockAllocation")]
    public class StockAllocation
    {
        [Key]
        public int ID { get; set; }
        public string SONO { get; set; }
        public int OrderDetailsID { get; set; }
        public string Item { get; set; }
        public decimal Quantity { get; set; }
        public decimal Shortage { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Boolean? IsActive { get; set; }
    }
}