using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models
{
    public class ProductCatalogAssignment
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        [StringLength(30)]
        public string ProductLine { get; set; } = ProductLineOptions.Gold;
    }
}
