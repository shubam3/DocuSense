using DocuSense.DTOs;
using DocuSense.Models;

namespace DocuSense.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentDto?> GetDocumentByIdAsync(Guid id, string userId);
        Task<List<DocumentDto>> GetDocumentsByUserAsync(string userId, DocumentSearchDto searchDto);
        Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDto, Stream fileStream);
        Task<DocumentDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto updateDto, string userId);
        Task<bool> DeleteDocumentAsync(Guid id, string userId);
        Task<DocumentDto> ProcessDocumentAsync(Guid id);
        Task<DocumentDto> RetryProcessingAsync(Guid id, string userId);
        Task<Stream> DownloadDocumentAsync(Guid id, string userId);
        Task<List<DocumentFieldDto>> GetDocumentFieldsAsync(Guid documentId, string userId);
        Task<DocumentFieldDto> UpdateDocumentFieldAsync(Guid fieldId, string fieldValue, string userId);
        Task<bool> VerifyDocumentFieldAsync(Guid fieldId, string userId);
        Task<DocumentProcessingResultDto> GetProcessingResultAsync(Guid documentId, string userId);
        Task<int> GetDocumentCountAsync(string userId);
        Task<List<string>> GetDocumentCategoriesAsync(string userId);
        Task<bool> IsDocumentAccessibleAsync(Guid documentId, string userId);
    }
} 