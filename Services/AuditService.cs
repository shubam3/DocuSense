using DocuSense.Data;
using DocuSense.Models;
using DocuSense.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DocuSense.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public AuditService(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogEventAsync(string action, string entityType, string entityId, string userId, string? description = null, string? details = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    UserId = userId,
                    Description = description,
                    Details = details,
                    Status = "Success",
                    Severity = "Info"
                };

                // Check for anomalous activity
                var isAnomalous = await IsAnomalousActivityAsync(userId, action, auditLog.Timestamp);
                if (isAnomalous)
                {
                    auditLog.IsAnomaly = true;
                    auditLog.AnomalyReason = "Unusual activity pattern detected";
                    auditLog.Severity = "Warning";
                }

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.Information("Audit event logged: {Action} on {EntityType} {EntityId} by {UserId}", 
                    action, entityType, entityId, userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error logging audit event: {Action} on {EntityType} {EntityId} by {UserId}", 
                    action, entityType, entityId, userId);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.AuditLogs
                    .Include(a => a.User)
                    .Include(a => a.Document)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(a => a.UserId == userId);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= toDate.Value);
                }

                var auditLogs = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return auditLogs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving audit logs");
                throw;
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsByEntityAsync(string entityType, string entityId)
        {
            try
            {
                var auditLogs = await _context.AuditLogs
                    .Include(a => a.User)
                    .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();

                return auditLogs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving audit logs for entity {EntityType} {EntityId}", entityType, entityId);
                throw;
            }
        }

        public async Task<List<AuditLog>> GetAnomaliesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.AuditLogs
                    .Include(a => a.User)
                    .Where(a => a.IsAnomaly);

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= toDate.Value);
                }

                var anomalies = await query
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();

                return anomalies;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving anomalies");
                throw;
            }
        }

        public async Task<int> GetAuditLogCountAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(a => a.UserId == userId);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= toDate.Value);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting audit log count");
                throw;
            }
        }

        public async Task<bool> IsAnomalousActivityAsync(string userId, string action, DateTime timestamp)
        {
            try
            {
                // Check for rapid successive actions (within 1 second)
                var recentActions = await _context.AuditLogs
                    .Where(a => a.UserId == userId && a.Timestamp >= timestamp.AddSeconds(-1))
                    .CountAsync();

                if (recentActions > 5) // More than 5 actions in 1 second
                {
                    return true;
                }

                // Check for unusual action patterns
                var lastHourActions = await _context.AuditLogs
                    .Where(a => a.UserId == userId && a.Timestamp >= timestamp.AddHours(-1))
                    .GroupBy(a => a.Action)
                    .Select(g => new { Action = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var actionGroup in lastHourActions)
                {
                    // Flag if user performs the same action more than 50 times in an hour
                    if (actionGroup.Count > 50)
                    {
                        return true;
                    }
                }

                // Check for failed actions in the last hour
                var failedActions = await _context.AuditLogs
                    .Where(a => a.UserId == userId && 
                               a.Timestamp >= timestamp.AddHours(-1) && 
                               a.Status == "Failed")
                    .CountAsync();

                if (failedActions > 10) // More than 10 failed actions in an hour
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking for anomalous activity for user {UserId}", userId);
                return false;
            }
        }
    }
} 