using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
     [Table("Customers")]    
    public class Customer
    {       
        public int CustomerID { get; set; }

        [Display(Name="Customer Name")]
        [MaxLength(100)]
        public string CustomerName { get; set; }


        [Display(Name = "Address")]
        [MaxLength(500)]
        
        public string BillingAddress { get; set; }


        [Display(Name = "City")]
        [MaxLength(50)]
  
        public string BillingCity { get; set; }


        [Display(Name = "Pincode")]
        [MaxLength(10)]

        public string BillingPincode { get; set; }



        [Display(Name = "State")]
        [MaxLength(50)]

        public string BillingState { get; set; }



        [Display(Name = "Country")]
        [MaxLength(50)]
        public string BillingCountry { get; set; }



        [Display(Name = "Phone")]
        [MaxLength(15)]

        public string BillingPhone { get; set; }



        [Display(Name = "Email")]
        [MaxLength(100)]
        
        public string BillingEmail { get; set; }



        [Display(Name = "Address")]
        [MaxLength(500)]
        public string ShippingAddress { get; set; }


        [Display(Name = "City")]
        [MaxLength(50)]
        public string ShippingCity { get; set; }


        [Display(Name = "Pincode")]
        [MaxLength(10)]
        public string ShippingPincode { get; set; }


        [Display(Name = "State")]
        [MaxLength(50)]
        public string ShippingState { get; set; }


        [Display(Name = "Country")]
        [MaxLength(50)]
  
        public string ShippingCountry { get; set; }


        [Display(Name = "Phone")]
        [MaxLength(15)]
        public string ShippingPhone { get; set; }


        [Display(Name = "Email")]
        [MaxLength(100)]

        public string ShippingEmail { get; set; }


        [Display(Name = "Website")]
        [MaxLength(100)]
        public string Website { get; set; }


        [Display(Name = "PAN No")]
        [MaxLength(20)]
       
        public string PANNo { get; set; }


        [Display(Name = "TAN No")]
        [MaxLength(10)]
        public string TANNo { get; set; }

        [Display(Name = "GST No")]
        [MaxLength(20)]
        public string GSTNo { get; set; }



        [Display(Name = "GST Treatment")]
        [MaxLength(50)]
        public string GSTTreatment { get; set; }


        [Display(Name = "IGST")]
        public bool? IGST { get; set; }


        [Display(Name = "IsExempt")]
        public bool? IsExempt { get; set; }


        [Display(Name = "Exepmtion Reason")]
        [MaxLength(100)]
        public string ExepmtionReason { get; set; }


        [Display(Name = "Currency")]
        [MaxLength(20)]
        public string Currency { get; set; }


        [Display(Name = "Payment Terms")]
        [MaxLength(50)]
        public string PaymentTerms { get; set; }


        [Display(Name = "Balance Amount")]
        public decimal? BalanceAmount { get; set; }


        [Display(Name = "Advance Amounts")]
        public decimal? AdvanceAmount { get; set; }


        [Display(Name = "Employee")]
        public string EmployeeID { get; set; }


        [Display(Name = "Facebook")]
        [MaxLength(50)]
        public string Facebook { get; set; }


        [Display(Name = "Twitter")]
        [MaxLength(50)]
        public string Twitter { get; set; }

        [Display(Name = "Whats App No")]
        [MaxLength(15)]
        public string WhatsApp { get; set; }


        [Display(Name = "Notes")]
        [MaxLength(500)]
        public string Notes { get; set; }


        [Display(Name = "Is Active")]
        public bool? IsActive { get; set; }


        [Display(Name = "Created By")]
        [MaxLength(30)]
        public string CreatedBy { get; set; }


        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Updated By")]
        [MaxLength(30)]
        public string UpdatedBy { get; set; }


        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }

        [Display(Name = "DrugLicenseNo")]
        public string DrugLicenseNo { get; set; }
    }
 }