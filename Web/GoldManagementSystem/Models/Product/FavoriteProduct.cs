using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class FavoriteProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
