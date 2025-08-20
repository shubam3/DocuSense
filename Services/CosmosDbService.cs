using DocuSense.Models;
using DocuSense.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DocuSense.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _auditLogsContainer;
        private readonly ILogger _logger;

        public CosmosDbService(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            
            var connectionString = configuration["ConnectionStrings:CosmosDb"];
            var databaseName = "DocuSense";
            var containerName = "AuditLogs";

            _cosmosClient = new CosmosClient(connectionString);
            _auditLogsContainer = _cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                // Create database if it doesn't exist
                var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync("DocuSense");

                // Create container if it doesn't exist
                var containerProperties = new ContainerProperties
                {
                    Id = "AuditLogs",
                    PartitionKeyPath = "/userId"
                };

                await database.Database.CreateContainerIfNotExistsAsync(containerProperties);

                _logger.Information("Cosmos DB initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing Cosmos DB");
                return false;
            }
        }

        public async Task<T?> GetItemAsync<T>(string id, string partitionKey) where T : class
        {
            try
            {
                var response = await _auditLogsContainer.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting item from Cosmos DB: {Id}", id);
                throw;
            }
        }

        public async Task<List<T>> GetItemsAsync<T>(string query) where T : class
        {
            try
            {
                var items = new List<T>();
                var queryDefinition = new QueryDefinition(query);
                var iterator = _auditLogsContainer.GetItemQueryIterator<T>(queryDefinition);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    items.AddRange(response);
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error querying items from Cosmos DB");
                throw;
            }
        }

        public async Task<T> CreateItemAsync<T>(T item, string partitionKey) where T : class
        {
            try
            {
                var response = await _auditLogsContainer.CreateItemAsync(item, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating item in Cosmos DB");
                throw;
            }
        }

        public async Task<T> UpdateItemAsync<T>(string id, T item, string partitionKey) where T : class
        {
            try
            {
                var response = await _auditLogsContainer.ReplaceItemAsync(item, id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating item in Cosmos DB: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteItemAsync<T>(string id, string partitionKey) where T : class
        {
            try
            {
                await _auditLogsContainer.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting item from Cosmos DB: {Id}", id);
                return false;
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = "SELECT * FROM c WHERE c.entityType = 'AuditLog'";
                var parameters = new List<object>();

                if (!string.IsNullOrEmpty(userId))
                {
                    query += " AND c.userId = @userId";
                    parameters.Add(new { userId });
                }

                if (fromDate.HasValue)
                {
                    query += " AND c.timestamp >= @fromDate";
                    parameters.Add(new { fromDate = fromDate.Value.ToString("O") });
                }

                if (toDate.HasValue)
                {
                    query += " AND c.timestamp <= @toDate";
                    parameters.Add(new { toDate = toDate.Value.ToString("O") });
                }

                query += " ORDER BY c.timestamp DESC";

                var auditLogs = await GetItemsAsync<AuditLog>(query);
                return auditLogs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting audit logs from Cosmos DB");
                throw;
            }
        }

        public async Task<bool> SaveAuditLogAsync(AuditLog auditLog)
        {
            try
            {
                await CreateItemAsync(auditLog, auditLog.UserId ?? "system");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving audit log to Cosmos DB");
                return false;
            }
        }

        public async Task<List<AuditLog>> GetAnomaliesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = "SELECT * FROM c WHERE c.entityType = 'AuditLog' AND c.isAnomaly = true";
                var parameters = new List<object>();

                if (fromDate.HasValue)
                {
                    query += " AND c.timestamp >= @fromDate";
                    parameters.Add(new { fromDate = fromDate.Value.ToString("O") });
                }

                if (toDate.HasValue)
                {
                    query += " AND c.timestamp <= @toDate";
                    parameters.Add(new { toDate = toDate.Value.ToString("O") });
                }

                query += " ORDER BY c.timestamp DESC";

                var anomalies = await GetItemsAsync<AuditLog>(query);
                return anomalies;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting anomalies from Cosmos DB");
                throw;
            }
        }

        public async Task<int> GetAuditLogCountAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = "SELECT VALUE COUNT(1) FROM c WHERE c.entityType = 'AuditLog'";
                var parameters = new List<object>();

                if (!string.IsNullOrEmpty(userId))
                {
                    query += " AND c.userId = @userId";
                    parameters.Add(new { userId });
                }

                if (fromDate.HasValue)
                {
                    query += " AND c.timestamp >= @fromDate";
                    parameters.Add(new { fromDate = fromDate.Value.ToString("O") });
                }

                if (toDate.HasValue)
                {
                    query += " AND c.timestamp <= @toDate";
                    parameters.Add(new { toDate = toDate.Value.ToString("O") });
                }

                var result = await GetItemsAsync<int>(query);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting audit log count from Cosmos DB");
                throw;
            }
        }
    }
} 