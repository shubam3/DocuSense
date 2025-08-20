using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocuSense.Models
{
    public class Document
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        [StringLength(500)]
        public string BlobUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContainerName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BlobName { get; set; } = string.Empty;

        [Required]
        public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;

        [StringLength(50)]
        public string? ProcessingType { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public DateTime? LastModified { get; set; }

        [StringLength(1000)]
        public string? ProcessingResult { get; set; }

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; } = 0;

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ProjectName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? DocumentCategory { get; set; }

        public bool IsPublic { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<DocumentField> ExtractedFields { get; set; } = new List<DocumentField>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }

    public enum DocumentStatus
    {
        Uploaded,
        Processing,
        Processed,
        Failed,
        Retrying,
        Cancelled
    }
} 