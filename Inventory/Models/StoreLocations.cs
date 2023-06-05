using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial class StoreLocations
    {
        [Key]
        public int StoreLocationId { get; set; }
        [Required]
        [Display(Name = "Warehouse")]       
        public string WarehouseId { get; set; }

        [Required]
        [Display(Name = "Store Location")]
        [MaxLength(50)]
        public string StoreLocation { get; set; }

        [Required]
        [Display(Name = "Description")]
        [MaxLength(200)]
        public string  Description { get; set; }

        [MaxLength(20)]
        public string CreatedBy { get; set; }      
        public DateTime? CreatedDate { get; set; }

        [MaxLength(20)]
        public string  ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Active")]
        public bool? IsActive { get; set; }
    }
}
