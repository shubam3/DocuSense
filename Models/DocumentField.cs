using System.ComponentModel.DataAnnotations;

namespace DocuSense.Models
{
    public class DocumentField
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        [StringLength(100)]
        public string FieldName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? FieldValue { get; set; }

        [StringLength(50)]
        public string? FieldType { get; set; }

        public double? Confidence { get; set; }

        [StringLength(100)]
        public string? BoundingBox { get; set; }

        public int? PageNumber { get; set; }

        public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? ExtractedBy { get; set; }

        public bool IsVerified { get; set; } = false;

        [StringLength(450)]
        public string? VerifiedBy { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation property
        public virtual Document Document { get; set; } = null!;
    }
} 