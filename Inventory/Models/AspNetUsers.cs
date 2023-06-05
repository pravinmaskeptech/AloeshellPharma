using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

namespace Inventory.Models
{
    [Table("AspNetUsers")]
    public class AspNetUsers
    {
        [Key]
        public string Id { get; set; }

        public string HOD { get; set; }

        public string Email { get; set; }

        public bool? EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string  SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }

        public bool? PhoneNumberConfirmed { get; set;}

        public bool? TwoFactorEnabled { get; set;}

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool? LockoutEnabled { get; set; }

        public int? AccessFailedCount { get; set; }

        public string UserName { get; set; }

        public int? CompanyID { get; set; }

    }
}