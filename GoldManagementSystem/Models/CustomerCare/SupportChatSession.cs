using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class SupportChatSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SessionCode { get; set; } = string.Empty;

        [StringLength(450)]
        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual AppUser Customer { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(20)]
        public string CustomerPhone { get; set; }

        [StringLength(256)]
        public string CustomerEmail { get; set; }

        [StringLength(450)]
        public string AssignedStaffId { get; set; }

        [ForeignKey("AssignedStaffId")]
        public virtual AppUser AssignedStaff { get; set; }

        [StringLength(100)]
        public string AssignedStaffName { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Waiting"; // Waiting, Active, Closed

        [StringLength(1000)]
        public string LastMessage { get; set; } = string.Empty;

        public int UnreadByStaffCount { get; set; } = 0;
        public int UnreadByCustomerCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SupportChatMessage> Messages { get; set; } = new List<SupportChatMessage>();
    }
}
