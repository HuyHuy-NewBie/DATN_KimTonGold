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

        // Every newly created support session is owned by a branch.  Keeping the
        // value nullable lets the system retain legacy records while ensuring a
        // branch staff member can never be granted access to an unscoped record.
        public int? BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }

        // Anonymous visitors authenticate to a chat session with an opaque
        // capability kept in an HttpOnly cookie.  Only its SHA-256 hash is
        // persisted, so a database disclosure does not disclose the capability.
        [StringLength(64)]
        public string GuestAccessTokenHash { get; set; }

        public DateTime? GuestAccessTokenExpiresAt { get; set; }

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
