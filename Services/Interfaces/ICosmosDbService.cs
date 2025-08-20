using DocuSense.Models;

namespace DocuSense.Services.Interfaces
{
    public interface ICosmosDbService
    {
        Task<bool> InitializeAsync();
        Task<T?> GetItemAsync<T>(string id, string partitionKey) where T : class;
        Task<List<T>> GetItemsAsync<T>(string query) where T : class;
        Task<T> CreateItemAsync<T>(T item, string partitionKey) where T : class;
        Task<T> UpdateItemAsync<T>(string id, T item, string partitionKey) where T : class;
        Task<bool> DeleteItemAsync<T>(string id, string partitionKey) where T : class;
        Task<List<AuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> SaveAuditLogAsync(AuditLog auditLog);
        Task<List<AuditLog>> GetAnomaliesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetAuditLogCountAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
} 