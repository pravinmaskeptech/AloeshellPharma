namespace Inventory.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Employee
    {
        [Key]
        [StringLength(10)]
        [Display(Name = "Employee ID")]
        public string EmployeeID { get; set; }

        [Required(ErrorMessage = "Employee Name is required")]
        [StringLength(100)]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }

        [Required(ErrorMessage = "Birth Date is required")]
        [Display(Name = "Date Of Birth")]        
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [StringLength(6)]
        [Display(Name = "Pincode")]
        public string Pincode { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [StringLength(50)]
        [Display(Name = "Designation")]
        public string Designation { get; set; }

        
        [StringLength(15)]      
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [StringLength(15)]
        [Display(Name = "Mobile")]
        public string Mobile { get; set; }

     
        [StringLength(100)]
        [RegularExpression("^[^@\\s]+@([^@\\s]+\\.)+[^@\\s]+$", ErrorMessage = "Enter valid Email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

      
        [StringLength(10)]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [StringLength(10)]
        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

       
        [StringLength(200)]
        [Display(Name = "Picture Path")]
        public string PicturePath { get; set; }

        [Display(Name = "Active")]
        public bool? IsActive { get; set; }

        [StringLength(30)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(30)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
