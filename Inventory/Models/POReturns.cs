using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial class POReturns
    {
        [Key]
        public int POreturnId { get; set; }

        [Required(ErrorMessage = "GRN NO is required")]
        [StringLength(20)]
        [Display(Name = "GrnNo")]
        public string GrnNo { get; set; }

        [StringLength(20)]
        public string Status { get; set; }
        public int CompanyID { get; set; }

        [StringLength(20)]
        public string POReturnNo { get; set; }
        public int? SupplierID { get; set; }
        public DateTime ReturnDate { get; set; }

        public string ProductCode { get; set; }

        public string BatchNo { get; set; }

        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "Return Qty is required")]
        [Display(Name = "ReturnQty")]
        public decimal ReturnQty { get; set; }

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

        [NotMapped]
        public int? GRNId { get; set; }

        [NotMapped]
        public int? PurchaseOrderDetailsID { get; set; }
        [NotMapped]
        public int SerialNoId { get; set; }

        [NotMapped]
        public string SerialNo { get; set; }

    }
}