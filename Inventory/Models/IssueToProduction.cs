using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("IssueToProduction")]
    public class IssueToProduction
    {
        [key]
        public int ID { get; set; }
        public string IssueToProductionNo { get; set; }
        public string SalesOderNo { get; set; }
        public string MRNNO { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
       
    }
}