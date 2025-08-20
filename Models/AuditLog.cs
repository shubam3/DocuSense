using System.ComponentModel.DataAnnotations;

namespace DocuSense.Models
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [StringLength(450)]
        public string? EntityId { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(100)]
        public string? UserEmail { get; set; }

        [StringLength(100)]
        public string? UserRole { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? Status { get; set; }

        [StringLength(1000)]
        public string? Details { get; set; }

        [StringLength(100)]
        public string? Severity { get; set; } = "Info";

        public bool IsAnomaly { get; set; } = false;

        [StringLength(500)]
        public string? AnomalyReason { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Document? Document { get; set; }
    }

    public enum AuditSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public enum AuditStatus
    {
        Success,
        Failed,
        Pending,
        Cancelled
    }
} 