namespace DocuSense.DTOs
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string BlobUrl { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ProcessingType { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public string? ProcessingResult { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public string? DocumentCategory { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDeleted { get; set; }
        public List<DocumentFieldDto> ExtractedFields { get; set; } = new List<DocumentFieldDto>();
    }

    public class DocumentFieldDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; }
        public string? FieldType { get; set; }
        public double? Confidence { get; set; }
        public string? BoundingBox { get; set; }
        public int? PageNumber { get; set; }
        public DateTime ExtractedAt { get; set; }
        public string? ExtractedBy { get; set; }
        public bool IsVerified { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateDocumentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public string? DocumentCategory { get; set; }
        public bool IsPublic { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class UpdateDocumentDto
    {
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public string? DocumentCategory { get; set; }
        public bool IsPublic { get; set; }
    }

    public class DocumentProcessingResultDto
    {
        public Guid DocumentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string? ProcessingResult { get; set; }
        public string? ErrorMessage { get; set; }
        public List<DocumentFieldDto> ExtractedFields { get; set; } = new List<DocumentFieldDto>();
    }

    public class DocumentSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? CategoryFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeDeleted { get; set; } = false;
    }
} 