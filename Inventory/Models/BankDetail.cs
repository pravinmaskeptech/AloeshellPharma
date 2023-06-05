namespace Inventory.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BankDetail
    {
        [Key]
        public int BankID { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [Display(Name = "Company Name")]
        public int? CompanyID { get; set; }

        [Required(ErrorMessage = "Bank Name is required")]
        [StringLength(100)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        [StringLength(50)]
        public string Branch { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        [Display(Name = "Address")]
        public string BankAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        public string City { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [StringLength(10)]
        public string Pincode { get; set; }

        [Required(ErrorMessage = "Account Holder Name is required")]
        [StringLength(100)]
        [Display(Name = "Account Holder Name")]
        public string AccountHolderName { get; set; }

        [Required(ErrorMessage = "Account No is required")]
        [StringLength(20)]
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }

        [StringLength(20)]
        [Required(ErrorMessage = "IFSC Code is required")]
        [RegularExpression("^[A-Za-z]{4}[a-zA-Z0-9]{7}$", ErrorMessage = "Enter valid IFSC Code")]
        [Display(Name = "IFSC Code")]
        public string IFSCCode { get; set; }
        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsActive { get; set; }
    }
}
