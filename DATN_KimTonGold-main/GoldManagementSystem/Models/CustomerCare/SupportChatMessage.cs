using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldManagementSystem.Models
{
    public class SupportChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int SupportChatSessionId { get; set; }

        [ForeignKey("SupportChatSessionId")]
        public virtual SupportChatSession SupportChatSession { get; set; }

        [StringLength(450)]
        public string SenderId { get; set; }

        [Required]
        [StringLength(100)]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SenderRole { get; set; } = "Customer"; // Customer, Staff, System

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
