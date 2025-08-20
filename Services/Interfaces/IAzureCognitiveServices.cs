using DocuSense.DTOs;

namespace DocuSense.Services.Interfaces
{
    public interface IAzureCognitiveServices
    {
        Task<DocumentProcessingResultDto> ProcessDocumentWithFormRecognizerAsync(Stream documentStream, string fileName);
        Task<DocumentProcessingResultDto> ProcessDocumentWithComputerVisionAsync(Stream documentStream, string fileName);
        Task<List<DocumentFieldDto>> ExtractTextFromImageAsync(Stream imageStream);
        Task<List<DocumentFieldDto>> ExtractFormFieldsAsync(Stream documentStream);
        Task<string> ExtractTextAsync(Stream documentStream);
        Task<double> GetDocumentConfidenceAsync(Stream documentStream);
        Task<bool> IsDocumentValidAsync(Stream documentStream, string fileName);
        Task<string> GetDocumentTypeAsync(Stream documentStream);
        Task<List<string>> GetSupportedLanguagesAsync();
        Task<DocumentProcessingResultDto> AnalyzeDocumentLayoutAsync(Stream documentStream);
    }
} 