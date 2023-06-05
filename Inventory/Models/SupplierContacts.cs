using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace Inventory.Models
{
    [Table("SupplierContacts")]
    public class SupplierContacts
    {
        [Key]
        public int ContactPersonID { get; set; }

        [Display(Name = "Customer Name")]
        public int CustomerID { get; set; }

        [NotMapped]
        public string Customer { get; set; }

        [Display(Name = "Contact Person")]
        [MaxLength(100)]
        public string ContactPerson { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(15)]
        public string Mobile { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}