using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models
{
    [Table("AspNetUserRoles")]
    public class AspNetUserRoles
    {
        [Key]
        public string UserId { get; set; }

        //[ForeignKey("UserId")]
        //public AspNetUsers User { get; set; }
      
        public string RoleId { get; set; }
    }
}