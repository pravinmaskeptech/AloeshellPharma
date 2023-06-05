using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("BillNumbering")]
    public class BillNumbering
    {
        [Key]
        public string Type { get; set; }
        public int? Number { get; set; }
    }
}
