using DocuSense.Models.ViewModels;
using DocuSense.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocuSense.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? statusFilter, string? categoryFilter, 
            DateTime? dateFrom, DateTime? dateTo, int pageNumber = 1)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var searchDto = new DTOs.DocumentSearchDto
                {
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    CategoryFilter = categoryFilter,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    PageNumber = pageNumber,
                    PageSize = 10,
                    UserId = userId
                };

                var documents = await _documentService.GetDocumentsByUserAsync(userId, searchDto);
                var categories = await _documentService.GetDocumentCategoriesAsync(userId);

                var viewModel = new DocumentListViewModel
                {
                    Documents = documents.Select(d => new DocumentViewModel
                    {
                        Id = d.Id,
                        FileName = d.FileName,
                        FileType = d.FileType,
                        FileSizeFormatted = FormatFileSize(d.FileSize),
                        Status = d.Status,
                        UploadedAt = d.UploadedAt,
                        ProcessedAt = d.ProcessedAt,
                        ProjectName = d.ProjectName,
                        Description = d.Description,
                        DocumentCategory = d.DocumentCategory,
                        ErrorMessage = d.ErrorMessage,
                        RetryCount = d.RetryCount,
                        UploadedBy = d.UserId,
                        CanEdit = true,
                        CanDelete = true,
                        CanDownload = true
                    }).ToList(),
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    CategoryFilter = categoryFilter,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    PageNumber = pageNumber,
                    PageSize = 10
                };

                ViewBag.Categories = categories;
                ViewBag.Statuses = Enum.GetValues(typeof(Models.DocumentStatus))
                    .Cast<Models.DocumentStatus>()
                    .Select(s => s.ToString())
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading documents for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return View("Error");
            }
        }

        public IActionResult Upload()
        {
            return View(new DocumentUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(DocumentUploadViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.File == null || model.File.Length == 0)
                {
                    ModelState.AddModelError("File", "Please select a file to upload.");
                    return View(model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var createDto = new DTOs.CreateDocumentDto
                {
                    FileName = model.File.FileName,
                    FileType = Path.GetExtension(model.File.FileName),
                    FileSize = model.File.Length,
                    ProjectName = model.ProjectName,
                    Description = model.Description,
                    DocumentCategory = model.DocumentCategory,
                    IsPublic = model.IsPublic,
                    UserId = userId
                };

                using var stream = model.File.OpenReadStream();
                var document = await _documentService.CreateDocumentAsync(createDto, stream);

                TempData["SuccessMessage"] = $"Document '{model.File.FileName}' uploaded successfully.";
                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                ModelState.AddModelError("", "An error occurred while uploading the document. Please try again.");
                return View(model);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var document = await _documentService.GetDocumentByIdAsync(id, userId);
                if (document == null)
                {
                    return NotFound();
                }

                var viewModel = new DocumentViewModel
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    FileType = document.FileType,
                    FileSizeFormatted = FormatFileSize(document.FileSize),
                    Status = document.Status,
                    UploadedAt = document.UploadedAt,
                    ProcessedAt = document.ProcessedAt,
                    ProjectName = document.ProjectName,
                    Description = document.Description,
                    DocumentCategory = document.DocumentCategory,
                    ErrorMessage = document.ErrorMessage,
                    RetryCount = document.RetryCount,
                    UploadedBy = document.UserId,
                    ExtractedFields = document.ExtractedFields.Select(f => new DocumentFieldViewModel
                    {
                        Id = f.Id,
                        FieldName = f.FieldName,
                        FieldValue = f.FieldValue,
                        FieldType = f.FieldType,
                        Confidence = f.Confidence,
                        PageNumber = f.PageNumber,
                        IsVerified = f.IsVerified,
                        Notes = f.Notes
                    }).ToList(),
                    CanEdit = true,
                    CanDelete = true,
                    CanDownload = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading document details for document {DocumentId}", id);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var success = await _documentService.DeleteDocumentAsync(id, userId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Document deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete document.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId} for user {UserId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "An error occurred while deleting the document.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryProcessing(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var document = await _documentService.RetryProcessingAsync(id, userId);
                TempData["SuccessMessage"] = "Document processing retry initiated successfully.";

                return RedirectToAction(nameof(Details), new { id = document.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying document processing for document {DocumentId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrying document processing.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var document = await _documentService.GetDocumentByIdAsync(id, userId);
                if (document == null)
                {
                    return NotFound();
                }

                var stream = await _documentService.DownloadDocumentAsync(id, userId);
                return File(stream, "application/octet-stream", document.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {DocumentId} for user {UserId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "An error occurred while downloading the document.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
} 