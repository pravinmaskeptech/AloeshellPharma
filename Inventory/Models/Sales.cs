using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
  
    public partial class Sales
    {
        [Key]
        public int SalesId { get; set; }

        [Required(ErrorMessage = "Invoice No is required")]
        [Display(Name = "Invoice No")]
        [StringLength(20)]
        public string InvoiceNo { get; set; }

        [Required(ErrorMessage = "Invoice Date")]
        [Display(Name = "Invoice Date")]
        public DateTime? InvoiceDate { get; set; }
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "Order No is required")]
        [StringLength(20)]
        [Display(Name = "Order No")]
        public string OrderNo { get; set; }

        [Required(ErrorMessage = "Product Code is required")]
        [StringLength(20)]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }

        [Display(Name = "Delivered Qty")]
        public decimal? DeliveredQty { get; set; }

        [Display(Name = "Return Qty")]
        public decimal? ReturnQty { get; set; }

        [StringLength(200)]
        [Display(Name = "Return Reason")]
        public string ReturnReason { get; set; }
        [StringLength(500)]
        public string BatchNo { get; set; }
        public int? CustomerID { get; set; }
        public decimal? AmountPerUnit { get; set; }

        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal Discount { get; set; }
        public decimal BasicRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreditDocNo { get; set; }
        public string DiscountAs { get; set; }

        [StringLength(20)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(20)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
       
        public int OrderDetailsID { get; set; }

        [NotMapped]
        public int GRNId { get; set; }

        public int? GRN_ID { get; set; }  

        [NotMapped]
        public int StoreLocationId { get; set; }

        [NotMapped]
        public string WarehouseID { get; set; }     

        [NotMapped]
        public decimal? ReceivedQty { get; set; }

        [NotMapped]
        public decimal? SalesQty { get; set; }

        public bool? SerialNoApplicable { get; set; }

        public DateTime SODate { get; set; }
       
        public decimal? ReplaceQty { get; set; }
        [StringLength(200)]
        public string ReplaceReason { get; set; }

        public decimal PayAmount { get; set; }
        [StringLength(15)]
        public string Status { get; set; }

        [StringLength(50)]
        [Display(Name = "Select Serial From")]
        public string SelectSerialFrom { get; set; }
        [StringLength(50)]
        [Display(Name = "Select Serial To")]
        public string SelectSerialTo { get; set; }
    }
}