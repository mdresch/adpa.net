using ADPA.Services.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ADPA.Controllers;

/// <summary>
/// Phase 3: Reporting API Controller
/// Provides comprehensive reporting, template management, and distribution endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportingService reportingService, ILogger<ReportsController> logger)
    {
        _reportingService = reportingService;
        _logger = logger;
    }

    /// <summary>
    /// Generate analytics report for specified period and type
    /// </summary>
    /// <param name="reportType">Type of report to generate</param>
    /// <param name="fromDate">Start date (optional, defaults to 30 days ago)</param>
    /// <param name="toDate">End date (optional, defaults to now)</param>
    /// <returns>Generated report data and metadata</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult<ReportData>> GenerateAnalyticsReportAsync(
        [FromQuery] ReportType reportType = ReportType.Executive,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📊 Analytics report request from user {UserId}: {ReportType} for {FromDate} to {ToDate}", 
                userId, reportType, fromDate?.ToString("yyyy-MM-dd") ?? "30-days-ago", 
                toDate?.ToString("yyyy-MM-dd") ?? "now");

            var report = await _reportingService.GenerateAnalyticsReportAsync(reportType, fromDate, toDate);

            _logger.LogInformation("✅ Analytics report generated: {ReportId} - {ReportType}", 
                report.ReportId, reportType);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate analytics report");
            return StatusCode(500, new { error = "Failed to generate analytics report", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate PDF report for download
    /// </summary>
    /// <param name="reportType">Type of report to generate</param>
    /// <param name="fromDate">Start date (optional, defaults to 30 days ago)</param>
    /// <param name="toDate">End date (optional, defaults to now)</param>
    /// <returns>PDF file for download</returns>
    [HttpGet("pdf")]
    public async Task<IActionResult> GeneratePdfReportAsync(
        [FromQuery] ReportType reportType = ReportType.Executive,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📄 PDF report request from user {UserId}: {ReportType} for {FromDate} to {ToDate}", 
                userId, reportType, fromDate?.ToString("yyyy-MM-dd") ?? "30-days-ago", 
                toDate?.ToString("yyyy-MM-dd") ?? "now");

            var pdfData = await _reportingService.GeneratePdfReportAsync(reportType, fromDate, toDate);

            _logger.LogInformation("✅ PDF report generated: {Size} bytes", pdfData.Length);

            var fileName = $"{reportType}_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            return File(pdfData, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate PDF report");
            return StatusCode(500, new { error = "Failed to generate PDF report", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate Excel report for download
    /// </summary>
    /// <param name="reportType">Type of report to generate</param>
    /// <param name="fromDate">Start date (optional, defaults to 30 days ago)</param>
    /// <param name="toDate">End date (optional, defaults to now)</param>
    /// <returns>Excel file for download</returns>
    [HttpGet("excel")]
    public async Task<IActionResult> GenerateExcelReportAsync(
        [FromQuery] ReportType reportType = ReportType.Executive,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📊 Excel report request from user {UserId}: {ReportType} for {FromDate} to {ToDate}", 
                userId, reportType, fromDate?.ToString("yyyy-MM-dd") ?? "30-days-ago", 
                toDate?.ToString("yyyy-MM-dd") ?? "now");

            var excelData = await _reportingService.GenerateExcelReportAsync(reportType, fromDate, toDate);

            _logger.LogInformation("✅ Excel report generated: {Size} bytes", excelData.Length);

            var fileName = $"{reportType}_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(excelData, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate Excel report");
            return StatusCode(500, new { error = "Failed to generate Excel report", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all report templates available to the user
    /// </summary>
    /// <returns>List of available report templates</returns>
    [HttpGet("templates")]
    public async Task<ActionResult<List<ReportTemplate>>> GetReportTemplatesAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📋 Report templates request from user {UserId}", userId);

            var templates = await _reportingService.GetReportTemplatesAsync(userId);

            _logger.LogInformation("✅ Report templates retrieved: {Count} templates", templates.Count);

            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get report templates");
            return StatusCode(500, new { error = "Failed to get report templates", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new report template
    /// </summary>
    /// <param name="template">Report template definition</param>
    /// <returns>Created report template</returns>
    [HttpPost("templates")]
    public async Task<ActionResult<ReportTemplate>> CreateReportTemplateAsync([FromBody] ReportTemplate template)
    {
        try
        {
            var userId = GetCurrentUserId();
            template.CreatedBy = userId;
            
            _logger.LogInformation("📝 Creating report template '{Name}' for user {UserId}", template.Name, userId);

            if (string.IsNullOrEmpty(template.Name))
            {
                return BadRequest(new { error = "Template name is required" });
            }

            var createdTemplate = await _reportingService.CreateReportTemplateAsync(template);

            _logger.LogInformation("✅ Report template created: {TemplateId} - {Name}", 
                createdTemplate.Id, createdTemplate.Name);

            return Ok(createdTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create report template");
            return StatusCode(500, new { error = "Failed to create report template", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing report template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="template">Updated template data</param>
    /// <returns>Updated report template</returns>
    [HttpPut("templates/{id}")]
    public async Task<ActionResult<ReportTemplate>> UpdateReportTemplateAsync(Guid id, [FromBody] ReportTemplate template)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📝 Updating report template {TemplateId} for user {UserId}", id, userId);

            template.Id = id;
            template.UpdatedBy = userId;
            template.UpdatedDate = DateTime.UtcNow;

            var updatedTemplate = await _reportingService.UpdateReportTemplateAsync(template);

            _logger.LogInformation("✅ Report template updated: {TemplateId} - {Name}", 
                updatedTemplate.Id, updatedTemplate.Name);

            return Ok(updatedTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to update report template");
            return StatusCode(500, new { error = "Failed to update report template", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a report template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("templates/{id}")]
    public async Task<IActionResult> DeleteReportTemplateAsync(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("🗑️ Deleting report template {TemplateId} for user {UserId}", id, userId);

            await _reportingService.DeleteReportTemplateAsync(id);

            _logger.LogInformation("✅ Report template deleted: {TemplateId}", id);

            return Ok(new { message = "Report template deleted successfully", templateId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete report template");
            return StatusCode(500, new { error = "Failed to delete report template", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all scheduled reports for the user
    /// </summary>
    /// <returns>List of scheduled reports</returns>
    [HttpGet("scheduled")]
    public async Task<ActionResult<List<ScheduledReport>>> GetScheduledReportsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("⏰ Scheduled reports request from user {UserId}", userId);

            var scheduledReports = await _reportingService.GetScheduledReportsAsync(userId);

            _logger.LogInformation("✅ Scheduled reports retrieved: {Count} reports", scheduledReports.Count);

            return Ok(scheduledReports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get scheduled reports");
            return StatusCode(500, new { error = "Failed to get scheduled reports", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new scheduled report
    /// </summary>
    /// <param name="scheduledReport">Scheduled report definition</param>
    /// <returns>Created scheduled report</returns>
    [HttpPost("scheduled")]
    public async Task<ActionResult<ScheduledReport>> CreateScheduledReportAsync([FromBody] ScheduledReport scheduledReport)
    {
        try
        {
            var userId = GetCurrentUserId();
            scheduledReport.UserId = userId;
            
            _logger.LogInformation("⏰ Creating scheduled report '{Name}' for user {UserId}", 
                scheduledReport.Name, userId);

            if (string.IsNullOrEmpty(scheduledReport.Name))
            {
                return BadRequest(new { error = "Scheduled report name is required" });
            }

            if (string.IsNullOrEmpty(scheduledReport.CronExpression))
            {
                return BadRequest(new { error = "Cron expression is required" });
            }

            var createdReport = await _reportingService.CreateScheduledReportAsync(scheduledReport);

            _logger.LogInformation("✅ Scheduled report created: {ReportId} - {Name}", 
                createdReport.Id, createdReport.Name);

            return Ok(createdReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create scheduled report");
            return StatusCode(500, new { error = "Failed to create scheduled report", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing scheduled report
    /// </summary>
    /// <param name="id">Scheduled report ID</param>
    /// <param name="scheduledReport">Updated report data</param>
    /// <returns>Updated scheduled report</returns>
    [HttpPut("scheduled/{id}")]
    public async Task<ActionResult<ScheduledReport>> UpdateScheduledReportAsync(Guid id, [FromBody] ScheduledReport scheduledReport)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("⏰ Updating scheduled report {ReportId} for user {UserId}", id, userId);

            scheduledReport.Id = id;

            var updatedReport = await _reportingService.UpdateScheduledReportAsync(scheduledReport);

            _logger.LogInformation("✅ Scheduled report updated: {ReportId} - {Name}", 
                updatedReport.Id, updatedReport.Name);

            return Ok(updatedReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to update scheduled report");
            return StatusCode(500, new { error = "Failed to update scheduled report", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a scheduled report
    /// </summary>
    /// <param name="id">Scheduled report ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("scheduled/{id}")]
    public async Task<IActionResult> DeleteScheduledReportAsync(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("🗑️ Deleting scheduled report {ReportId} for user {UserId}", id, userId);

            await _reportingService.DeleteScheduledReportAsync(id);

            _logger.LogInformation("✅ Scheduled report deleted: {ReportId}", id);

            return Ok(new { message = "Scheduled report deleted successfully", reportId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete scheduled report");
            return StatusCode(500, new { error = "Failed to delete scheduled report", details = ex.Message });
        }
    }

    /// <summary>
    /// Get report history for the user
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of report history</returns>
    [HttpGet("history")]
    public async Task<ActionResult<PagedResult<ReportHistory>>> GetReportHistoryAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📚 Report history request from user {UserId}: page {Page}, size {Size}", 
                userId, pageNumber, pageSize);

            if (pageSize > 100)
            {
                return BadRequest(new { error = "Page size cannot exceed 100" });
            }

            var history = await _reportingService.GetReportHistoryAsync(userId, pageNumber, pageSize);

            _logger.LogInformation("✅ Report history retrieved: {Count} reports", history.Items.Count);

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get report history");
            return StatusCode(500, new { error = "Failed to get report history", details = ex.Message });
        }
    }

    /// <summary>
    /// Share a report with other users
    /// </summary>
    /// <param name="reportShare">Report sharing configuration</param>
    /// <returns>Created report share</returns>
    [HttpPost("share")]
    public async Task<ActionResult<ReportShare>> ShareReportAsync([FromBody] ReportShare reportShare)
    {
        try
        {
            var userId = GetCurrentUserId();
            reportShare.SharedBy = userId;
            
            _logger.LogInformation("🔗 Sharing report {ReportId} by user {UserId} with {UserCount} users", 
                reportShare.ReportId, userId, reportShare.SharedWithUserIds?.Count ?? 0);

            if (reportShare.ReportId == Guid.Empty)
            {
                return BadRequest(new { error = "Report ID is required" });
            }

            var createdShare = await _reportingService.ShareReportAsync(reportShare);

            _logger.LogInformation("✅ Report shared: {ShareId} - {ReportId}", 
                createdShare.Id, createdShare.ReportId);

            return Ok(createdShare);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to share report");
            return StatusCode(500, new { error = "Failed to share report", details = ex.Message });
        }
    }

    /// <summary>
    /// Get reports shared with the user
    /// </summary>
    /// <returns>List of shared reports</returns>
    [HttpGet("shared")]
    public async Task<ActionResult<List<ReportShare>>> GetSharedReportsAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("🔗 Shared reports request from user {UserId}", userId);

            var sharedReports = await _reportingService.GetSharedReportsAsync(userId);

            _logger.LogInformation("✅ Shared reports retrieved: {Count} reports", sharedReports.Count);

            return Ok(sharedReports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get shared reports");
            return StatusCode(500, new { error = "Failed to get shared reports", details = ex.Message });
        }
    }

    /// <summary>
    /// Email a report to specified recipients
    /// </summary>
    /// <param name="emailRequest">Email configuration and recipients</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("email")]
    public async Task<IActionResult> EmailReportAsync([FromBody] ReportEmailRequest emailRequest)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📧 Email report request from user {UserId}: {ReportType} to {RecipientCount} recipients", 
                userId, emailRequest.ReportType, emailRequest.Recipients?.Count ?? 0);

            if (emailRequest.Recipients == null || !emailRequest.Recipients.Any())
            {
                return BadRequest(new { error = "At least one recipient is required" });
            }

            await _reportingService.EmailReportAsync(emailRequest);

            _logger.LogInformation("✅ Report emailed successfully to {RecipientCount} recipients", 
                emailRequest.Recipients.Count);

            return Ok(new { message = "Report emailed successfully", recipientCount = emailRequest.Recipients.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to email report");
            return StatusCode(500, new { error = "Failed to email report", details = ex.Message });
        }
    }

    /// <summary>
    /// Get report distribution status
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <returns>Report distribution information</returns>
    [HttpGet("distribution/{reportId}")]
    public async Task<ActionResult<List<ReportDistribution>>> GetReportDistributionAsync(Guid reportId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("📊 Report distribution request from user {UserId} for report {ReportId}", 
                userId, reportId);

            var distribution = await _reportingService.GetReportDistributionAsync(reportId);

            _logger.LogInformation("✅ Report distribution retrieved: {Count} distributions", distribution.Count);

            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get report distribution");
            return StatusCode(500, new { error = "Failed to get report distribution", details = ex.Message });
        }
    }

    /// <summary>
    /// Get reporting service capabilities and configuration
    /// </summary>
    /// <returns>Reporting service capabilities</returns>
    [HttpGet("capabilities")]
    public ActionResult<object> GetReportingCapabilities()
    {
        try
        {
            var capabilities = new
            {
                Version = "3.0",
                ReportTypes = Enum.GetNames(typeof(ReportType)),
                ExportFormats = new[]
                {
                    "PDF", "Excel", "HTML", "CSV", "JSON"
                },
                Features = new[]
                {
                    "Comprehensive Report Generation",
                    "Template Management and Customization",
                    "Scheduled Report Automation",
                    "Report Sharing and Collaboration",
                    "Email Report Distribution",
                    "Multi-format Export Capabilities",
                    "Report History and Audit Trail",
                    "Dashboard Integration",
                    "Real-time Report Generation",
                    "Custom Report Templates"
                },
                TemplateFeatures = new[]
                {
                    "Custom Layout Design",
                    "Data Source Configuration",
                    "Chart and Graph Templates",
                    "Conditional Formatting",
                    "Header and Footer Customization",
                    "Logo and Branding Integration",
                    "Multi-language Support",
                    "Responsive Design"
                },
                SchedulingOptions = new[]
                {
                    "Cron-based Scheduling",
                    "Recurring Reports (Daily, Weekly, Monthly)",
                    "Custom Time Intervals",
                    "Timezone Support",
                    "Automatic Distribution",
                    "Report Expiration"
                },
                DistributionMethods = new[]
                {
                    "Email Distribution",
                    "Shared Links",
                    "Direct Download",
                    "API Access",
                    "Dashboard Integration"
                },
                Limits = new
                {
                    MaxReportSize = "100 MB",
                    MaxRecipientsPerEmail = 100,
                    MaxScheduledReportsPerUser = 50,
                    MaxTemplatesPerUser = 25,
                    MaxReportHistoryRetention = "365 days"
                }
            };

            return Ok(capabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get reporting capabilities");
            return StatusCode(500, new { error = "Failed to get reporting capabilities", details = ex.Message });
        }
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}

