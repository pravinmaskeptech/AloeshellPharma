using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial  class DiscountTypes
    {
        [Key]
        public int DiscountId { get; set; }

        [Required(ErrorMessage = "Discount Type is required")]
        [StringLength(100)]
        [Display(Name = "Discount Type")]
        public string DiscountType { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsActive { get; set; }
    }
}