namespace DocuSense.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogEventAsync(string action, string entityType, string entityId, string userId, string? description = null, string? details = null);
        Task<List<Models.AuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50);
        Task<List<Models.AuditLog>> GetAuditLogsByEntityAsync(string entityType, string entityId);
        Task<List<Models.AuditLog>> GetAnomaliesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetAuditLogCountAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> IsAnomalousActivityAsync(string userId, string action, DateTime timestamp);
    }
} 