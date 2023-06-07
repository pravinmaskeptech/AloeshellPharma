using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial class Products
    {
        [Key]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(250)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [StringLength(100)]
        [Display(Name = "Model Name")]
        public string ModelName { get; set; }
       
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        
        [Display(Name = "Opening Qty")]
        public decimal? OpeningQuantity { get; set; }
       
        [Display(Name = "Inward Qty")]
        public decimal? InwardQuantity { get; set; }
       
        [Display(Name = "Outward Qty")]
        public decimal? OutwardQuantity { get; set; }
                  
        [Display(Name = "Closing Qty")]
        public decimal? ClosingQuantity { get; set; }

        [Display(Name = "Allocated Qty")]
        public decimal? AllocatedQty { get; set; }

        [Display(Name = "Selling Price")]
        public decimal? SellingPrice { get; set; }
        public decimal? PurchasePrice { get; set; }

      
        [Display(Name = "Warranty")]
        public decimal? StandardWarranty { get; set; }
   
        [Display(Name = "Weight")]
        public decimal? Weight { get; set; }

      
        [StringLength(50)]
        [Display(Name = "Size")]
        public string Size { get; set; }
    
        [Display(Name = "Discount")]
        public decimal? Discount { get; set; }

       
        [Display(Name = "Discount In")]
        [StringLength(15)]
        public string DiscountIn { get; set; }
   
        [Display(Name = "Reorder Level")]
        public int? ReorderLevel { get; set; }

     
        [Display(Name = "Max Level")]
        public int? MaxLevel { get; set; }


        [Display(Name = "Hsn Code")]
        public string HsnCode { get; set; }

       
        [StringLength(200)]
        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Lead Time Purchase")]
        public int? LeadTimePurchase { get; set; }

        [Display(Name = "Lead Time Sell")]
        public int? LeadTimeSell { get; set; }

        [Display(Name = "UOM")]
        [StringLength(20)]
        public string UOM { get; set; }
               
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? SerialNoApplicable { get; set; }
        public decimal? DamageQty { get; set; }
        [Display(Name = "Product Type")]       
        [StringLength(50)]
        public string ProductType { get; set; }

        public string ProductClass { get; set; }

        public int? IssuedToProductionQty { get; set; } 

        public int? ManufactureGRNQty { get; set; }
        public string Prefix { get; set; }
        public int? CurrentSerialNo { get; set; }
        public decimal? GSTPer { get; set; }
        public decimal? MRP { get; set; }

        public int? SupplierId { get; set; }

        public string MFR { get; set; }


        [NotMapped]
        public string str_Supplier { get; set; }

        [NotMapped]
        public string str_category { get; set; }
    }
}