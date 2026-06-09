using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileApi.Models;

public class Order
{
    [Key]
    public int Id { get; set; }

    public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    [Required]
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    [StringLength(100)]
    public string? CustomerName { get; set; }

    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = OrderStatusOptions.Completed;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

public class OrderDetail
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}
