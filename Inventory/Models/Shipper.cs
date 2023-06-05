using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("Shipper")]
    public partial class Shipper
    {        
        [Key]
        public int ShipperId { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        
        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        
        [StringLength(50)]
        public string City { get; set; }

        
        [StringLength(10)]
        public string Pincode { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

          
        [StringLength(50)]
        [Display(Name = "State")]
        public string State { get; set; }

        
        [StringLength(50)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [StringLength(15)]
        
        [Display(Name = "Contact No")]
        public string ContactNo { get; set; }

        
        public string PlaceOfSupply { get; set; }

        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdationDate { get; set; }

        public bool? Freeze { get; set; }

        [NotMapped]
        public string Customer { get; set; }
    }
}