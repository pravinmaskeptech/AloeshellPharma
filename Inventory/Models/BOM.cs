using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{

    [Table("BOM")]
    public partial class BOM
    {
       
        [Key]
        public int BomId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(20)]
        [Display(Name = "Product Name")]
        public string ProductId { get; set; }

        [Required(ErrorMessage = "Component Name is required")]
        [StringLength(20)]
        [Display(Name = "Component Name")]
        public string ComponentId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]       
        [Display(Name = "Quantity")]
        public decimal? Quantity { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool? IsActive { get; set; }

        public bool? hasSubComponent { get; set; }
        
        [NotMapped]
        public int BOMNo { get; set; } 
    }
}