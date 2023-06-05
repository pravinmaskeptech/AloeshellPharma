using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class TaxMaster
    {
        [Key]
        [Required(ErrorMessage = "HSN Code is required")]
        [StringLength(10)]
        [Display(Name = "HSN Code")]
        public string HSNCode { get; set; }

        [Required(ErrorMessage = "Tax Percentage is required")]       
        [Display(Name = "Tax Percent")]
        public decimal? TaxPercent { get; set; }

        [Display(Name = "Active")]
        public bool? IsActive { get; set; }

        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}