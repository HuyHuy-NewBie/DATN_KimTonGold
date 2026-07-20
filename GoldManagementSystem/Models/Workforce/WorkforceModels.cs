using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoldManagementSystem.Models
{
    public class WorkShift
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; }
        public DateTime ShiftDate { get; set; }
        [MaxLength(20)] public string ShiftType { get; set; } = "Morning";
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        [MaxLength(1000)] public string ManagerNote { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ShiftAssignment> Assignments { get; set; } = new List<ShiftAssignment>();
    }

    public class ShiftAssignment
    {
        public int Id { get; set; }
        public int WorkShiftId { get; set; }
        public WorkShift WorkShift { get; set; }
        [MaxLength(450)] public string UserId { get; set; }
        public AppUser User { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        [MaxLength(30)] public string AttendanceStatus { get; set; } = "Scheduled";
        [MaxLength(1000)] public string SystemNote { get; set; } = string.Empty;
        [MaxLength(1000)] public string ManagerNote { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ShiftChangeLog
    {
        public int Id { get; set; }
        public int WorkShiftId { get; set; }
        [MaxLength(450)] public string ChangedByUserId { get; set; }
        [MaxLength(1000)] public string Details { get; set; }
        [MaxLength(30)] public string ChangeType { get; set; } = "Supplemental";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserFeaturePermission
    {
        public int Id { get; set; }
        [MaxLength(450)] public string UserId { get; set; }
        [MaxLength(100)] public string FeatureKey { get; set; }
        public int? BranchId { get; set; }
        public Branch Branch { get; set; }
        public bool IsGranted { get; set; }
        [MaxLength(450)] public string GrantedByUserId { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class EmployeeManagementNote
    {
        public int Id { get; set; }
        [MaxLength(450)] public string UserId { get; set; }
        public AppUser User { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; }
        [MaxLength(2000)] public string SystemNote { get; set; } = string.Empty;
        [MaxLength(2000)] public string ManagerNote { get; set; } = string.Empty;
        [MaxLength(450)] public string UpdatedByUserId { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
