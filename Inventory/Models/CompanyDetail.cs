namespace Inventory.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CompanyDetail
    {
        [Key]
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(150)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
       
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [StringLength(10)]
        [Display(Name = "Pincode")]
        public string Pincode { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(50)]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Phone No is required")]
        [StringLength(15)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Fax No is required")]
        [StringLength(50)]
        [Display(Name = "Fax")]
        public string Fax { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100)]
             
        [Display(Name = "Email")]
        [RegularExpression("^[^@\\s]+@([^@\\s]+\\.)+[^@\\s]+$", ErrorMessage = "Enter valid Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tan No is required")]
        [StringLength(20)]
        [Display(Name = "Tan No")]
        public string TanNo { get; set; }

        [Required(ErrorMessage = "Pan No is required")]
        [StringLength(20)]
        [RegularExpression("^([a-zA-Z]){5}([0-9]){4}([a-zA-Z]){1}?$", ErrorMessage = "Enter valid Pan no")]
        [Display(Name = "Pan No")]
        public string PanNo { get; set; }

        [Required(ErrorMessage = "Website is required")]
        [StringLength(200)]
        [Display(Name = "Website")]
        public string Website { get; set; }

        [Required(ErrorMessage = "Industry is required")]
        [StringLength(50)]
        [Display(Name = "Industry")]
        public string Industry { get; set; }

        [Required(ErrorMessage = "Fiscal year is required")]
        [StringLength(50)]
        [Display(Name = "Fiscal year")]
        public string FiscalYearFrom { get; set; }

        [Required(ErrorMessage = "Time Zone is required")]
        [StringLength(100)]
        [Display(Name = "Time Zone")]
        public string TimeZone { get; set; }

        [Required(ErrorMessage = "GST No is required")]
        [StringLength(20)]
        [Display(Name = "GST No")]
        [RegularExpression("^([0][1-9]|[1-2][0-9]|[3][0-5])([a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}[1-9a-zA-Z]{1}[zZ]{1}[0-9a-zA-Z]{1})+$", ErrorMessage = "Enter valid GST no")]
        public string GSTNo { get; set; }

      
        [StringLength(100)]
        [Display(Name = "Logo Path")]
        public string LogoPath { get; set; }

        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsActive { get; set; }
    }
}
