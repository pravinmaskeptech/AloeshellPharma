using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{

    [Table("PRNMain")]
    public class PRNMain
    {
        [Key]
        public int PRNID { get; set; }
        public string PRNNo { get; set; } 
        public string RaisedBy { get; set; } 
        public DateTime RaisedDate { get; set; } 
        public DateTime RequiredDate { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }         
        public string CreatedBy { get; set; }

        public string DisapproveReason { get; set; } 
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; } 
        public DateTime? UpdatedDate { get; set; }


        [NotMapped]    
        public string ProductName { get; set; }

        [NotMapped]
        public int? ID { get; set; } 

        [NotMapped]
        public string ProductCode { get; set; }

        [NotMapped]
        public int Quantity { get; set; }

        public Boolean? ShortListed { get; set; } 
    }
}