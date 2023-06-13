using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("OrderMains")]
    public partial class OrderMain
    {
        [Key]
        public int OrderID { get; set; }

        public int? CompanyID { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        [Display(Name = "Employee")]
        [StringLength(100)]
        public String EmployeeID { get; set; }

        [Required(ErrorMessage = "Customer  is required")]
        [Display(Name = "Bill To")]
        public int? CustomerID { get; set; }

        [Required(ErrorMessage = "Order No is required")]
        [StringLength(200)]
        [Display(Name = "Order No")]
        public string OrderNo { get; set; }

        [Required(ErrorMessage = "Order Date is required")]        
        public DateTime? OrderDate { get; set; }
                
        [StringLength(500)]
        [Display(Name = "Terms And Conditions")]
        public string TermsAndConditions { get; set; }

        
        [StringLength(300)]
        public string CurrentStatus { get; set; }

        [Required(ErrorMessage = "Deliver To is required")]
        [StringLength(100)]
        [Display(Name = "Ship To")]
        public string DeliverTo { get; set; }

        [Required(ErrorMessage = "Net Amount To is required")]
        [Display(Name = "Net Amount")]
        public decimal? NetAmount { get; set; }

        [Required(ErrorMessage = "Discount To is required")]
        [Display(Name = "Discount")]
        public decimal? Discount { get; set; }

        [Required(ErrorMessage = "IGST To is required")]
        [Display(Name = "IGST")]
        public decimal? IGST { get; set; }

        [Required(ErrorMessage = "SGST To is required")]
        [Display(Name = "SGST")]
        public decimal? SGST { get; set; }

        [Required(ErrorMessage = "CGST To is required")]
        [Display(Name = "CGST")]
        public decimal? CGST { get; set; }

        [Required(ErrorMessage = "Total Amount To is required")]
        [Display(Name = "Total Amount")]
        public decimal? TotalAmount { get; set; }

        [StringLength(500)]
        public string DisapproveReason { get; set; }
        
        [StringLength(500)]
        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(500)]
        public string UpdatedBy { get; set; }

        public DateTime? UpdationDate { get; set; }

        public bool? Freeze { get; set; }


        [NotMapped]
        public string HSNCode { get; set; }

        [NotMapped]
        public decimal? CGSTAmount { get; set; }

        [NotMapped]
        public decimal? SGSTAmount { get; set; }

        [NotMapped]
        public decimal? IGSTAmount { get; set; }

        [NotMapped]
        public decimal? AmountNew { get; set; }


        [NotMapped]
        public decimal? TotAmount { get; set; }

        [NotMapped]
        public string ProductCode { get; set; }

        [NotMapped]
        public decimal? OrderQty { get; set; }

        [NotMapped]
        public decimal? GSTPercentage { get; set; }

        [NotMapped]
        public decimal? Price { get; set; }

        [NotMapped]
        public decimal? discPer { get; set; }

        [NotMapped]
        public string discIn { get; set; }

        [NotMapped]
        public int? OrderDetailsID { get; set; }

        [NotMapped]
        public int? OrderMAinID { get; set; }

        [NotMapped]
        public bool? isActive { get; set; }

        [NotMapped]
        public int? OrderDtlID { get; set; }



        public bool BarcodeApplicable { get; set; }
        public decimal? PayAmount { get; set; }
        [StringLength(15)]
        public string Status { get; set; }




        public string CustomerName { get; set; }

        [Display(Name = "City")]
        public string CustomerCity { get; set; }

        [Display(Name = "Pincode")]
        public string CustomerPincode { get; set; }

        [Display(Name = "GST No")]
        public string CustomerGSTNo { get; set; }

        [Display(Name = "Mobile")]
        public string CustomerMobile { get; set; }

        [Display(Name = "Address")]
        public string CustomerAddress { get; set; }

        public Boolean? IsCashCustomer { get; set; }
        public string DoctorCode { get; set; }

        [NotMapped]
        public Int64? tempSRNO { get; set; }
        public string Transport { get; set; }

        public DateTime? DispatchDate { get; set; }
        public string Delivery { get; set; }
        public string VehicleNo { get; set; }
        public bool? IsDCSale { get; set; }
        public string ReferenceNumber { get; set; }
        public string CustRefNo { get; set; }

        public string BookingStatus { get; set; }

    }
}