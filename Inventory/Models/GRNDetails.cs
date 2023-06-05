using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial class GRNDetails
    {
        [Key]
        public int GRNId { get; set; }
        [Required(ErrorMessage = "PO Details ID is required")]
        public int PurchaseOrderDetailsID  { get; set; }
       public string GRNNo { get; set; }

        [Display(Name = "Warehouse")]
        public string WarehouseID { get; set; }
        public int CompanyID { get; set; }
        public int? StoreLocationId { get; set; }

        [Required(ErrorMessage = "Grn Date")]       
        [Display(Name = "Grn Date")]
        public DateTime? GRNDate { get; set; }

        public int? SupplierID { get; set; }

        [Required(ErrorMessage = "PO No is required")]
        [StringLength(20)]
        [Display(Name = "PO No")]
        public string PONo { get; set; }

        [Required(ErrorMessage = "Product Code is required")]
        [StringLength(20)]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }
      
        [StringLength(20)]
        [Display(Name = "Batch No")]
        public string BatchNo { get; set; }
        
        [Display(Name = "Sales Qty")]
        public decimal? SalesQty { get; set; }

        [Display(Name = "ReceivedQty")]
        public decimal? ReceivedQty { get; set; }

        [Display(Name = "Return Qty")]
        public decimal? ReturnQty { get; set; }
              
        [StringLength(200)]
        [Display(Name = "Return Reason")]
        public string ReturnReason { get; set; }
        
        [Display(Name = "Manufacturing Date")]
        public DateTime? ManufacturingDate { get; set; }
       
        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

        public decimal? ReplaceQty { get; set; }
        [StringLength(20)]
        public string ReplaceReason { get; set; }

        [StringLength(20)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(20)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool? SerialNoApplicable { get; set; }

        [NotMapped]
        public decimal? PurchasePrice { get; set; }

        public decimal? DamageQty { get; set; }

        public decimal? AmountPerItem { get; set; }

        public DateTime? PODate { get; set; }
        public string DebitDocNo { get; set; }

        public decimal? BasicRate { get; set; }
        public decimal? Discount { get; set; }
        public string DiscountAs { get; set; }
        public decimal? CGST { get; set; }
        public decimal? IGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? Tax { get; set; }

        [NotMapped]
        public string MRNNO { get; set; }

        [StringLength(50)]
        [Display(Name = "Serial From")]
        public string SerialFrom { get; set; }
        [StringLength(50)]
        [Display(Name = "Serial To")]
        public string SerialTo { get; set; }      
        public DateTime? InvoiceDate { get; set; }
        public decimal? TransportAmount { get; set; }
        public string InvoiceNo { get; set; }
        public decimal? BasicAmount { get; set; }
        public decimal? DiscAmount { get; set; }
        public decimal? TAmount { get; set; }
        public decimal? TaxAmount { get; set; }

    }
}