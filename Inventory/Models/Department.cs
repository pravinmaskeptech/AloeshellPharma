using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("Department")]
    public partial class Department
    {
        [Key]
        public int ID { get; set; }

        public string DeptName { get; set; } 
    }
}