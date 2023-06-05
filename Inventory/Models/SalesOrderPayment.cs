using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("SalesOrderPayment")]
    public partial class SalesOrderPayment
    {
       
        [Key]
        public int ID { get; set; }
        [StringLength(20)]
        public string DocNo { get; set; }
        public DateTime? DocDate  { get; set; }
        public int CustomerId { get; set; }
        [StringLength(20)]
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        [StringLength(50)]
        public string PaymentType { get; set; }
        [StringLength(50)]
        public string PaymentMode { get; set; }
        [StringLength(20)]
        public string ReferenceNo { get; set; }
        public DateTime? Date { get; set; }
        public string Remarks { get; set; }
        public decimal? InvoiceAmount { get; set; }
        [StringLength(20)]
        public string SoNo { get; set; }
        public decimal AdvanceAmount { get; set; }
        [NotMapped]
        public decimal? PayAmount { get; set; }              
    
        [NotMapped]
        public decimal balanceAmount { get; set; }

        [NotMapped]
        public decimal? hfAdvanceamt { get; set; }
               
     
    }
}