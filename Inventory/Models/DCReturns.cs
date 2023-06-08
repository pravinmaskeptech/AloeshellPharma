using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("DCReturns")]
    public class DCReturns
    {
        [Key]
        public int DCreturnId { get; set; }

        [StringLength(20)]
        public string DCReturnNo { get; set; }
        public DateTime? ReturnDate { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public string ProductCode { get; set; }
      

        public string BatchNo { get; set; }

        public string SerialNumber { get; set; }

        public string ReplaceWithSRNo { get; set; } 

        [Required(ErrorMessage = "Return Qty is required")]
        [Display(Name = "ReturnQty")]
        public decimal ReturnQty { get; set; }

        public int? CustomerId { get; set; }
        public int CompanyID { get; set; }       
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Return Reason is required")]
        [StringLength(500)]
        [Display(Name = "Return Reason")]
        public string ReturnReason { get; set; }
        [Display(Name = "Warehouse")]
        public string WarehouseID { get; set; }

      

        public int? StoreLocationId { get; set; }
        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsActive { get; set; }
       
        public string InvoiceNo { get; set; }

        [NotMapped]
        public int? OrderDetailsID { get; set; }

        [NotMapped]
        public int? SalesId { get; set; }

        [NotMapped]
        public decimal? ReceivedQty { get; set; }

        [NotMapped]
        public int? GrnId { get; set; }

        [StringLength(20)]
        public string OrderNo { get; set; }
        [NotMapped]
        public int? SerialNoId { get; set; }
        [NotMapped]
        public string Returnstatus { get; set; }
        public string Status { get; set; }
    }
}