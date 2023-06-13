using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial class OrderDetails
    {
        [Key]
        public int? OrderDetailsID { get; set; }
        public int? OrderID { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [StringLength(20)]
        [Display(Name = "Product")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "HSN Code is required")]
        [StringLength(10)]
        public string HSNCode { get; set; }
        public int CompanyID { get; set; }

        [Display(Name = "Category")]
        public int? CategoryID { get; set; }
        
        public int? ModelID { get; set; }

        [Required(ErrorMessage = "GST Percentage is required")]
        public decimal? GSTPercentage { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Order Qty is required")]
        public decimal? OrderQty { get; set; }

        [Required(ErrorMessage = "Net Amount is required")]
        public decimal? NetAmount { get; set; }

        [Required(ErrorMessage = "CGST Amount is required")]
        public decimal? CGSTAmount { get; set; }

        [Required(ErrorMessage = "SGST Amount is required")]
        public decimal? SGSTAmount { get; set; }

        [Required(ErrorMessage = "IGST Amount is required")]
        public decimal? IGSTAmount { get; set; }

        [Required(ErrorMessage = "Discount Amount is required")]
        public decimal? Discount { get; set; }

        [Required(ErrorMessage = "Total Amount is required")]
        public decimal? TotalAmount { get; set; }

       
        public decimal? DeliveredQty { get; set; }
        [StringLength(20)]
        public string OrderNo { get; set; }
        [StringLength(15)]
        public string DiscountAs { get; set; }
      
        public decimal? ReturnQty { get; set; }

        public decimal? DiscountAmount { get; set; }
        [StringLength(50)]
        public string  ReturnReason { get; set; }
        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }
        public DateTime? UpdationDate { get; set; }

        public int? CustomerId { get; set; }
        
        [StringLength(20)]
        public string UOM { get; set; }
        public bool BarcodeApplicable { get; set; }
        public bool BomApplicable { get; set; }

        public Int64? tempSRNO { get; set; }

        public int? GRN_ID { get; set; }  

    }
}