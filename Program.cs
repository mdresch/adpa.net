using ADPA.Services;
using ADPA.Services.Intelligence;
using ADPA.Services.Notifications;
using ADPA.Services.Batch;
using ADPA.Data.Repositories;
using ADPA.Data;
using ADPA.Middleware;
using ADPA.Authentication;
using ADPA.Hubs;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
// Add API documentation support
builder.Services.AddEndpointsApiExplorer();
// Register Entity Framework Core 10 DbContext
builder.Services.AddDbContext<AdpaEfDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\MSSQLLocalDB;Database=AdpaDb;Trusted_Connection=true;MultipleActiveResultSets=true;";
    options.UseSqlServer(connectionString);
});
// Register EF Core repositories  
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IDocumentRepository, EfDocumentRepository>();
builder.Services.AddScoped<EfProcessingResultRepository>();
// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
builder.Services.AddScoped<IAdvancedDocumentProcessor, AdvancedDocumentProcessor>(); // Phase 2 Enhanced Processing
builder.Services.AddSingleton<IDataProcessingService, DataProcessingService>(); // Legacy service
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>(); // Changed from Singleton to Scoped
// Register Phase 3 Intelligence Services
builder.Services.AddScoped<IIntelligenceService, IntelligenceService>();
builder.Services.AddScoped<IOcrService, TesseractOcrService>();
builder.Services.AddScoped<IDocumentClassificationService, MlNetDocumentClassificationService>();
builder.Services.AddScoped<ITextAnalysisService, AdvancedTextAnalysisService>();
// Phase 4: Register Real-time Notification Services
builder.Services.AddScoped<INotificationService, NotificationService>();
// Phase 4: Register Batch Processing Services
builder.Services.AddScoped<IBatchProcessingService, BatchProcessingService>();
// Phase 4: Register Document Comparison Services
builder.Services.AddScoped<ADPA.Services.Comparison.IDocumentComparisonService, ADPA.Services.Comparison.DocumentComparisonService>();
// Phase 4: Register Workflow Services
builder.Services.AddScoped<ADPA.Services.Workflow.IWorkflowService, ADPA.Services.Workflow.WorkflowService>();
// Phase 3: Register Analytics & Reporting Services
builder.Services.AddScoped<ADPA.Services.Analytics.IAnalyticsService, ADPA.Services.Analytics.AnalyticsService>();
builder.Services.AddScoped<ADPA.Services.Reporting.IReportingService, ADPA.Services.Reporting.ReportingService>();
// Phase 5.4: Register Comprehensive Audit System
builder.Services.AddScoped<ADPA.Services.Security.IAuditService, ADPA.Services.Security.AuditService>();
builder.Services.AddScoped<ADPA.Services.Security.IAuditStorage, ADPA.Services.Security.AuditStorage>();
builder.Services.AddScoped<ADPA.Services.Security.IAuditIntegrityService, ADPA.Services.Security.AuditIntegrityService>();
builder.Services.AddScoped<ADPA.Services.Security.IComplianceReportGenerator, ADPA.Services.Security.ComplianceReportGenerator>();
// Phase 5.5: Register Security Monitoring & Threat Detection
builder.Services.AddScoped<ADPA.Services.Security.ISecurityMonitoringService, ADPA.Services.Security.SecurityMonitoringService>();
builder.Services.Configure<ADPA.Services.Security.SecurityMonitoringConfiguration>(
    builder.Configuration.GetSection("SecurityMonitoring"));
// Configure default security monitoring settings if not provided in config
builder.Services.PostConfigure<ADPA.Services.Security.SecurityMonitoringConfiguration>(config =>
{
    // Set default values if not configured
    if (config.DefaultAnomalySensitivity == 0)
        config.DefaultAnomalySensitivity = 80.0;
    if (config.MaxIncidentsPerDay == 0)
        config.MaxIncidentsPerDay = 1000;
    if (config.MaxAlertsPerHour == 0)
        config.MaxAlertsPerHour = 100;
});
// Phase 5.6: Register Compliance Framework Integration
builder.Services.AddScoped<ADPA.Services.Compliance.IComplianceService, ADPA.Services.Compliance.ComplianceService>();
builder.Services.Configure<ADPA.Models.Entities.ComplianceConfiguration>(
    builder.Configuration.GetSection("Compliance"));
// Configure default compliance settings if not provided in config
builder.Services.PostConfigure<ADPA.Models.Entities.ComplianceConfiguration>(config =>
{
    // Set default values if not configured
    if (config.DataSubjectRequestTimeoutDays == 0)
        config.DataSubjectRequestTimeoutDays = 30;
    if (config.BreachNotificationHours == 0)
        config.BreachNotificationHours = 72;
    if (config.ComplianceAssessmentFrequencyDays == 0)
        config.ComplianceAssessmentFrequencyDays = 365;
    if (config.DataRetentionCheckFrequencyDays == 0)
        config.DataRetentionCheckFrequencyDays = 30;
    if (config.FrameworkSettings == null)
        config.FrameworkSettings = new Dictionary<string, string>();
});
// Phase 5.7: Register API Security Enhancements
builder.Services.AddScoped<ADPA.Services.Security.IAPISecurityService, ADPA.Services.Security.APISecurityService>();
builder.Services.Configure<ADPA.Models.Entities.APISecurityConfiguration>(
    builder.Configuration.GetSection("APISecurity"));
// Configure default API security settings if not provided in config
builder.Services.PostConfigure<ADPA.Models.Entities.APISecurityConfiguration>(config =>
{
    // Set default values if not configured
    if (config.DefaultRateLimit == 0)
        config.DefaultRateLimit = 1000;
    if (config.DefaultTimeWindow == 0)
        config.DefaultTimeWindow = 3600; // 1 hour
    if (config.MaxRiskScore == 0)
        config.MaxRiskScore = 100;
    if (config.DefaultSanitizationLevel == null)
        config.DefaultSanitizationLevel = "Standard";
    if (config.DefaultSecurityHeaders == null)
    {
        config.DefaultSecurityHeaders = new Dictionary<string, string>
        {
            { "X-Content-Type-Options", "nosniff" },
            { "X-Frame-Options", "DENY" },
            { "X-XSS-Protection", "1; mode=block" },
            { "Strict-Transport-Security", "max-age=31536000; includeSubDomains" },
            { "Content-Security-Policy", "default-src 'self'" },
            { "Referrer-Policy", "strict-origin-when-cross-origin" }
        };
    }
    if (string.IsNullOrEmpty(config.DefaultCORSPolicy))
    {
        config.DefaultCORSPolicy = "DefaultPolicy";
    }
    if (config.BlockedCountries == null)
        config.BlockedCountries = string.Empty;
    if (config.AllowedCountries == null)
        config.AllowedCountries = Array.Empty<string>();
});
// Configure Authentication & Authorization
builder.Services.AddAuthentication("Bearer")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, JwtAuthenticationHandler>(
        "Bearer", options => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("role", "Admin"));
});
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    // SignalR-specific CORS policy
    options.AddPolicy("SignalRPolicy",
        policy =>
        {
            policy.WithOrigins("https://localhost:7050", "http://localhost:5050")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Required for SignalR
        });
});
// Phase 4: Add SignalR for real-time notifications
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}
// Add global exception handling
app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();

// Configure default files (index.html)
app.UseDefaultFiles();
app.UseStaticFiles(); // Enable static file serving for UI

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
// Add custom middleware for request logging
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    logger.LogInformation("🌐 {Method} {Path} - Request started", 
        context.Request.Method, context.Request.Path);
    await next();
    stopwatch.Stop();
    logger.LogInformation("✅ {Method} {Path} - {StatusCode} in {ElapsedMs}ms", 
        context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
});

// Map Controllers
app.MapControllers();

// Map dashboard route - serve index.html for root and dashboard paths
app.MapFallbackToFile("index.html");

// Phase 4: Map SignalR Hub
app.MapHub<ProcessingHub>("/hubs/processing");
Console.WriteLine("🚀 ADPA Phase 3: Analytics & Reporting - Business Intelligence! ✨");
Console.WriteLine("===============================================");
Console.WriteLine($"Server running at: https://localhost:7050");
Console.WriteLine();
Console.WriteLine("📋 Available API Endpoints:");
Console.WriteLine();
Console.WriteLine("🔐 Authentication:");
Console.WriteLine("  POST /api/auth/register - Register new user");
Console.WriteLine("  POST /api/auth/login - User login");
Console.WriteLine("  GET /api/auth/user/{id} - Get user info");
Console.WriteLine("  POST /api/auth/validate - Validate token");
Console.WriteLine();
Console.WriteLine("📄 Document Management:");
Console.WriteLine("  POST /api/documents/upload - Upload document");
Console.WriteLine("  GET /api/documents - Get all documents");
Console.WriteLine("  GET /api/documents/{id} - Get specific document");
Console.WriteLine("  GET /api/documents/my-documents - Get user documents");
Console.WriteLine("  DELETE /api/documents/{id} - Delete document");
Console.WriteLine("  POST /api/documents/process-pending - Process pending docs");
Console.WriteLine();
Console.WriteLine("🚀 Phase 2: Advanced Processing:");
Console.WriteLine("  POST /api/advancedprocessing/{id}/process-advanced - Enhanced processing");
Console.WriteLine("  GET /api/advancedprocessing/supported-formats - Supported formats");
Console.WriteLine();
Console.WriteLine("⚡ System:");
Console.WriteLine("  GET /api/health - Health check");
Console.WriteLine();
Console.WriteLine("📊 Legacy Data API (for backward compatibility):");
Console.WriteLine("  GET /api/data - Get processed data");
Console.WriteLine("  POST /api/data - Submit data for processing");
Console.WriteLine();
Console.WriteLine("🎉 ADPA .NET - Phase 3: Analytics & Reporting Complete!");
Console.WriteLine("✅ Phase 1, 2, 3 & 4 Features Implemented:");
Console.WriteLine("  ✅ User Management & Authentication");
Console.WriteLine("  ✅ Document Upload & Processing");
Console.WriteLine("  ✅ Repository Pattern Architecture");
Console.WriteLine("  ✅ Enhanced Error Handling");
Console.WriteLine("  ✅ Async Processing Pipeline");
Console.WriteLine("  ✅ Duplicate Detection");
Console.WriteLine("  ✅ Multi-format Support");
Console.WriteLine("  ✅ Advanced Document Processing (Word, PDF, Text, CSV)");
Console.WriteLine("  ✅ Metadata Extraction & Analysis");
Console.WriteLine("  🧠 OCR Text Extraction (Tesseract)");
Console.WriteLine("  🏷️ ML-based Document Classification");
Console.WriteLine("  🔍 Language Detection & Text Analysis");
Console.WriteLine("  🎯 Entity Extraction & Sentiment Analysis");
Console.WriteLine("  📊 Comprehensive Intelligence Analysis");
Console.WriteLine("  ✅ Confidence Scoring & Performance Metrics");
Console.WriteLine("  🔄 Real-time Processing Notifications (SignalR)");
Console.WriteLine("  📦 Batch Upload & Processing");
Console.WriteLine("  ⚡ Parallel Document Processing");
Console.WriteLine("  🔍 Intelligent Document Comparison");
Console.WriteLine("  🔄 Workflow Automation & Routing");
Console.WriteLine("  📊 Similarity Analysis & Statistics");
Console.WriteLine("  📈 Comprehensive Analytics Engine");
Console.WriteLine("  🎯 Performance & Metrics Tracking");
Console.WriteLine("  📊 Real-time System Monitoring");
Console.WriteLine("  🔮 Predictive Analytics & Trends");
Console.WriteLine("  📋 Custom KPIs & Business Intelligence");
Console.WriteLine("  📄 Advanced Reporting System");
Console.WriteLine("  📧 Automated Report Distribution");
Console.WriteLine("  ⏰ Scheduled Report Generation");
Console.WriteLine();
Console.WriteLine("🎯 Test the Intelligence API:");
Console.WriteLine("  Intelligence Capabilities: GET /api/intelligence/capabilities");
Console.WriteLine("  OCR Processing: POST /api/intelligence/ocr");
Console.WriteLine("  Document Classification: POST /api/intelligence/classify");
Console.WriteLine("  Text Analysis: POST /api/intelligence/analyze-text");
Console.WriteLine("  Full Intelligence Analysis: POST /api/intelligence/{id}/analyze");
Console.WriteLine();
Console.WriteLine("📊 Test the Analytics API:");
Console.WriteLine("  Analytics Summary: GET /api/analytics/summary");
Console.WriteLine("  Real-time Metrics: GET /api/analytics/realtime");
Console.WriteLine("  Performance Analytics: GET /api/analytics/performance");
Console.WriteLine("  Trend Analysis: GET /api/analytics/trends?trendType=DocumentVolume");
Console.WriteLine("  Predictive Analytics: GET /api/analytics/predictions");
Console.WriteLine("  Custom KPIs: GET /api/analytics/custom-kpis");
Console.WriteLine();
Console.WriteLine("📋 Test the Reporting API:");
Console.WriteLine("  Generate PDF Report: GET /api/reports/pdf?reportType=Executive");
Console.WriteLine("  Generate Excel Report: GET /api/reports/excel?reportType=Operational");
Console.WriteLine("  Report Templates: GET /api/reports/templates");
Console.WriteLine("  Scheduled Reports: GET /api/reports/scheduled");
Console.WriteLine("  Report History: GET /api/reports/history");
Console.WriteLine();
Console.WriteLine("🚀 Phase 4: Advanced Features API:");
Console.WriteLine("  Real-time Hub: /hubs/processing (SignalR)");
Console.WriteLine("  Batch Upload: POST /api/batch/upload");
Console.WriteLine("  Create Batch Job: POST /api/batch/create-job");
Console.WriteLine("  Start Batch Processing: POST /api/batch/{id}/start");
Console.WriteLine("  Batch Job Status: GET /api/batch/{id}");
Console.WriteLine("  My Batch Jobs: GET /api/batch/my-jobs");
Console.WriteLine("  Batch Capabilities: GET /api/batch/capabilities");
Console.WriteLine();
Console.WriteLine("🔍 Document Comparison API:");
Console.WriteLine("  Compare Documents: POST /api/comparison/compare-documents");
Console.WriteLine("  Compare Content: POST /api/comparison/compare-content");
Console.WriteLine("  Calculate Similarity: POST /api/comparison/similarity");
Console.WriteLine("  Comparison History: GET /api/comparison/{documentId}/history");
Console.WriteLine("  Comparison Capabilities: GET /api/comparison/capabilities");
Console.WriteLine();
Console.WriteLine("🔄 Workflow Automation API:");
Console.WriteLine("  Create Workflow: POST /api/workflow/create");
Console.WriteLine("  Get Workflow: GET /api/workflow/{workflowId}");
Console.WriteLine("  Document Workflows: GET /api/workflow/document/{documentId}");
Console.WriteLine("  Advance Workflow: POST /api/workflow/{workflowId}/action");
Console.WriteLine("  Cancel Workflow: POST /api/workflow/{workflowId}/cancel");
Console.WriteLine("  Workflow Templates: GET /api/workflow/templates");
Console.WriteLine("  Create Template: POST /api/workflow/templates");
Console.WriteLine("  Workflow Capabilities: GET /api/workflow/capabilities");
Console.WriteLine();
Console.WriteLine("📊 Phase 3: Analytics & Reporting API:");
Console.WriteLine("  Analytics Summary: GET /api/analytics/summary");
Console.WriteLine("  Processing Metrics: GET /api/analytics/processing-metrics");
Console.WriteLine("  Performance Analytics: GET /api/analytics/performance");
Console.WriteLine("  Real-time Metrics: GET /api/analytics/realtime");
Console.WriteLine("  Trend Analysis: GET /api/analytics/trends");
Console.WriteLine("  Predictive Analytics: GET /api/analytics/predictions");
Console.WriteLine("  Custom KPIs: GET /api/analytics/custom-kpis");
Console.WriteLine("  Create KPI: POST /api/analytics/custom-kpis");
Console.WriteLine("  Historical Analysis: GET /api/analytics/historical");
Console.WriteLine("  Processing Events: GET /api/analytics/events");
Console.WriteLine("  Export Analytics: POST /api/analytics/export/excel");
Console.WriteLine("  Analytics Capabilities: GET /api/analytics/capabilities");
Console.WriteLine();
Console.WriteLine("📋 Reporting API:");
Console.WriteLine("  Generate Report: GET /api/reports/analytics");
Console.WriteLine("  PDF Report: GET /api/reports/pdf");
Console.WriteLine("  Excel Report: GET /api/reports/excel");
Console.WriteLine("  Report Templates: GET /api/reports/templates");
Console.WriteLine("  Create Template: POST /api/reports/templates");
Console.WriteLine("  Update Template: PUT /api/reports/templates/{id}");
Console.WriteLine("  Scheduled Reports: GET /api/reports/scheduled");
Console.WriteLine("  Create Scheduled: POST /api/reports/scheduled");
Console.WriteLine("  Report History: GET /api/reports/history");
Console.WriteLine("  Share Report: POST /api/reports/share");
Console.WriteLine("  Shared Reports: GET /api/reports/shared");
Console.WriteLine("  Email Report: POST /api/reports/email");
Console.WriteLine("  Report Distribution: GET /api/reports/distribution/{id}");
Console.WriteLine("  Reporting Capabilities: GET /api/reports/capabilities");
Console.WriteLine();
Console.WriteLine("🔗 Core API Endpoints:");
Console.WriteLine("  Register: curl -X POST https://localhost:7050/api/auth/register");
Console.WriteLine("  Upload: curl -X POST -F 'file=@document.pdf' https://localhost:7050/api/documents/upload");
app.Run();
