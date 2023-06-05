using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("TempSerialNo")]
    public partial class TempSerialNo
    {
        [Key]
        public int SerialNoId { get; set; }
        public decimal? ReceivedQty { get; set; }
        public int PODetailsId { get; set; }
        public string ProductCode { get; set; }
        public string BatchNo { get; set; }
        public string WarehouseId { get; set; }
        public int StoreLocationId { get; set; }
        public string GrnNo { get; set; }
        public string PONO { get; set; }
        public DateTime GrnDate { get; set; }
        public string SerialNo { get; set; }

        public string  Status { get; set; }

        //public string SerialFrom { get; set; }
        //public string SerialTo { get; set; }
        //public int? EndCustomerID { get; set; }
    }
}