using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models
{
    public class MultiCategoryProductCatalogEntry
    {
        [Key]
        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
