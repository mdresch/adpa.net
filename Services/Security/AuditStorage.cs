using Microsoft.EntityFrameworkCore;
using ADPA.Data;
using ADPA.Models.Entities;
using ADPA.Models.DTOs;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
// Use alias to disambiguate ComplianceViolation between DTOs and Entities
using ComplianceViolationDto = ADPA.Models.DTOs.ComplianceViolation;

namespace ADPA.Services.Security;

/// <summary>
/// Phase 5.4: Audit Storage Implementation
/// Entity Framework Core implementation for audit data persistence
/// </summary>
public class AuditStorage : IAuditStorage
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<AuditStorage> _logger;

    public AuditStorage(AdpaEfDbContext context, ILogger<AuditStorage> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task StoreAuditEntryAsync(AuditLogEntry entry)
    {
        try
        {
            _context.Set<AuditLogEntry>().Add(entry);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store audit entry {Id}", entry.Id);
            throw;
        }
    }

    public async Task<List<AuditLogEntry>> SearchAuditEntriesAsync(AuditSearchRequest request)
    {
        var query = _context.Set<AuditLogEntry>().AsQueryable();

        // Apply filters
        if (request.StartDate.HasValue)
            query = query.Where(e => e.Timestamp >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(e => e.Timestamp <= request.EndDate.Value);

        if (request.EventTypes.Any())
            query = query.Where(e => request.EventTypes.Contains(e.EventType));

        if (request.Severities.Any())
            query = query.Where(e => request.Severities.Contains(e.Severity));

        if (request.UserIds.Any())
        {
            var userGuids = request.UserIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .ToList();
            if (userGuids.Any())
                query = query.Where(e => e.UserId.HasValue && userGuids.Contains(e.UserId.Value));
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(e => 
                e.Description.Contains(request.SearchTerm) ||
                e.Action.Contains(request.SearchTerm) ||
                e.Resource.Contains(request.SearchTerm) ||
                e.UserName.Contains(request.SearchTerm));
        }

        if (request.Resources.Any())
            query = query.Where(e => request.Resources.Contains(e.Resource));

        if (request.WasSuccessful.HasValue)
            query = query.Where(e => e.WasSuccessful == request.WasSuccessful.Value);

        // Apply sorting
        query = request.SortBy switch
        {
            "Timestamp" => request.Descending ? query.OrderByDescending(e => e.Timestamp) : query.OrderBy(e => e.Timestamp),
            "EventType" => request.Descending ? query.OrderByDescending(e => e.EventType) : query.OrderBy(e => e.EventType),
            "Severity" => request.Descending ? query.OrderByDescending(e => e.Severity) : query.OrderBy(e => e.Severity),
            "UserName" => request.Descending ? query.OrderByDescending(e => e.UserName) : query.OrderBy(e => e.UserName),
            _ => query.OrderByDescending(e => e.Timestamp)
        };

        // Apply pagination
        return await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
    }

    public async Task<int> CountAuditEntriesAsync(AuditSearchRequest request)
    {
        var query = _context.Set<AuditLogEntry>().AsQueryable();

        // Apply same filters as SearchAuditEntriesAsync
        if (request.StartDate.HasValue)
            query = query.Where(e => e.Timestamp >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(e => e.Timestamp <= request.EndDate.Value);

        if (request.EventTypes.Any())
            query = query.Where(e => request.EventTypes.Contains(e.EventType));

        if (request.Severities.Any())
            query = query.Where(e => request.Severities.Contains(e.Severity));

        if (request.UserIds.Any())
        {
            var userGuids = request.UserIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .ToList();
            if (userGuids.Any())
                query = query.Where(e => e.UserId.HasValue && userGuids.Contains(e.UserId.Value));
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(e => 
                e.Description.Contains(request.SearchTerm) ||
                e.Action.Contains(request.SearchTerm) ||
                e.Resource.Contains(request.SearchTerm) ||
                e.UserName.Contains(request.SearchTerm));
        }

        if (request.Resources.Any())
            query = query.Where(e => request.Resources.Contains(e.Resource));

        if (request.WasSuccessful.HasValue)
            query = query.Where(e => e.WasSuccessful == request.WasSuccessful.Value);

        return await query.CountAsync();
    }

    public async Task StoreDataLineageEntryAsync(DataLineageEntry entry)
    {
        try
        {
            _context.Set<DataLineageEntry>().Add(entry);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store data lineage entry {Id}", entry.Id);
            throw;
        }
    }

    public async Task<List<DataLineageEntry>> GetDataLineageEntriesAsync(string dataType, string? dataId)
    {
        var query = _context.Set<DataLineageEntry>()
            .Where(e => e.DataType == dataType);

        if (!string.IsNullOrEmpty(dataId))
            query = query.Where(e => e.DataId == dataId);

        return await query
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<List<DataLineageEntry>> TraceDataFlowAsync(string dataId, DateTime? fromDate)
    {
        var query = _context.Set<DataLineageEntry>()
            .Where(e => e.DataId == dataId);

        if (fromDate.HasValue)
            query = query.Where(e => e.Timestamp >= fromDate.Value);

        return await query
            .OrderBy(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task StoreComplianceAuditEntryAsync(ComplianceAuditEntry entry)
    {
        try
        {
            _context.Set<ComplianceAuditEntry>().Add(entry);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store compliance audit entry {Id}", entry.Id);
            throw;
        }
    }

    public async Task<List<ComplianceAuditEntry>> GetComplianceAuditEntriesAsync(ComplianceFramework framework, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Set<ComplianceAuditEntry>()
            .Where(e => e.Framework == framework);

        if (startDate.HasValue)
            query = query.Where(e => e.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ComplianceViolationDto>> GetComplianceViolationsAsync(ComplianceFramework framework, ComplianceStatus? status)
    {
        var auditEntries = await _context.Set<ComplianceAuditEntry>()
            .Where(e => e.Framework == framework && e.Status != ComplianceStatus.Compliant)
            .ToListAsync();

        var violations = auditEntries.Select(e => new ComplianceViolationDto
        {
            Id = e.Id,
            Framework = e.Framework,
            Control = e.ControlId,
            Requirement = e.Requirement,
            Severity = (ComplianceSeverity)e.Severity,
            Description = e.Description,
            DetectedAt = e.Timestamp,
            Status = e.Status,
            RemediationPlan = e.RemediationAction,
            RemediationDeadline = e.RemediationDate
        }).ToList();

        if (status.HasValue)
            violations = violations.Where(v => v.Status == status.Value).ToList();

        return violations;
    }

    public async Task<AuditConfiguration> GetAuditConfigurationAsync()
    {
        var config = await _context.Set<AuditConfiguration>().FirstOrDefaultAsync();
        
        if (config == null)
        {
            config = new AuditConfiguration
            {
                Name = "Default Audit Configuration",
                IsEnabled = true,
                MinimumSeverity = AuditSeverity.Information,
                RetentionPeriod = TimeSpan.FromDays(2555), // 7 years
                StorageType = AuditStorageType.Database
            };
            
            _context.Set<AuditConfiguration>().Add(config);
            await _context.SaveChangesAsync();
        }

        return config;
    }

    public async Task UpdateAuditConfigurationAsync(AuditConfiguration config)
    {
        _context.Set<AuditConfiguration>().Update(config);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditAlertRule>> GetAlertRulesAsync()
    {
        return await _context.Set<AuditAlertRule>()
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task StoreAlertRuleAsync(AuditAlertRule rule)
    {
        _context.Set<AuditAlertRule>().Add(rule);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAlertRuleAsync(AuditAlertRule rule)
    {
        _context.Set<AuditAlertRule>().Update(rule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAlertRuleAsync(Guid ruleId)
    {
        var rule = await _context.Set<AuditAlertRule>().FindAsync(ruleId);
        if (rule != null)
        {
            _context.Set<AuditAlertRule>().Remove(rule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ArchiveAuditLogsAsync(DateTime beforeDate)
    {
        var logsToArchive = await _context.Set<AuditLogEntry>()
            .Where(e => e.Timestamp < beforeDate)
            .ToListAsync();

        // In a full implementation, this would move logs to archive storage
        // For now, we'll just mark them as archived in metadata
        foreach (var log in logsToArchive)
        {
            var metadata = log.GetMetadata();
            metadata["Archived"] = true;
            metadata["ArchivedAt"] = DateTime.UtcNow;
            log.SetMetadata(metadata);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Archived {Count} audit log entries before {Date}", 
            logsToArchive.Count, beforeDate);
    }

    public async Task PurgeAuditLogsAsync(DateTime beforeDate)
    {
        var logsToPurge = await _context.Set<AuditLogEntry>()
            .Where(e => e.Timestamp < beforeDate)
            .ToListAsync();

        _context.Set<AuditLogEntry>().RemoveRange(logsToPurge);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Purged {Count} audit log entries before {Date}", 
            logsToPurge.Count, beforeDate);
    }
}

/// <summary>
/// Audit integrity service for ensuring log tamper-proofing
/// </summary>
public class AuditIntegrityService : IAuditIntegrityService
{
    private readonly AdpaEfDbContext _context;
    private readonly ILogger<AuditIntegrityService> _logger;
    private readonly string _integrityKey;

    public AuditIntegrityService(AdpaEfDbContext context, ILogger<AuditIntegrityService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _integrityKey = configuration["AuditIntegrity:Key"] ?? "DefaultAuditIntegrityKey2024!";
    }

    public async Task<string> CalculateHashAsync(AuditLogEntry entry)
    {
        try
        {
            var data = new
            {
                entry.Id,
                entry.Timestamp,
                entry.EventType,
                entry.Category,
                entry.Action,
                entry.Resource,
                entry.ResourceId,
                entry.UserId,
                entry.UserName,
                entry.IpAddress,
                entry.WasSuccessful,
                entry.Description,
                Metadata = JsonSerializer.Serialize(entry.Metadata),
                BeforeState = JsonSerializer.Serialize(entry.BeforeState),
                AfterState = JsonSerializer.Serialize(entry.AfterState)
            };

            var json = JsonSerializer.Serialize(data);
            var keyBytes = Encoding.UTF8.GetBytes(_integrityKey);
            var dataBytes = Encoding.UTF8.GetBytes(json);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            
            return Convert.ToBase64String(hashBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate hash for audit entry {Id}", entry.Id);
            throw;
        }
    }

    public async Task<bool> VerifyEntryIntegrityAsync(Guid entryId)
    {
        try
        {
            var entry = await _context.Set<AuditLogEntry>()
                .FirstOrDefaultAsync(e => e.Id == entryId);

            if (entry == null)
            {
                _logger.LogWarning("Audit entry {Id} not found for integrity verification", entryId);
                return false;
            }

            var metadata = entry.GetMetadata();
            if (!metadata.TryGetValue("IntegrityHash", out var storedHashObj))
            {
                _logger.LogWarning("No integrity hash found for audit entry {Id}", entryId);
                return false;
            }

            var storedHash = storedHashObj.ToString();
            
            // Temporarily remove hash to recalculate
            var originalHash = metadata["IntegrityHash"];
            metadata.Remove("IntegrityHash");
            entry.SetMetadata(metadata);
            
            var calculatedHash = await CalculateHashAsync(entry);
            
            // Restore hash
            metadata["IntegrityHash"] = originalHash;
            entry.SetMetadata(metadata);
            
            var isValid = storedHash == calculatedHash;
            
            if (!isValid)
            {
                _logger.LogWarning("Integrity verification failed for audit entry {Id}. Expected: {Expected}, Calculated: {Calculated}", 
                    entryId, storedHash, calculatedHash);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify integrity for audit entry {Id}", entryId);
            return false;
        }
    }

    public async Task<bool> VerifyTrailIntegrityAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var entries = await _context.Set<AuditLogEntry>()
                .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
                .OrderBy(e => e.Timestamp)
                .ToListAsync();

            var totalEntries = entries.Count;
            var validEntries = 0;

            foreach (var entry in entries)
            {
                if (await VerifyEntryIntegrityAsync(entry.Id))
                    validEntries++;
            }

            var integrityPercentage = totalEntries > 0 ? (double)validEntries / totalEntries : 1.0;
            var isValid = integrityPercentage >= 0.99; // 99% threshold

            _logger.LogInformation("Audit trail integrity verification: {ValidEntries}/{TotalEntries} ({Percentage:P2}) - {Result}",
                validEntries, totalEntries, integrityPercentage, isValid ? "PASSED" : "FAILED");

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify audit trail integrity from {StartDate} to {EndDate}", startDate, endDate);
            return false;
        }
    }
}

/// <summary>
/// Compliance report generator for regulatory reporting
/// </summary>
public class ComplianceReportGenerator : IComplianceReportGenerator
{
    private readonly ILogger<ComplianceReportGenerator> _logger;

    public ComplianceReportGenerator(ILogger<ComplianceReportGenerator> logger)
    {
        _logger = logger;
    }

    public async Task<ComplianceReportResponse> GenerateReportAsync(ComplianceReportRequest request, List<ComplianceAuditEntry> entries)
    {
        try
        {
            var response = new ComplianceReportResponse
            {
                Framework = request.Framework,
                GeneratedAt = DateTime.UtcNow,
                ReportPeriod = $"{(request.EndDate - request.StartDate).Days} days"
            };

            // Analyze compliance status by control
            var controlGroups = entries.GroupBy(e => e.ControlId);
            
            foreach (var group in controlGroups)
            {
                var controlEntries = group.ToList();
                var totalChecks = controlEntries.Count;
                var passedChecks = controlEntries.Count(e => e.Status == ComplianceStatus.Compliant);
                var failedChecks = totalChecks - passedChecks;
                
                response.ControlStatuses[group.Key] = new ADPA.Models.DTOs.ComplianceControlStatus
                {
                    ControlId = group.Key,
                    ControlName = controlEntries.First().ControlDescription,
                    Status = failedChecks == 0 ? ComplianceStatus.Compliant : 
                             passedChecks > failedChecks ? ComplianceStatus.PartiallyCompliant : 
                             ComplianceStatus.NonCompliant,
                    TotalChecks = totalChecks,
                    PassedChecks = passedChecks,
                    FailedChecks = failedChecks,
                    CompliancePercentage = totalChecks > 0 ? (double)passedChecks / totalChecks * 100 : 100,
                    LastAssessed = controlEntries.Max(e => e.Timestamp)
                };
            }

            // Determine overall status
            var allCompliant = response.ControlStatuses.Values.All(c => c.Status == ComplianceStatus.Compliant);
            var mostlyCompliant = response.ControlStatuses.Values.Count(c => c.Status == ComplianceStatus.Compliant) > 
                                 response.ControlStatuses.Values.Count / 2;

            response.OverallStatus = allCompliant ? ComplianceOverallStatus.FullyCompliant :
                                   mostlyCompliant ? ComplianceOverallStatus.MostlyCompliant :
                                   ComplianceOverallStatus.PartiallyCompliant;

            // Generate violations list
            response.Violations = entries
                .Where(e => e.Status != ComplianceStatus.Compliant)
                .Select(e => new ComplianceViolationDto
                {
                    Id = e.Id,
                    Framework = e.Framework,
                    Control = e.ControlId,
                    Requirement = e.Requirement,
                    Severity = (ComplianceSeverity)e.Severity,
                    Description = e.Description,
                    DetectedAt = e.Timestamp,
                    Status = e.Status,
                    RemediationPlan = e.RemediationAction,
                    RemediationDeadline = e.RemediationDate
                })
                .ToList();

            // Generate report data based on format
            response.ReportData = request.Format switch
            {
                ReportFormat.JSON => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })),
                ReportFormat.CSV => await GenerateCsvReportAsync(response),
                ReportFormat.PDF => await GeneratePdfReportAsync(response),
                _ => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response))
            };

            _logger.LogInformation("Generated compliance report for {Framework} covering period {Start} to {End}",
                request.Framework, request.StartDate, request.EndDate);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate compliance report for {Framework}", request.Framework);
            throw;
        }
    }

    public async Task<byte[]> ExportAuditLogsAsync(List<AuditLogEntry> entries, ReportFormat format)
    {
        try
        {
            return format switch
            {
                ReportFormat.JSON => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true })),
                ReportFormat.CSV => await GenerateAuditLogsCsvAsync(entries),
                ReportFormat.Excel => await GenerateAuditLogsExcelAsync(entries),
                _ => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entries))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export audit logs in format {Format}", format);
            throw;
        }
    }

    private async Task<byte[]> GenerateCsvReportAsync(ComplianceReportResponse response)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Control ID,Control Name,Status,Total Checks,Passed Checks,Failed Checks,Compliance %,Last Assessed");
        
        foreach (var control in response.ControlStatuses.Values)
        {
            csv.AppendLine($"{control.ControlId},{control.ControlName},{control.Status}," +
                          $"{control.TotalChecks},{control.PassedChecks},{control.FailedChecks}," +
                          $"{control.CompliancePercentage:F2},{control.LastAssessed:yyyy-MM-dd HH:mm:ss}");
        }

        csv.AppendLine();
        csv.AppendLine("Violations:");
        csv.AppendLine("Control ID,Control Name,Violation Type,Description,Severity,Detected At,Status");
        
        foreach (var violation in response.Violations)
        {
            csv.AppendLine($"{violation.Id},{violation.Control},{violation.Framework}," +
                          $"\"{violation.Description}\",{violation.Severity},{violation.DetectedAt:yyyy-MM-dd HH:mm:ss},{violation.Status}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private async Task<byte[]> GeneratePdfReportAsync(ComplianceReportResponse response)
    {
        // In a full implementation, this would generate a proper PDF report
        // For now, return formatted text that could be converted to PDF
        var content = new StringBuilder();
        content.AppendLine($"Compliance Report - {response.Framework}");
        content.AppendLine($"Generated: {response.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        content.AppendLine($"Report Period: {response.ReportPeriod}");
        content.AppendLine($"Overall Status: {response.OverallStatus}");
        content.AppendLine();
        
        content.AppendLine("Control Status Summary:");
        foreach (var control in response.ControlStatuses.Values)
        {
            content.AppendLine($"- {control.ControlId}: {control.Status} ({control.CompliancePercentage:F1}%)");
        }
        
        content.AppendLine();
        content.AppendLine($"Total Violations: {response.Violations.Count}");
        
        return Encoding.UTF8.GetBytes(content.ToString());
    }

    private async Task<byte[]> GenerateAuditLogsCsvAsync(List<AuditLogEntry> entries)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,Event Type,Category,Action,Resource,User,IP Address,Success,Description");
        
        foreach (var entry in entries)
        {
            csv.AppendLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss},{entry.EventType},{entry.Category}," +
                          $"{entry.Action},{entry.Resource},{entry.UserName},{entry.IpAddress}," +
                          $"{entry.WasSuccessful},\"{entry.Description}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private async Task<byte[]> GenerateAuditLogsExcelAsync(List<AuditLogEntry> entries)
    {
        // In a full implementation, this would generate an actual Excel file
        // For now, return CSV format that Excel can open
        return await GenerateAuditLogsCsvAsync(entries);
    }
}