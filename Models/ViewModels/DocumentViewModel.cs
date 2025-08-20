using System.ComponentModel.DataAnnotations;

namespace DocuSense.Models.ViewModels
{
    public class DocumentViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;

        [Display(Name = "File Type")]
        public string FileType { get; set; } = string.Empty;

        [Display(Name = "File Size")]
        public string FileSizeFormatted { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Uploaded")]
        public DateTime UploadedAt { get; set; }

        [Display(Name = "Processed")]
        public DateTime? ProcessedAt { get; set; }

        [Display(Name = "Project")]
        public string? ProjectName { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Category")]
        public string? DocumentCategory { get; set; }

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; }

        public string UploadedBy { get; set; } = string.Empty;

        public List<DocumentFieldViewModel> ExtractedFields { get; set; } = new List<DocumentFieldViewModel>();

        public bool CanEdit { get; set; } = false;

        public bool CanDelete { get; set; } = false;

        public bool CanDownload { get; set; } = false;
    }

    public class DocumentFieldViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Field Name")]
        public string FieldName { get; set; } = string.Empty;

        [Display(Name = "Value")]
        public string? FieldValue { get; set; }

        [Display(Name = "Type")]
        public string? FieldType { get; set; }

        [Display(Name = "Confidence")]
        public double? Confidence { get; set; }

        [Display(Name = "Page")]
        public int? PageNumber { get; set; }

        [Display(Name = "Verified")]
        public bool IsVerified { get; set; }

        public string? Notes { get; set; }
    }

    public class DocumentUploadViewModel
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile? File { get; set; }

        [Display(Name = "Project Name")]
        public string? ProjectName { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Category")]
        public string? DocumentCategory { get; set; }

        [Display(Name = "Make Public")]
        public bool IsPublic { get; set; } = false;
    }

    public class DocumentListViewModel
    {
        public List<DocumentViewModel> Documents { get; set; } = new List<DocumentViewModel>();

        public int TotalCount { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public string? SearchTerm { get; set; }

        public string? StatusFilter { get; set; }

        public string? CategoryFilter { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
} 