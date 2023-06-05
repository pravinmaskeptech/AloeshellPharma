using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public partial class Warehouse
    {
        [Key]
        [StringLength(10)]
        public string WareHouseID { get; set; }

        [Required(ErrorMessage = "WareHouse Name is required")]
        [StringLength(100)]
        [Display(Name = "WareHouse Name")]
        public string WareHouseName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [StringLength(10)]
        public string Pincode { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^[^@\\s]+@([^@\\s]+\\.)+[^@\\s]+$", ErrorMessage = "Enter valid Email")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone No is required")]
        [StringLength(15)]
        [Display(Name = "Phone No")]
        public string Phone { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "State is required")]
        [Display(Name = "State")]
        public string State { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public string Country { get; set; }


        [Required(ErrorMessage = "Is Primary is required")]
        [Display(Name = "Primary")]
        public string IsPrimary { get; set; }
       
    }
}