using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("PurchaseOrderPayment")]
    public partial class PurchaseOrderPayment
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Doc No is required")]
        [StringLength(100)]
        [Display(Name = "Doc No")]
        public string DocNo { get; set; }       
        public DateTime? DocDate { get; set; }
        public int CompanyID { get; set; }

        public int? SupplierID { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public Decimal? Amount { get; set; }
        public string PaymentType { get; set; }

        public string PaymentMode { get; set; }

        public string ReferenceNo { get; set; }
        public DateTime? Date { get; set; }
        public string Remarks { get; set; }
        public decimal? InvoiceAmount { get; set; }
        [NotMapped]
        public decimal PayAmount { get; set; }

        public decimal? AdvanceAmount { get; set; }
        public string PoNo { get; set; }
        [NotMapped]
        public decimal? balanceAmount { get; set; }

        [NotMapped]
        public decimal? hfAdvanceamt { get; set; }


    }
}