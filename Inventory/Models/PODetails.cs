using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial  class PODetails
    {
        [Key]
        public int PurchaseOrderDetailsID { get; set; }
             
        public int? PurchaseOrderID { get; set; }

        public decimal? DiscountAmount { get; set; }

        [StringLength(20)]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }
        public int CompanyID { get; set; }
             
        [StringLength(10)]
        [Display(Name = "HSN Code")]
        public string HSNCode { get; set; }
           
        [Display(Name = "GST Percentage")]
        public decimal? GSTPercentage { get; set; }        
        public int? CategoryID { get; set; }      
        public int? ModelID { get; set; }              
        public decimal? Price { get; set; }

        [StringLength(50)]
        public string Size { get; set; }
        public decimal? OrderQty { get; set; }
        public decimal? ReceivedQty { get; set; }        

        [StringLength(20)]
        public string PONO { get; set; }

        [StringLength(10)]  
        public string DiscountAs { get; set; }
        public decimal? Discount { get; set; }
        public decimal? ReturnQty { get; set; }            
        [StringLength(50)]
        [Display(Name = "Return Reason")]
        public string ReturnReason { get; set; }       
        public decimal? CGSTAmount { get; set; }
        public decimal? SGSTAmount { get; set; }
        public decimal? IGSTAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? NetAmount { get; set; }

       

        public bool? IsActive { get; set; }

        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdationDate { get; set; }
        public bool BarcodeApplicable { get; set; }
        public bool? IsProduction { get; set; }

        public string MRNNO { get; set; }
        public string SalesOrderNo { get; set; }

        public int? IssueToProductionQty { get; set; }

        public string PRNNO { get; set; } 
    }
}