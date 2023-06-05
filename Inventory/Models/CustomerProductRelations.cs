using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial class CustomerProductRelations
    {
        [Key]
        public int CustomerProductRelationId { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(20)]
        [Display(Name = "Product Name")]
        public string ProductCode { get; set; }


        [Display(Name = "Product Price")]
        public decimal? ProductPrice { get; set; }

      

        public decimal? Tax { get; set; }


        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsActive { get; set; }

        [NotMapped]
        public int? SupplierProductId { get; set; }
        
    
    }
}
