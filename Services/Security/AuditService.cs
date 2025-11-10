using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ADPA.Data;
using ADPA.Models.Entities;
using ADPA.Models.DTOs;
using System.Security.Cryptography;
using System.Text;
// Use aliases to disambiguate between DTOs and Entities for ComplianceViolation
using ComplianceViolationDto = ADPA.Models.DTOs.ComplianceViolation;

namespace ADPA.Services.Security;

/// <summary>
/// Phase 5.4: Comprehensive Audit Service Interface
/// Complete audit logging, compliance reporting, and data lineage tracking
/// </summary>
public interface IAuditService
{
    // Core Audit Logging
    Task LogEventAsync(AuditLogEntry entry);
    Task LogEventAsync(string eventType, string action, string resource, bool wasSuccessful = true, 
        Dictionary<string, object>? metadata = null, Guid? userId = null);
    Task LogAsync(string message, string entityType, string entityId, string userId);
    Task LogSecurityEventAsync(string eventType, string message, string userId, object metadata);
    Task<List<AuditLogEntry>> GetAuditLogsAsync(ADPA.Models.DTOs.AuditSearchRequest request);
    Task<ADPA.Models.DTOs.AuditSearchResponse> SearchAuditLogsAsync(ADPA.Models.DTOs.AuditSearchRequest request);
    
    // Data Lineage Tracking
    Task LogDataLineageAsync(DataLineageEntry entry);
    Task LogDataOperationAsync(string dataType, string dataId, DataOperation operation, Guid userId, 
        string purpose, string? legalBasis = null);
    Task<List<DataLineageEntry>> GetDataLineageAsync(string dataType, string? dataId = null);
    Task<List<DataLineageEntry>> TraceDataFlowAsync(string dataId, DateTime? fromDate = null);
    
    // Compliance Auditing
    Task LogComplianceEventAsync(ComplianceAuditEntry entry);
    Task<ComplianceReportResponse> GenerateComplianceReportAsync(ComplianceReportRequest request);
    Task<List<ComplianceAuditEntry>> GetComplianceAuditTrailAsync(ComplianceFramework framework, 
        DateTime? startDate = null, DateTime? endDate = null);
    Task<List<ComplianceViolationDto>> GetComplianceViolationsAsync(ComplianceFramework framework, 
        ComplianceStatus? status = null);
    
    // Audit Trail Analysis
    Task<AuditAnalysisResult> AnalyzeAuditTrailAsync(DateTime startDate, DateTime endDate);
    Task<List<AuditPattern>> DetectPatternsAsync(TimeSpan analysisWindow);
    Task<List<AuditAnomaly>> DetectAnomaliesAsync(TimeSpan analysisWindow);
    
    // Configuration Management
    Task<AuditConfiguration> GetAuditConfigurationAsync();
    Task<bool> UpdateAuditConfigurationAsync(AuditConfiguration config);
    Task<bool> ValidateAuditConfigurationAsync(AuditConfiguration config);
    
    // Alert Management
    Task<List<AuditAlertRule>> GetAlertRulesAsync();
    Task<AuditAlertRule> CreateAlertRuleAsync(AuditAlertRule rule);
    Task<bool> UpdateAlertRuleAsync(AuditAlertRule rule);
    Task<bool> DeleteAlertRuleAsync(Guid ruleId);
    Task ProcessAuditAlertsAsync(AuditLogEntry entry);
    
    // Audit Integrity & Security
    Task<bool> VerifyAuditIntegrityAsync(Guid entryId);
    Task<bool> VerifyAuditTrailIntegrityAsync(DateTime startDate, DateTime endDate);
    Task<string> CalculateAuditHashAsync(AuditLogEntry entry);
    
    // Export & Archival
    Task<byte[]> ExportAuditLogsAsync(AuditSearchRequest request, ReportFormat format);
    Task<bool> ArchiveAuditLogsAsync(DateTime beforeDate);
    Task<bool> PurgeAuditLogsAsync(DateTime beforeDate, bool force = false);
    
    // Statistics & Reporting
    Task<Dictionary<string, object>> GetAuditStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<List<AuditLogEntry>> GetRecentHighRiskEventsAsync(int count = 10);
    Task<Dictionary<string, long>> GetEventFrequencyAsync(TimeSpan period);
}

/// <summary>
/// Comprehensive Audit Service Implementation
/// </summary>
public class AuditService : IAuditService
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly AuditConfiguration _config;
    private readonly IAuditStorage _storage;
    private readonly IAuditIntegrityService _integrityService;
    private readonly IComplianceReportGenerator _reportGenerator;

    public AuditService(
        AdpaEfDbContext context,
        ILogger<AuditService> logger,
        IOptions<AuditConfiguration> config,
        IAuditStorage storage,
        IAuditIntegrityService integrityService,
        IComplianceReportGenerator reportGenerator)
    {
        _context = context;
        _logger = logger;
        _config = config.Value;
        _storage = storage;
        _integrityService = integrityService;
        _reportGenerator = reportGenerator;
    }

    public async Task LogEventAsync(AuditLogEntry entry)
    {
        try
        {
            if (!_config.IsEnabled) return;

            // Validate and enrich entry
            await EnrichAuditEntry(entry);
            
            // Apply filtering
            if (!ShouldLogEvent(entry)) return;

            // Calculate integrity hash
            var integrityhash = await _integrityService.CalculateHashAsync(entry);
            var existingMetadata = entry.GetMetadata();
            existingMetadata["IntegrityHash"] = integrityhash;
            entry.SetMetadata(existingMetadata);
            
            // Store the audit entry
            await _storage.StoreAuditEntryAsync(entry);
            
            // Process alerts
            await ProcessAuditAlertsAsync(entry);
            
            _logger.LogDebug("Audit event logged: {EventType} for resource {Resource}", 
                entry.EventType, entry.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit event: {EventType}", entry.EventType);
            // Don't throw - audit logging should not break the main application
        }
    }

    public async Task LogEventAsync(string eventType, string action, string resource, bool wasSuccessful = true,
        Dictionary<string, object>? metadata = null, Guid? userId = null)
    {
        var entry = new AuditLogEntry
        {
            EventType = eventType,
            Action = action,
            Resource = resource,
            WasSuccessful = wasSuccessful,
            UserId = userId,
            Category = DetermineCategory(eventType),
            Severity = DetermineSeverity(eventType, wasSuccessful)
        };

        // Set metadata using the helper method
        if (metadata != null)
        {
            entry.SetMetadata(metadata);
        }

        await LogEventAsync(entry);
    }

    public async Task LogAsync(string message, string entityType, string entityId, string userId)
    {
        await LogEventAsync(
            eventType: "AuditLog", 
            action: message,
            resource: $"{entityType}:{entityId}",
            wasSuccessful: true,
            metadata: new Dictionary<string, object>
            {
                ["EntityType"] = entityType,
                ["EntityId"] = entityId
            },
            userId: Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null
        );
    }

    public async Task LogSecurityEventAsync(string eventType, string message, string userId, object metadata)
    {
        var metadataDict = new Dictionary<string, object>();
        
        if (metadata != null)
        {
            // Convert metadata object to dictionary using reflection or JSON serialization
            var properties = metadata.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(metadata);
                if (value != null)
                    metadataDict[prop.Name] = value;
            }
        }

        await LogEventAsync(
            eventType: eventType,
            action: message,
            resource: "SecurityConfiguration",
            wasSuccessful: true,
            metadata: metadataDict,
            userId: Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null
        );
    }

    public async Task<List<AuditLogEntry>> GetAuditLogsAsync(AuditSearchRequest request)
    {
        return await _storage.SearchAuditEntriesAsync(request);
    }

    public async Task<AuditSearchResponse> SearchAuditLogsAsync(AuditSearchRequest request)
    {
        var entries = await _storage.SearchAuditEntriesAsync(request);
        var totalCount = await _storage.CountAuditEntriesAsync(request);
        
        return new AuditSearchResponse
        {
            Results = entries,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasNextPage = request.Page < (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasPreviousPage = request.Page > 1
        };
    }

    public async Task LogDataLineageAsync(DataLineageEntry entry)
    {
        try
        {
            await _storage.StoreDataLineageEntryAsync(entry);
            
            // Also log as regular audit event for comprehensive tracking
            var auditLogEntry = new AuditLogEntry
            {
                EventType = "DataLineage",
                Category = "DataGovernance", 
                Action = entry.Operation.ToString(),
                Resource = $"{entry.TableName}.{entry.FieldName}",
                ResourceId = entry.DataId,
                UserId = entry.UserId,
                Description = entry.OperationDescription,
                ComplianceFramework = string.Join(",", entry.ApplicableRegulations)
            };

            // Set metadata using the helper method
            auditLogEntry.SetMetadata(new Dictionary<string, object>
            {
                ["DataType"] = entry.DataType,
                ["Classification"] = entry.Classification,
                ["IsPersonalData"] = entry.IsPersonalData,
                ["PersonalDataTypes"] = entry.PersonalDataTypes,
                ["LegalBasis"] = entry.LegalBasis ?? string.Empty,
                ["Purpose"] = entry.Purpose
            });

            await LogEventAsync(auditLogEntry);

            _logger.LogDebug("Data lineage logged: {Operation} on {DataType}", 
                entry.Operation, entry.DataType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log data lineage for {DataType}", entry.DataType);
        }
    }

    public async Task LogDataOperationAsync(string dataType, string dataId, DataOperation operation, 
        Guid userId, string purpose, string? legalBasis = null)
    {
        var entry = new DataLineageEntry
        {
            DataType = dataType,
            DataId = dataId,
            Operation = operation,
            UserId = userId,
            Purpose = purpose,
            LegalBasis = legalBasis ?? "LegitimateInterest",
            OperationDescription = $"{operation} operation on {dataType}",
            IsPersonalData = IsPersonalDataType(dataType)
        };

        await LogDataLineageAsync(entry);
    }

    public async Task<List<DataLineageEntry>> GetDataLineageAsync(string dataType, string? dataId = null)
    {
        return await _storage.GetDataLineageEntriesAsync(dataType, dataId);
    }

    public async Task<List<DataLineageEntry>> TraceDataFlowAsync(string dataId, DateTime? fromDate = null)
    {
        return await _storage.TraceDataFlowAsync(dataId, fromDate);
    }

    public async Task LogComplianceEventAsync(ComplianceAuditEntry entry)
    {
        try
        {
            await _storage.StoreComplianceAuditEntryAsync(entry);
            
            // Log as audit event
            var auditLogEntry = new AuditLogEntry
            {
                EventType = "Compliance",
                Category = "Compliance",
                Action = entry.EventType,
                Resource = entry.ControlId,
                Description = entry.Description,
                WasSuccessful = entry.Status == ComplianceStatus.Compliant,
                ComplianceFramework = entry.Framework.ToString(),
                ComplianceTags = [entry.Regulation, entry.ControlId],
                RiskLevel = ConvertComplianceRiskToAuditRisk(entry.RiskLevel)
            };

            auditLogEntry.SetMetadata(new Dictionary<string, object>
            {
                ["Framework"] = entry.Framework.ToString(),
                ["Regulation"] = entry.Regulation,
                ["ControlId"] = entry.ControlId,
                ["Status"] = entry.Status.ToString(),
                ["AssessmentResult"] = entry.AssessmentResult
            });

            await LogEventAsync(auditLogEntry);

            _logger.LogDebug("Compliance event logged: {Framework} {ControlId}", 
                entry.Framework, entry.ControlId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log compliance event for {Framework} {ControlId}", 
                entry.Framework, entry.ControlId);
        }
    }

    public async Task<ComplianceReportResponse> GenerateComplianceReportAsync(ComplianceReportRequest request)
    {
        try
        {
            var auditEntries = await _storage.GetComplianceAuditEntriesAsync(
                request.Framework, request.StartDate, request.EndDate);

            var report = await _reportGenerator.GenerateReportAsync(request, auditEntries);
            
            // Log report generation
            await LogEventAsync("ComplianceReportGenerated", "Generate", "ComplianceReport", true,
                new Dictionary<string, object>
                {
                    ["Framework"] = request.Framework.ToString(),
                    ["StartDate"] = request.StartDate,
                    ["EndDate"] = request.EndDate,
                    ["Format"] = request.Format.ToString(),
                    ["ReportId"] = report.ReportId
                });

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate compliance report for {Framework}", request.Framework);
            throw;
        }
    }

    public async Task<List<ComplianceAuditEntry>> GetComplianceAuditTrailAsync(ComplianceFramework framework,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _storage.GetComplianceAuditEntriesAsync(framework, startDate, endDate);
    }

    public async Task<List<ComplianceViolationDto>> GetComplianceViolationsAsync(ComplianceFramework framework,
        ComplianceStatus? status = null)
    {
        return await _storage.GetComplianceViolationsAsync(framework, status);
    }

    public async Task<AuditAnalysisResult> AnalyzeAuditTrailAsync(DateTime startDate, DateTime endDate)
    {
        var entries = await _storage.SearchAuditEntriesAsync(new AuditSearchRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            PageSize = int.MaxValue
        });

        var result = new AuditAnalysisResult
        {
            AnalysisPeriod = endDate - startDate,
            TotalEvents = entries.Count
        };

        // Calculate statistics
        result.EventsByType = entries.GroupBy(e => e.EventType)
            .ToDictionary(g => g.Key, g => g.Count());

        result.EventsBySeverity = entries.GroupBy(e => e.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        result.EventsByUser = entries.Where(e => e.UserId.HasValue)
            .GroupBy(e => e.UserName)
            .ToDictionary(g => g.Key, g => g.Count());

        // Risk assessment
        result.HighRiskEvents = entries.Where(e => e.RiskLevel == AuditRiskLevel.High || e.RiskLevel == AuditRiskLevel.Critical).ToList();
        result.SecurityViolations = entries.Where(e => e.Category == "Security" && !e.WasSuccessful)
            .Select(e => new ADPA.Models.DTOs.ComplianceViolation
            {
                Id = e.Id,
                Framework = ComplianceFramework.GDPR,
                Control = e.EventType,
                Requirement = e.Action,
                Severity = ComplianceSeverity.High,
                Description = e.Description,
                DetectedAt = e.Timestamp,
                Status = ComplianceStatus.NonCompliant
            }).ToList();
        result.ComplianceViolations = entries.Where(e => e.Category == "Compliance" && !e.WasSuccessful)
            .Select(e => new ADPA.Models.DTOs.ComplianceViolation
            {
                Id = e.Id,
                Framework = ComplianceFramework.GDPR,
                Control = e.EventType,
                Requirement = e.Action,
                Severity = ComplianceSeverity.High,
                Description = e.Description,
                DetectedAt = e.Timestamp,
                Status = ComplianceStatus.NonCompliant
            }).ToList();

        // Detect patterns and anomalies
        result.DetectedPatterns = await DetectPatternsInEntries(entries);
        result.DetectedAnomalies = await DetectAnomaliesInEntries(entries);

        return result;
    }

    public async Task<List<AuditPattern>> DetectPatternsAsync(TimeSpan analysisWindow)
    {
        var startDate = DateTime.UtcNow.Subtract(analysisWindow);
        var entries = await _storage.SearchAuditEntriesAsync(new AuditSearchRequest
        {
            StartDate = startDate,
            PageSize = int.MaxValue
        });

        return await DetectPatternsInEntries(entries);
    }

    public async Task<List<AuditAnomaly>> DetectAnomaliesAsync(TimeSpan analysisWindow)
    {
        var startDate = DateTime.UtcNow.Subtract(analysisWindow);
        var entries = await _storage.SearchAuditEntriesAsync(new AuditSearchRequest
        {
            StartDate = startDate,
            PageSize = int.MaxValue
        });

        return await DetectAnomaliesInEntries(entries);
    }

    public async Task<AuditConfiguration> GetAuditConfigurationAsync()
    {
        return await _storage.GetAuditConfigurationAsync();
    }

    public async Task<bool> UpdateAuditConfigurationAsync(AuditConfiguration config)
    {
        try
        {
            if (!await ValidateAuditConfigurationAsync(config))
                return false;

            await _storage.UpdateAuditConfigurationAsync(config);
            
            await LogEventAsync("ConfigurationUpdated", "Update", "AuditConfiguration", true,
                new Dictionary<string, object> { ["ConfigurationId"] = config.Id });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update audit configuration");
            return false;
        }
    }

    public async Task<bool> ValidateAuditConfigurationAsync(AuditConfiguration config)
    {
        // Validate retention period
        if (config.RetentionPeriod < TimeSpan.FromDays(30))
        {
            _logger.LogWarning("Retention period too short: {Period}", config.RetentionPeriod);
            return false;
        }

        // Validate storage settings
        if (string.IsNullOrEmpty(config.StorageConnectionString) && 
            config.StorageType != AuditStorageType.Database)
        {
            _logger.LogWarning("Storage connection string required for {StorageType}", config.StorageType);
            return false;
        }

        return true;
    }

    public async Task<List<AuditAlertRule>> GetAlertRulesAsync()
    {
        return await _storage.GetAlertRulesAsync();
    }

    public async Task<AuditAlertRule> CreateAlertRuleAsync(AuditAlertRule rule)
    {
        rule.CreatedAt = DateTime.UtcNow;
        await _storage.StoreAlertRuleAsync(rule);
        
        await LogEventAsync("AlertRuleCreated", "Create", "AuditAlertRule", true,
            new Dictionary<string, object> { ["RuleId"] = rule.Id, ["RuleName"] = rule.Name });

        return rule;
    }

    public async Task<bool> UpdateAlertRuleAsync(AuditAlertRule rule)
    {
        try
        {
            await _storage.UpdateAlertRuleAsync(rule);
            
            await LogEventAsync("AlertRuleUpdated", "Update", "AuditAlertRule", true,
                new Dictionary<string, object> { ["RuleId"] = rule.Id });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update alert rule {RuleId}", rule.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAlertRuleAsync(Guid ruleId)
    {
        try
        {
            await _storage.DeleteAlertRuleAsync(ruleId);
            
            await LogEventAsync("AlertRuleDeleted", "Delete", "AuditAlertRule", true,
                new Dictionary<string, object> { ["RuleId"] = ruleId });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete alert rule {RuleId}", ruleId);
            return false;
        }
    }

    public async Task ProcessAuditAlertsAsync(AuditLogEntry entry)
    {
        var rules = await GetAlertRulesAsync();
        var activeRules = rules.Where(r => r.IsEnabled);

        foreach (var rule in activeRules)
        {
            if (await EvaluateAlertRule(rule, entry))
            {
                await TriggerAlert(rule, entry);
            }
        }
    }

    public async Task<bool> VerifyAuditIntegrityAsync(Guid entryId)
    {
        return await _integrityService.VerifyEntryIntegrityAsync(entryId);
    }

    public async Task<bool> VerifyAuditTrailIntegrityAsync(DateTime startDate, DateTime endDate)
    {
        return await _integrityService.VerifyTrailIntegrityAsync(startDate, endDate);
    }

    public async Task<string> CalculateAuditHashAsync(AuditLogEntry entry)
    {
        return await _integrityService.CalculateHashAsync(entry);
    }

    public async Task<byte[]> ExportAuditLogsAsync(AuditSearchRequest request, ReportFormat format)
    {
        var entries = await _storage.SearchAuditEntriesAsync(request);
        return await _reportGenerator.ExportAuditLogsAsync(entries, format);
    }

    public async Task<bool> ArchiveAuditLogsAsync(DateTime beforeDate)
    {
        try
        {
            await _storage.ArchiveAuditLogsAsync(beforeDate);
            
            await LogEventAsync("AuditLogsArchived", "Archive", "AuditLogs", true,
                new Dictionary<string, object> { ["BeforeDate"] = beforeDate });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive audit logs before {Date}", beforeDate);
            return false;
        }
    }

    public async Task<bool> PurgeAuditLogsAsync(DateTime beforeDate, bool force = false)
    {
        try
        {
            // Safety check - don't purge recent logs unless forced
            if (!force && beforeDate > DateTime.UtcNow.AddDays(-30))
            {
                _logger.LogWarning("Attempted to purge recent audit logs - use force flag if intentional");
                return false;
            }

            await _storage.PurgeAuditLogsAsync(beforeDate);
            
            await LogEventAsync("AuditLogsPurged", "Purge", "AuditLogs", true,
                new Dictionary<string, object> { ["BeforeDate"] = beforeDate, ["Forced"] = force });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge audit logs before {Date}", beforeDate);
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetAuditStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var analysis = await AnalyzeAuditTrailAsync(startDate, endDate);
        
        return new Dictionary<string, object>
        {
            ["TotalEvents"] = analysis.TotalEvents,
            ["EventsByType"] = analysis.EventsByType,
            ["EventsBySeverity"] = analysis.EventsBySeverity,
            ["HighRiskEvents"] = analysis.HighRiskEvents,
            ["SecurityViolations"] = analysis.SecurityViolations,
            ["ComplianceViolations"] = analysis.ComplianceViolations,
            ["TopUsers"] = analysis.EventsByUser.OrderByDescending(kvp => kvp.Value).Take(10),
            ["DetectedPatterns"] = analysis.DetectedPatterns.Count,
            ["DetectedAnomalies"] = analysis.DetectedAnomalies.Count
        };
    }

    public async Task<List<AuditLogEntry>> GetRecentHighRiskEventsAsync(int count = 10)
    {
        var request = new AuditSearchRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            PageSize = count,
            SortBy = "Timestamp",
            Descending = true
        };

        var entries = await _storage.SearchAuditEntriesAsync(request);
        return entries.Where(e => e.RiskLevel == AuditRiskLevel.High || e.RiskLevel == AuditRiskLevel.Critical).Take(count).ToList();
    }

    public async Task<Dictionary<string, long>> GetEventFrequencyAsync(TimeSpan period)
    {
        var startDate = DateTime.UtcNow.Subtract(period);
        var entries = await _storage.SearchAuditEntriesAsync(new AuditSearchRequest
        {
            StartDate = startDate,
            PageSize = int.MaxValue
        });

        return entries.GroupBy(e => e.EventType)
            .ToDictionary(g => g.Key, g => (long)g.Count());
    }

    // Private helper methods
    private async Task EnrichAuditEntry(AuditLogEntry entry)
    {
        if (string.IsNullOrEmpty(entry.Category))
            entry.Category = DetermineCategory(entry.EventType);

        if (entry.Severity == AuditSeverity.Information && !entry.WasSuccessful)
            entry.Severity = AuditSeverity.Error;

        if (string.IsNullOrEmpty(entry.RequestId))
            entry.RequestId = Guid.NewGuid().ToString();

        await Task.CompletedTask;
    }

    private bool ShouldLogEvent(AuditLogEntry entry)
    {
        if (!_config.IsEnabled) return false;
        
        if (entry.Severity > _config.MinimumSeverity) return false;
        
        if (_config.ExcludedEventTypes.Contains(entry.EventType)) return false;
        
        if (_config.IncludedEventTypes.Any() && !_config.IncludedEventTypes.Contains(entry.EventType))
            return false;

        return true;
    }

    private string DetermineCategory(string eventType)
    {
        return eventType switch
        {
            var type when type.Contains("Auth") => "Authentication",
            var type when type.Contains("Login") => "Authentication",
            var type when type.Contains("Permission") => "Authorization",
            var type when type.Contains("Role") => "Authorization",
            var type when type.Contains("Data") => "DataAccess",
            var type when type.Contains("Encrypt") => "Security",
            var type when type.Contains("Key") => "Security",
            var type when type.Contains("Config") => "Configuration",
            var type when type.Contains("Compliance") => "Compliance",
            _ => "General"
        };
    }

    private AuditSeverity DetermineSeverity(string eventType, bool wasSuccessful)
    {
        if (!wasSuccessful)
        {
            return eventType switch
            {
                var type when type.Contains("Login") => AuditSeverity.Warning,
                var type when type.Contains("Auth") => AuditSeverity.Warning,
                var type when type.Contains("Permission") => AuditSeverity.Error,
                var type when type.Contains("Security") => AuditSeverity.Critical,
                _ => AuditSeverity.Error
            };
        }

        return AuditSeverity.Information;
    }

    private bool IsPersonalDataType(string dataType)
    {
        var personalDataTypes = new[] { "email", "name", "phone", "address", "ssn", "dob", "ip" };
        return personalDataTypes.Any(pdt => dataType.ToLower().Contains(pdt));
    }

    private async Task<List<AuditPattern>> DetectPatternsInEntries(List<AuditLogEntry> entries)
    {
        var patterns = new List<AuditPattern>();

        // Detect repeated failures
        var failureGroups = entries.Where(e => !e.WasSuccessful)
            .GroupBy(e => new { e.EventType, e.UserId })
            .Where(g => g.Count() >= 5);

        foreach (var group in failureGroups)
        {
            patterns.Add(new AuditPattern
            {
                PatternType = "RepeatedFailures",
                Description = $"User {group.Key.UserId} has {group.Count()} failed {group.Key.EventType} attempts",
                Frequency = group.Count(),
                RiskLevel = group.Count() >= 10 ? AuditRiskLevel.High : AuditRiskLevel.Medium
            });
        }

        await Task.CompletedTask;
        return patterns;
    }

    private async Task<List<AuditAnomaly>> DetectAnomaliesInEntries(List<AuditLogEntry> entries)
    {
        var anomalies = new List<AuditAnomaly>();

        // Detect unusual activity times
        var hourlyActivity = entries.GroupBy(e => e.Timestamp.Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        var avgActivity = hourlyActivity.Values.Average();
        var threshold = avgActivity * 3; // 3x average is anomalous

        foreach (var hour in hourlyActivity.Where(kvp => kvp.Value > threshold))
        {
            anomalies.Add(new AuditAnomaly
            {
                AnomalyType = "UnusualActivityTime",
                Description = $"Unusual activity at hour {hour.Key} with {hour.Value} events",
                Score = hour.Value / avgActivity,
                RiskLevel = hour.Value > avgActivity * 5 ? AuditRiskLevel.High : AuditRiskLevel.Medium,
                FirstDetected = entries.Where(e => e.Timestamp.Hour == hour.Key).Min(e => e.Timestamp)
            });
        }

        await Task.CompletedTask;
        return anomalies;
    }

    private async Task<bool> EvaluateAlertRule(AuditAlertRule rule, AuditLogEntry entry)
    {
        try
        {
            var conditions = JsonSerializer.Deserialize<List<AuditAlertCondition>>(rule.Conditions) ?? new List<AuditAlertCondition>();
            foreach (var condition in conditions)
            {
                if (!await EvaluateCondition(condition, entry))
                    return false;
            }
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async Task<bool> EvaluateCondition(AuditAlertCondition condition, AuditLogEntry entry)
    {
        var fieldValue = GetFieldValue(condition.Field, entry)?.ToString() ?? string.Empty;
        var conditionValue = condition.Value;

        if (!condition.CaseSensitive)
        {
            fieldValue = fieldValue.ToLowerInvariant();
            conditionValue = conditionValue.ToLowerInvariant();
        }

        return condition.Operator switch
        {
            ComparisonOperator.Equals => fieldValue == conditionValue,
            ComparisonOperator.NotEquals => fieldValue != conditionValue,
            ComparisonOperator.Contains => fieldValue.Contains(conditionValue),
            ComparisonOperator.StartsWith => fieldValue.StartsWith(conditionValue),
            ComparisonOperator.EndsWith => fieldValue.EndsWith(conditionValue),
            ComparisonOperator.Matches => System.Text.RegularExpressions.Regex.IsMatch(fieldValue, conditionValue),
            _ => await Task.FromResult(false)
        };
    }

    private object? GetFieldValue(string fieldName, AuditLogEntry entry)
    {
        return fieldName switch
        {
            "EventType" => entry.EventType,
            "Category" => entry.Category,
            "Action" => entry.Action,
            "Resource" => entry.Resource,
            "UserName" => entry.UserName,
            "IpAddress" => entry.IpAddress,
            "WasSuccessful" => entry.WasSuccessful,
            "Severity" => entry.Severity.ToString(),
            "RiskLevel" => entry.RiskLevel.ToString(),
            _ => GetMetadataValue(entry.Metadata, fieldName)
        };
    }

    private string GetMetadataValue(string metadata, string fieldName)
    {
        try
        {
            var metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata) ?? new Dictionary<string, object>();
            return metadataDict.TryGetValue(fieldName, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private AuditRiskLevel ConvertComplianceRiskToAuditRisk(ComplianceRiskLevel complianceRisk)
    {
        return complianceRisk switch
        {
            ComplianceRiskLevel.VeryLow or ComplianceRiskLevel.Low => AuditRiskLevel.Low,
            ComplianceRiskLevel.Medium => AuditRiskLevel.Medium,
            ComplianceRiskLevel.High => AuditRiskLevel.High,
            ComplianceRiskLevel.VeryHigh or ComplianceRiskLevel.Critical => AuditRiskLevel.Critical,
            _ => AuditRiskLevel.Low
        };
    }

    private async Task TriggerAlert(AuditAlertRule rule, AuditLogEntry entry)
    {
        try
        {
            var actions = JsonSerializer.Deserialize<List<AlertAction>>(rule.Actions) ?? new List<AlertAction>();
            foreach (var action in actions)
            {
                await ExecuteAlertAction(action, rule, entry);
            }

            _logger.LogInformation("Alert triggered: {RuleName} for event {EventType}", 
                rule.Name, entry.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger alert for rule {RuleName}", rule.Name);
        }
    }

    private async Task ExecuteAlertAction(AlertAction action, AuditAlertRule rule, AuditLogEntry entry)
    {
        switch (action.Type)
        {
            case AlertActionType.Email:
                // Implementation would send email
                _logger.LogInformation("Email alert triggered for rule {RuleName}", rule.Name);
                break;
                
            case AlertActionType.Webhook:
                // Implementation would call webhook
                _logger.LogInformation("Webhook alert triggered for rule {RuleName}", rule.Name);
                break;
                
            case AlertActionType.Database:
                // Store alert in database
                await LogEventAsync("AlertTriggered", "Alert", "AuditAlert", true,
                    new Dictionary<string, object>
                    {
                        ["RuleName"] = rule.Name,
                        ["RuleId"] = rule.Id,
                        ["TriggeringEventId"] = entry.Id,
                        ["AlertSeverity"] = rule.AlertSeverity.ToString()
                    });
                break;
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Supporting service interfaces
/// </summary>
public interface IAuditStorage
{
    Task StoreAuditEntryAsync(AuditLogEntry entry);
    Task<List<AuditLogEntry>> SearchAuditEntriesAsync(AuditSearchRequest request);
    Task<int> CountAuditEntriesAsync(AuditSearchRequest request);
    Task StoreDataLineageEntryAsync(DataLineageEntry entry);
    Task<List<DataLineageEntry>> GetDataLineageEntriesAsync(string dataType, string? dataId);
    Task<List<DataLineageEntry>> TraceDataFlowAsync(string dataId, DateTime? fromDate);
    Task StoreComplianceAuditEntryAsync(ComplianceAuditEntry entry);
    Task<List<ComplianceAuditEntry>> GetComplianceAuditEntriesAsync(ComplianceFramework framework, DateTime? startDate, DateTime? endDate);
    Task<List<ComplianceViolationDto>> GetComplianceViolationsAsync(ComplianceFramework framework, ComplianceStatus? status);
    Task<AuditConfiguration> GetAuditConfigurationAsync();
    Task UpdateAuditConfigurationAsync(AuditConfiguration config);
    Task<List<AuditAlertRule>> GetAlertRulesAsync();
    Task StoreAlertRuleAsync(AuditAlertRule rule);
    Task UpdateAlertRuleAsync(AuditAlertRule rule);
    Task DeleteAlertRuleAsync(Guid ruleId);
    Task ArchiveAuditLogsAsync(DateTime beforeDate);
    Task PurgeAuditLogsAsync(DateTime beforeDate);
}

public interface IAuditIntegrityService
{
    Task<string> CalculateHashAsync(AuditLogEntry entry);
    Task<bool> VerifyEntryIntegrityAsync(Guid entryId);
    Task<bool> VerifyTrailIntegrityAsync(DateTime startDate, DateTime endDate);
}

public interface IComplianceReportGenerator
{
    Task<ComplianceReportResponse> GenerateReportAsync(ComplianceReportRequest request, List<ComplianceAuditEntry> entries);
    Task<byte[]> ExportAuditLogsAsync(List<AuditLogEntry> entries, ReportFormat format);
}