using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("tbl_DoctorMaster")]
    public class DoctorMasterModel
    {
        [Key]
        public int DoctorID { get; set; }
        public string FirmName { get; set; }
        public string DoctorName { get; set; }
        public string DoctorCode { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int? Pincode { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }

        public string SalesPersonName { get; set; }   
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool? IsActive { get; set; }
        public string Type { get; set; }    
        public string DoctorDropdownRegister { get; set; }
        public string RegisterUnder { get; set; }
    }
}