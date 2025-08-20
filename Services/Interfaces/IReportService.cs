using DocuSense.Models;

namespace DocuSense.Services.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateDocumentReportAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> GenerateAuditReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> GenerateUserActivityReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<Dictionary<string, object>> GetDashboardStatsAsync(string userId);
        Task<List<AuditLog>> GetRecentActivityAsync(int count = 10);
        Task<Dictionary<string, int>> GetDocumentStatsByStatusAsync(string userId);
        Task<Dictionary<string, int>> GetDocumentStatsByCategoryAsync(string userId);
        Task<List<AuditLog>> GetAnomalyReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
} 