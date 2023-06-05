using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("IssueToProductionDetails")]
    public class IssueToProductionDetails
    {
        [key]
        public int ID { get; set; }

        public int IssueToProductionID { get; set; }
        
        public string Product { get; set; }
        public int ProductQty { get; set; }
        public string ProductComponent { get; set; }
        public int ComponentQty { get; set; }
        public int GRNQty { get; set; }
        public int IssuedQty { get; set; }
        public int IssueToProductionQty { get; set; }
        public string ProdCode { get; set; }
        public string MainProductCode { get; set; }
        public string IssueToProductionNo { get; set; }
        public string MRNNO { get; set; }
        public string SalesOderNo { get; set; }

        public int ManufactureGRNQty { get; set; } 
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}