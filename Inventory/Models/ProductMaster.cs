using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models
{
    [Table("ProductMaster")]
    public class ProductMaster
    {
        [Key]
        public int ProductID { get; set; }
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(250)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [StringLength(500)]
        [Display(Name = "HSNCode")]
        public string HSNCode { get; set; }

        public decimal GSTPer { get; set; }
        public bool? IsActive { get; set; }

    }
}