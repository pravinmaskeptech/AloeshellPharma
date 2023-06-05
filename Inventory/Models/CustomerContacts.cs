using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("CustomerContacts")]
    public class CustomerContacts
    {
        [Key]
        public int ContactPersonID { get; set; }

        [Display(Name="Customer Name")]
        [Required]
        public int CustomerID { get; set; }

        [NotMapped]       
        public string Customer { get; set; }

        [Display(Name="Contact Person")]
        [MaxLength(100)]
        [Required]
        public string ContactPerson { get; set; }

        [MaxLength(15)]
        [Required]
        public string Phone { get; set; }
        
        [MaxLength(15)]
        [Required]
        public string Mobile { get; set; }

        [MaxLength(100)]
        [Required]
        public string Email { get; set; }

        [Required]
        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}