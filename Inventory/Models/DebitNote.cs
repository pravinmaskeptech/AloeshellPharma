using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("DebitNote")]
    public class DebitNote
    {
        [Key]
        public int DebitNoteId { get; set; }

        public string DocNo { get; set; }

        public DateTime DocDate { get; set; }

        public int? SupplierId { get; set; }
       
        [Display(Name="PO NO")]
        public string PONO { get; set; }
        public int CompanyID { get; set; }

        [Display(Name=("PO Date"))]
        public DateTime? PODate { get; set; }
     
        [Display(Name = ("Return Items"))]
        public string ReturnItems { get; set; }
        
        [Display(Name = ("Value"))]
        public decimal Value { get; set; }

        [Display(Name = ("Active"))]
        public Boolean? IsActive { get; set; }

        [Display(Name = ("Created By"))]
        public string CreatedBy { get; set; }

        [Display(Name = ("Created Date"))]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = ("Updated By"))]
        public string UpdatedBy { get; set; }

        [Display(Name = ("Updated Date"))]
        public DateTime? UpdatedDate { get; set; }
    }
}