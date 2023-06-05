using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("ProductionOrder")]
    public class ProductionOrder
    {
        [Key]
        public string ProductionOrderID { get; set; }
        public string OrderNo { get; set; }
        public string MRNNO { get; set; }
        public DateTime Date { get; set; }

        public string Status { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        [Display(Name = "Request Date")]
        public DateTime? RequiredDate { get; set; }      
       
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [NotMapped]
        public List<StockStatus> StockStatus { get; set; }
    }
}