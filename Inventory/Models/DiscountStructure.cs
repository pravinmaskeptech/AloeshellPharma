using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("DiscountStructure")]
    public class DiscountStructure
    {
        [key]
        public int DiscountStructureID { get; set; }
        public string ProductName { get; set; }
        public decimal? MRP { get; set; }
        public decimal? EcomDiscount { get; set; }
        public decimal? CustDiscount { get; set; }
        public int? MRPoints { get; set; }
        public int? DoctorRefCodePt { get; set; }
        public int? NutraRefCodePt { get; set; }
        
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
       
    }
}