using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("PRNDetails")]
    public partial class PRNDetails
    {
        [Key]
        public int ID { get; set; }
        public int PRNID { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public string PRNNo { get; set; } 
        public string CreatedBy { get; set; } 
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Boolean? ShortListed { get; set; }


        public int? SupplierProductID { get; set; } 
        [NotMapped]
        public string SupplierproductrelationID { get; set; }
        [NotMapped]
        public string SupplierName { get; set; }
        [NotMapped]
        public string ProductPrice { get; set; }
        [NotMapped]
        public string DeliveryInDays { get; set; }
       
    }
}