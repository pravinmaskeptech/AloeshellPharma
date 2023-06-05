using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("Currency")]
    public class Currency
    {
        [Key]
        public int CurrencyID { get; set; }


        [Display(Name = "Currency")]
        [MaxLength(20)]
        [Required]
        public string CurrencyName { get; set; }

        [Display(Name = "Currency Symbol")]
        [MaxLength(2)]
        [Required]
        public string CurrencySymbol { get; set; }

        [Display(Name = "Active")]       
        public bool? IsActive { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}