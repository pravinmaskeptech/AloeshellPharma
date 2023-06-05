using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("POReplacement")]
    public class POReplacement
    {
        [Key]
        public int PReplaceId { get; set; }

        [Required]
        [Display(Name = "GRN NO")]
        [StringLength(20)]
        public string GRNNo { get; set; }
        public int CompanyID { get; set; }

        [Required]
        [Display(Name = "GRN Date")]
        public DateTime GRNDate { get; set; }

        [Required]
        [Display(Name = "PO NO")]
        [StringLength(20)]
        public string PONO { get; set; }
        
        [Display(Name = "PO Date")]
        public DateTime? PODate { get; set; }

        [Required]
        [Display(Name = "Replacement Qty")]
        public decimal? ReplacementQty { get; set; }

        [Required]
        [Display(Name = "Replaced Qty")]
        public decimal? ReplacedQty { get; set; }

        [Required]
        [Display(Name = "New GRN No")]
        [StringLength(20)]
        public string NewGRNNo { get; set; }

        [Required]
        [Display(Name = "New GRN Date")]
        public DateTime NewGRNDate { get; set; }

        [Required]
        public string ProductCode { get; set; }

        [Display(Name = "Active")]
        public Boolean? IsActive { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}