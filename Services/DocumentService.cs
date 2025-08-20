using DocuSense.Data;
using DocuSense.DTOs;
using DocuSense.Models;
using DocuSense.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DocuSense.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureBlobStorageService _blobStorageService;
        private readonly IAzureCognitiveServices _cognitiveServices;
        private readonly IAuditService _auditService;
        private readonly ILogger _logger;

        public DocumentService(
            ApplicationDbContext context,
            IAzureBlobStorageService blobStorageService,
            IAzureCognitiveServices cognitiveServices,
            IAuditService auditService,
            ILogger logger)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _cognitiveServices = cognitiveServices;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.ExtractedFields)
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == id && (d.UserId == userId || d.IsPublic));

                if (document == null)
                    return null;

                return MapToDto(document);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting document {DocumentId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsByUserAsync(string userId, DocumentSearchDto searchDto)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.ExtractedFields)
                    .Include(d => d.User)
                    .Where(d => d.UserId == userId && !d.IsDeleted);

                // Apply filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    query = query.Where(d => d.FileName.Contains(searchDto.SearchTerm) ||
                                           d.ProjectName.Contains(searchDto.SearchTerm) ||
                                           d.Description.Contains(searchDto.SearchTerm));
                }

                if (!string.IsNullOrEmpty(searchDto.StatusFilter))
                {
                    if (Enum.TryParse<DocumentStatus>(searchDto.StatusFilter, out var status))
                    {
                        query = query.Where(d => d.Status == status);
                    }
                }

                if (!string.IsNullOrEmpty(searchDto.CategoryFilter))
                {
                    query = query.Where(d => d.DocumentCategory == searchDto.CategoryFilter);
                }

                if (searchDto.DateFrom.HasValue)
                {
                    query = query.Where(d => d.UploadedAt >= searchDto.DateFrom.Value);
                }

                if (searchDto.DateTo.HasValue)
                {
                    query = query.Where(d => d.UploadedAt <= searchDto.DateTo.Value);
                }

                // Apply pagination
                var documents = await query
                    .OrderByDescending(d => d.UploadedAt)
                    .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting documents for user {UserId}", userId);
                throw;
            }
        }

        public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDto, Stream fileStream)
        {
            try
            {
                // Upload file to blob storage
                var containerName = "documents";
                var blobName = await _blobStorageService.UploadFileAsync(fileStream, createDto.FileName, containerName);

                // Create document record
                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = createDto.FileName,
                    FileType = createDto.FileType,
                    FileSize = createDto.FileSize,
                    BlobUrl = await _blobStorageService.GetFileUrlAsync(blobName, containerName),
                    ContainerName = containerName,
                    BlobName = blobName,
                    Status = DocumentStatus.Uploaded,
                    UserId = createDto.UserId,
                    ProjectName = createDto.ProjectName,
                    Description = createDto.Description,
                    DocumentCategory = createDto.DocumentCategory,
                    IsPublic = createDto.IsPublic,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Log audit event
                await _auditService.LogEventAsync("DocumentCreated", "Document", document.Id.ToString(), createDto.UserId);

                _logger.Information("Document created successfully: {DocumentId} by user {UserId}", document.Id, createDto.UserId);

                return MapToDto(document);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating document for user {UserId}", createDto.UserId);
                throw;
            }
        }

        public async Task<DocumentDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto updateDto, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

                if (document == null)
                    throw new InvalidOperationException("Document not found or access denied");

                // Update properties
                if (!string.IsNullOrEmpty(updateDto.ProjectName))
                    document.ProjectName = updateDto.ProjectName;
                if (!string.IsNullOrEmpty(updateDto.Description))
                    document.Description = updateDto.Description;
                if (!string.IsNullOrEmpty(updateDto.DocumentCategory))
                    document.DocumentCategory = updateDto.DocumentCategory;
                
                document.IsPublic = updateDto.IsPublic;
                document.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log audit event
                await _auditService.LogEventAsync("DocumentUpdated", "Document", id.ToString(), userId);

                _logger.Information("Document updated successfully: {DocumentId} by user {UserId}", id, userId);

                return await GetDocumentByIdAsync(id, userId) ?? throw new InvalidOperationException("Document not found after update");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating document {DocumentId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(Guid id, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

                if (document == null)
                    return false;

                // Soft delete
                document.IsDeleted = true;
                document.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log audit event
                await _auditService.LogEventAsync("DocumentDeleted", "Document", id.ToString(), userId);

                _logger.Information("Document deleted successfully: {DocumentId} by user {UserId}", id, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting document {DocumentId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<DocumentDto> ProcessDocumentAsync(Guid id)
        {
            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                    throw new InvalidOperationException("Document not found");

                // Update status to processing
                document.Status = DocumentStatus.Processing;
                await _context.SaveChangesAsync();

                // Download document from blob storage
                var stream = await _blobStorageService.DownloadFileAsync(document.BlobName, document.ContainerName);

                // Process with cognitive services
                DocumentProcessingResultDto result;
                if (document.FileType.ToLower() == ".pdf")
                {
                    result = await _cognitiveServices.ProcessDocumentWithFormRecognizerAsync(stream, document.FileName);
                }
                else
                {
                    result = await _cognitiveServices.ProcessDocumentWithComputerVisionAsync(stream, document.FileName);
                }

                // Update document with results
                document.Status = result.Status == "Success" ? DocumentStatus.Processed : DocumentStatus.Failed;
                document.ProcessedAt = DateTime.UtcNow;
                document.ProcessingResult = result.ProcessingResult;
                document.ErrorMessage = result.ErrorMessage;
                document.ProcessingType = document.FileType.ToLower() == ".pdf" ? "FormRecognizer" : "ComputerVision";

                // Save extracted fields
                foreach (var field in result.ExtractedFields)
                {
                    var documentField = new DocumentField
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = document.Id,
                        FieldName = field.FieldName,
                        FieldValue = field.FieldValue,
                        FieldType = field.FieldType,
                        Confidence = field.Confidence,
                        BoundingBox = field.BoundingBox,
                        PageNumber = field.PageNumber,
                        ExtractedAt = DateTime.UtcNow,
                        ExtractedBy = "Azure Cognitive Services"
                    };

                    _context.DocumentFields.Add(documentField);
                }

                await _context.SaveChangesAsync();

                // Log audit event
                await _auditService.LogEventAsync("DocumentProcessed", "Document", id.ToString(), document.UserId);

                _logger.Information("Document processed successfully: {DocumentId}", id);

                return MapToDto(document);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing document {DocumentId}", id);
                throw;
            }
        }

        public async Task<DocumentDto> RetryProcessingAsync(Guid id, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

                if (document == null)
                    throw new InvalidOperationException("Document not found or access denied");

                // Clear previous processing results
                var existingFields = await _context.DocumentFields
                    .Where(f => f.DocumentId == id)
                    .ToListAsync();

                _context.DocumentFields.RemoveRange(existingFields);

                document.Status = DocumentStatus.Uploaded;
                document.ProcessedAt = null;
                document.ProcessingResult = null;
                document.ErrorMessage = null;
                document.RetryCount++;

                await _context.SaveChangesAsync();

                // Log audit event
                await _auditService.LogEventAsync("DocumentRetry", "Document", id.ToString(), userId);

                _logger.Information("Document retry initiated: {DocumentId} by user {UserId}", id, userId);

                return MapToDto(document);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrying document processing {DocumentId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<Stream> DownloadDocumentAsync(Guid id, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id && (d.UserId == userId || d.IsPublic));

                if (document == null)
                    throw new InvalidOperationException("Document not found or access denied");

                var stream = await _blobStorageService.DownloadFileAsync(document.BlobName, document.ContainerName);

                // Log audit event
                await _auditService.LogEventAsync("DocumentDownloaded", "Document", id.ToString(), userId);

                return stream;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error downloading document {DocumentId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<List<DocumentFieldDto>> GetDocumentFieldsAsync(Guid documentId, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == documentId && (d.UserId == userId || d.IsPublic));

                if (document == null)
                    throw new InvalidOperationException("Document not found or access denied");

                var fields = await _context.DocumentFields
                    .Where(f => f.DocumentId == documentId)
                    .ToListAsync();

                return fields.Select(MapFieldToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting document fields for document {DocumentId} and user {UserId}", documentId, userId);
                throw;
            }
        }

        public async Task<DocumentFieldDto> UpdateDocumentFieldAsync(Guid fieldId, string fieldValue, string userId)
        {
            try
            {
                var field = await _context.DocumentFields
                    .Include(f => f.Document)
                    .FirstOrDefaultAsync(f => f.Id == fieldId && f.Document.UserId == userId);

                if (field == null)
                    throw new InvalidOperationException("Field not found or access denied");

                field.FieldValue = fieldValue;
                await _context.SaveChangesAsync();

                // Log audit event
                await _auditService.LogEventAsync("DocumentFieldUpdated", "DocumentField", fieldId.ToString(), userId);

                return MapFieldToDto(field);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating document field {FieldId} for user {UserId}", fieldId, userId);
                throw;
            }
        }

        public async Task<bool> VerifyDocumentFieldAsync(Guid fieldId, string userId)
        {
            try
            {
                var field = await _context.DocumentFields
                    .Include(f => f.Document)
                    .FirstOrDefaultAsync(f => f.Id == fieldId && f.Document.UserId == userId);

                if (field == null)
                    return false;

                field.IsVerified = true;
                field.VerifiedBy = userId;
                field.VerifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log audit event
                await _auditService.LogEventAsync("DocumentFieldVerified", "DocumentField", fieldId.ToString(), userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error verifying document field {FieldId} for user {UserId}", fieldId, userId);
                throw;
            }
        }

        public async Task<DocumentProcessingResultDto> GetProcessingResultAsync(Guid documentId, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.ExtractedFields)
                    .FirstOrDefaultAsync(d => d.Id == documentId && (d.UserId == userId || d.IsPublic));

                if (document == null)
                    throw new InvalidOperationException("Document not found or access denied");

                return new DocumentProcessingResultDto
                {
                    DocumentId = document.Id,
                    Status = document.Status.ToString(),
                    ProcessedAt = document.ProcessedAt ?? DateTime.UtcNow,
                    ProcessingResult = document.ProcessingResult,
                    ErrorMessage = document.ErrorMessage,
                    ExtractedFields = document.ExtractedFields.Select(MapFieldToDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting processing result for document {DocumentId} and user {UserId}", documentId, userId);
                throw;
            }
        }

        public async Task<int> GetDocumentCountAsync(string userId)
        {
            try
            {
                return await _context.Documents
                    .CountAsync(d => d.UserId == userId && !d.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting document count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<string>> GetDocumentCategoriesAsync(string userId)
        {
            try
            {
                return await _context.Documents
                    .Where(d => d.UserId == userId && !d.IsDeleted && !string.IsNullOrEmpty(d.DocumentCategory))
                    .Select(d => d.DocumentCategory!)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting document categories for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsDocumentAccessibleAsync(Guid documentId, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                return document != null && (document.UserId == userId || document.IsPublic);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking document access for document {DocumentId} and user {UserId}", documentId, userId);
                return false;
            }
        }

        private static DocumentDto MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FileType = document.FileType,
                FileSize = document.FileSize,
                BlobUrl = document.BlobUrl,
                ContainerName = document.ContainerName,
                BlobName = document.BlobName,
                Status = document.Status.ToString(),
                ProcessingType = document.ProcessingType,
                UploadedAt = document.UploadedAt,
                ProcessedAt = document.ProcessedAt,
                LastModified = document.LastModified,
                ProcessingResult = document.ProcessingResult,
                ErrorMessage = document.ErrorMessage,
                RetryCount = document.RetryCount,
                UserId = document.UserId,
                ProjectName = document.ProjectName,
                Description = document.Description,
                DocumentCategory = document.DocumentCategory,
                IsPublic = document.IsPublic,
                IsDeleted = document.IsDeleted,
                ExtractedFields = document.ExtractedFields.Select(MapFieldToDto).ToList()
            };
        }

        private static DocumentFieldDto MapFieldToDto(DocumentField field)
        {
            return new DocumentFieldDto
            {
                Id = field.Id,
                DocumentId = field.DocumentId,
                FieldName = field.FieldName,
                FieldValue = field.FieldValue,
                FieldType = field.FieldType,
                Confidence = field.Confidence,
                BoundingBox = field.BoundingBox,
                PageNumber = field.PageNumber,
                ExtractedAt = field.ExtractedAt,
                ExtractedBy = field.ExtractedBy,
                IsVerified = field.IsVerified,
                VerifiedBy = field.VerifiedBy,
                VerifiedAt = field.VerifiedAt,
                Notes = field.Notes
            };
        }
    }
} 