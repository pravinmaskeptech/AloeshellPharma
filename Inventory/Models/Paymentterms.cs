using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("PaymentTerms")]
    public class Paymentterms
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Payment Term")]
        [MaxLength(100)]
        [Required]
        public string PaymentTerm { get; set; }

        [Required]
        public int Days { get; set; } 

        [Display(Name = "Active")]
        public bool? IsActive { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}