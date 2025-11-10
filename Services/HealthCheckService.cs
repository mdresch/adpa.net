using ADPA.Models.DTOs;
using ADPA.Data.Repositories;
using System.Reflection;
using System.Diagnostics;

namespace ADPA.Services;

/// <summary>
/// Interface for health check service
/// </summary>
public interface IHealthCheckService
{
    Task<HealthCheckResponseDto> GetHealthStatusAsync();
}

/// <summary>
/// Service for health check operations
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;

    public HealthCheckService(
        ILogger<HealthCheckService> logger,
        IUserRepository userRepository,
        IDocumentRepository documentRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _documentRepository = documentRepository;
    }

    /// <summary>
    /// Gets the health status of the application
    /// </summary>
    public async Task<HealthCheckResponseDto> GetHealthStatusAsync()
    {
        _logger.LogInformation("Health check requested");

        // Perform comprehensive health checks
        var healthDetails = await PerformHealthChecksAsync();
        var isHealthy = healthDetails.Where(kvp => kvp.Value is bool).All(kvp => (bool)kvp.Value);

        var response = new HealthCheckResponseDto
        {
            Status = isHealthy ? "Healthy" : "Degraded",
            Timestamp = DateTime.UtcNow,
            Version = GetApplicationVersion(),
            Details = healthDetails
        };

        return response;
    }

    /// <summary>
    /// Performs comprehensive health checks
    /// </summary>
    private async Task<Dictionary<string, object>> PerformHealthChecksAsync()
    {
        var healthDetails = new Dictionary<string, object>();
        
        try
        {
            // Check system resources
            healthDetails["SystemMemory"] = await CheckSystemMemoryAsync();
            healthDetails["DiskSpace"] = await CheckDiskSpaceAsync();
            
            // Check repositories
            healthDetails["UserRepository"] = await CheckUserRepositoryAsync();
            healthDetails["DocumentRepository"] = await CheckDocumentRepositoryAsync();
            
            // Check application metrics
            healthDetails["Uptime"] = GetUptimeAsync();
            healthDetails["RequestsPerformed"] = "N/A"; // TODO: Implement request counter
            
            // System information
            healthDetails["FrameworkVersion"] = Environment.Version.ToString();
            healthDetails["MachineName"] = Environment.MachineName;
            healthDetails["ProcessorCount"] = Environment.ProcessorCount;
            healthDetails["WorkingSet"] = GC.GetTotalMemory(false);
            
            return healthDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            healthDetails["Error"] = ex.Message;
            return healthDetails;
        }
    }

    /// <summary>
    /// Check system memory
    /// </summary>
    private async Task<bool> CheckSystemMemoryAsync()
    {
        await Task.CompletedTask;
        var workingSet = GC.GetTotalMemory(false);
        return workingSet < 500 * 1024 * 1024; // Less than 500MB
    }

    /// <summary>
    /// Check available disk space
    /// </summary>
    private async Task<bool> CheckDiskSpaceAsync()
    {
        await Task.CompletedTask;
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory)!);
            return drive.AvailableFreeSpace > 1024 * 1024 * 1024; // More than 1GB free
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check user repository health
    /// </summary>
    private async Task<bool> CheckUserRepositoryAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User repository health check failed");
            return false;
        }
    }

    /// <summary>
    /// Check document repository health
    /// </summary>
    private async Task<bool> CheckDocumentRepositoryAsync()
    {
        try
        {
            var documents = await _documentRepository.GetAllAsync();
            return documents != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Document repository health check failed");
            return false;
        }
    }

    /// <summary>
    /// Get application uptime
    /// </summary>
    private static string GetUptimeAsync()
    {
        var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
    }

    /// <summary>
    /// Gets the application version
    /// </summary>
    private static string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }
}