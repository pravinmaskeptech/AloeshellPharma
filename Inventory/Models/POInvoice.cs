using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("POInvoice")]
    public partial class POInvoice
    {
        [Key]
        [StringLength(20)]
        public string DocunentNo { get; set; }
        public DateTime? DocumentDate { get; set; }

        [StringLength(20)]
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }

        [StringLength(20)]
        public string PoNo { get; set; }
        public int CompanyID { get; set; }
        public DateTime? PoDate { get; set; }
        public decimal? BasicAmount { get; set; }
        public decimal? DiscAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TransportAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? SupplierId { get; set; }
        public decimal? PayAmount { get; set; }
        public string Status { get; set; }
        public decimal? BalanceAmount { get; set; }

        public DateTime? PaymentDate { get; set; }
        public string PaymentMode { get; set; }
        public string ReferenceNo { get; set; }
        public string Reason { get; set; }
    }
}