using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class CreditNote
    {
        [Key]
        public int ID { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public int? CustomerID { get; set; }

        public string CustomerName { get; set; }  
        public string InvoiceNo { get; set; }        
        public decimal Amount { get; set; }
        public int CompanyID { get; set; }
    }
}