using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    [Table("Destination_details")]
    public class Destination_details
    {

        public string name { get; set; }
        public string phone { get; set; }
        public string alternate_phone { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string pincode { get; set; }
        public string city { get; set; }
        public string state { get; set; }
    }
}