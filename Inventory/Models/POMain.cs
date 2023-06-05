using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("POMains")]
    public partial  class POMain
    {
        [Key]      
        public int PurchaseOrderID { get; set; }

       [Display(Name = "Supplier")]
        public int? SupplierID { get; set; }

        [StringLength(50)]
        public string ShipVia { get; set; }
        [StringLength(500)]
        public string ShippingTerms { get; set; }

        public decimal? PayAmount { get; set; }

        [Display(Name = "Supplier")]
        [NotMapped]
        public string Supplier { get; set; }

        [Required(ErrorMessage = "Purchase Order No is required")]
        [StringLength(20)]
        [Display(Name = "Purchase Order No")]
       public string PurchaseOrderNo { get; set; }

      
        [Display(Name = "Order Date")]
        public DateTime? PurchaseOrderDate { get; set; }

         
        [Display(Name = "Expected Delivery Date")]
        public DateTime? ExpectedDeliveryDate { get; set; }
      
        [StringLength(50)]
        [Display(Name = "Deliver To")]
        public string DeliverTo { get; set; }
                    
        [Display(Name = "Customer")]
        public int? CustomerID { get; set; }
        

        [Required(ErrorMessage = "Net Amount is required")]      
        [Display(Name = "Net Amount")]
        public decimal? NetAmount { get; set; }


        [Required(ErrorMessage = "IGST is required")]
        [Display(Name = "IGST")]
        public decimal? IGST { get; set; }

        [Required(ErrorMessage = "SGST is required")]
        [Display(Name = "SGST")]
        public decimal? SGST { get; set; }

        [Required(ErrorMessage = "CGST is required")]
        [Display(Name = "CGST")]
        public decimal? CGST { get; set; }


        [Required(ErrorMessage = "Total Amount is required")]
        [Display(Name = "Total Amount")]
        public decimal? TotalAmount { get; set; }

       public int? CompanyID { get; set; }

        
        [Display(Name = "PO Status")]
        public string POStatus { get; set; }

        [StringLength(500)]
        public string DisapproveReason { get; set; }
              
        [StringLength(500)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
       
        [StringLength(500)]
        [Display(Name = "TermsAndConditions")]
        public string TermsAndConditions { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

         [Display(Name = "Freeze")]
        public bool? freeze { get; set; }
        [NotMapped]
        public string ProductCode { get; set; }

        [NotMapped]
        public decimal? GSTPercentage { get; set; }

        [NotMapped]
        public decimal? Price { get; set; }

        [NotMapped]
        public decimal? DiscountAmount { get; set; }

        [NotMapped]
        public decimal? OrderQty { get; set; }

       
        public decimal? Discount { get; set; }

        [NotMapped]
        public string HSNCode { get; set; }

        [NotMapped]
        public decimal? CGSTAmount { get; set; }

        [NotMapped]
        public decimal? SGSTAmount { get; set; }

        [NotMapped]
        public decimal? IGSTAmount { get; set; }

        [NotMapped]
        public decimal? AmountNew { get; set; }


        [NotMapped]
        public decimal? TotAmount { get; set; }

        [NotMapped]
        public int? PurchaseOrderDetailsID { get; set; }

        [NotMapped]
        public string DiscountAs { get; set; }
       
        [NotMapped]
        public bool? IsActive { get; set; }

        [NotMapped]
        public decimal? DiscountPercent { get; set; }

        [Display(Name = "Select Warehouse")]
        [StringLength(20)]
        public string WarehouseId { get; set; }

        public bool BarcodeApplicable { get; set; }

        public string MRNNO { get; set; }

        public string SalesOrderNo { get; set; }

        [NotMapped]
        public string PRNNO { get; set; }


    }
}