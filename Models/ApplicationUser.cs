using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DocuSense.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? Company { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(50)]
        public string? Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int LoginAttempts { get; set; } = 0;

        public DateTime? LockoutEnd { get; set; }

        [StringLength(500)]
        public string? AzureAdObjectId { get; set; }

        [StringLength(100)]
        public string? TenantId { get; set; }

        // Navigation properties
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
} 