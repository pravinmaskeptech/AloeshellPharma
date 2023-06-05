using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models
{
    [Table("SalesPersonMaster")]
    public class SalesPersonMaster
    {
        [Key]
        public int SalesPersonID { get; set; }

        public int? MMEID { get; set; }


        public string SalesPersonName { get; set; }


        public string SalesPersonCode { get; set; }


        public string Password { get; set; }


        public string Address { get; set; }
        public string City { get; set; }


        public int Pincode { get; set; }


        public string ContactNo { get; set; }

        public string Email { get; set; }
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}