using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("ExplodedBOM")]
    public partial class ExplodedBOM
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Product Name")]
        public string ProductId { get; set; }

        [StringLength(20)]
        [Display(Name = "Component Name")]
        public string ComponentId { get; set; }

        [StringLength(20)]
        [Display(Name = "Finish Goods")]
        public string FinishGoods { get; set; }        

        [Display(Name = "Quantity")]
        public decimal? Quantity { get; set; }

    }
}