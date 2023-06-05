using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("ReturnReason")]
    public class ReturnReason
    {
        [Key]        
        public int ReturnId { get; set; }
        public string Reason { get;set; }
        public string CreatedBy{get;set;}
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool? IsActive { get; set; }

    }
}