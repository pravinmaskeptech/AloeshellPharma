using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("ProductSerialNo")]
    public partial class ProductSerialNo
    {
        [Key]
        public int SerialNoId { get; set; }

        public int? PODetailsId { get; set; }
        public string ProductCode { get; set; }
        public string BatchNo { get; set; }
        public int? CompanyID { get; set; }
        public string  WarehouseId { get; set; }
        public int? StoreLocationId { get; set; }
        public string GrnNo { get; set; }
        public string PONO { get; set; }
        public DateTime GrnDate { get; set; }
        public string SerialNo { get; set; }
        //public decimal? ReceivedQty { get; set; }
        public string  InvoiceNo { get; set; }
        public string Status { get; set; }

        public Boolean? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        [NotMapped]
        public string ReturnReason { get; set; }
        [NotMapped]
        public decimal? ReturnQty { get; set; }

        [NotMapped]
        public int? SupplierID { get; set; }
        public int? Warrenty { get; set; }
        public DateTime? WarrentyDate { get; set; }
        public string SerialFrom { get; set; }
        public string SerialTo { get; set; }
        public int? EndCustomerID { get; set; }
        public Int64? tempSRNO { get; set; }
        public string DoctorCode { get; set; }
    }
}