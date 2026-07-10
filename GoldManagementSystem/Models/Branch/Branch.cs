using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string BranchName { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<AppUser> AppUsers { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Warehouse> Warehouses { get; set; }
            = new List<Warehouse>();
    }
}
