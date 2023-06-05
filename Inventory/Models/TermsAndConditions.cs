using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial class TermsAndConditions
    {
        [Key]
        public int TermsId { get; set; }

        [Required(ErrorMessage = "Orders is required")]
        [Display(Name = "Order")]
        [StringLength(50)]
        public string  Orders { get; set; }

        [Required(ErrorMessage = "Terms And Condition is required")]
        [StringLength(500)]
        [Display(Name = "Terms And Condition")]
        public string TermsAndCondition { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool? IsActive { get; set; }
    }
}