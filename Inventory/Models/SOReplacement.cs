using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("SOReplacement")]
    public class SOReplacement
    {
        [Key]
        public int SReplaceId { get; set; }
        public string InvNo { get; set; }
        public DateTime InvDate { get; set; }
        public string OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? ReplacementQty { get; set; }
        public string NewInvNo { get; set; }
        public DateTime NewInvDate { get; set; }
        public string ProductCode { get; set; }
        public Boolean IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int CompanyID { get; set; }
    }
}