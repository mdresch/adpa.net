using ADPA.Models.Entities;
using System.Text.Json;

namespace ADPA.Data;

/// <summary>
/// Enhanced in-memory database context that simulates Entity Framework behavior
/// Ready to be replaced with actual EF Core when packages become available
/// </summary>
public class AdpaDbContext : IDisposable
{
    // In-memory collections that simulate database tables
    private readonly Dictionary<Guid, User> _users = new();
    private readonly Dictionary<Guid, Document> _documents = new();
    private readonly Dictionary<Guid, ProcessingResult> _processingResults = new();
    
    // Transaction simulation
    private bool _hasChanges = false;
    private readonly List<object> _changeTracker = new();

    /// <summary>
    /// Users DbSet simulation
    /// </summary>
    public IQueryable<User> Users => _users.Values.AsQueryable();

    /// <summary>
    /// Documents DbSet simulation  
    /// </summary>
    public IQueryable<Document> Documents => _documents.Values.AsQueryable();

    /// <summary>
    /// Processing Results DbSet simulation
    /// </summary>
    public IQueryable<ProcessingResult> ProcessingResults => _processingResults.Values.AsQueryable();

    /// <summary>
    /// Add entity to context (equivalent to DbContext.Add)
    /// </summary>
    public void Add<T>(T entity) where T : class
    {
        switch (entity)
        {
            case User user:
                user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                _users[user.Id] = user;
                break;
                
            case Document document:
                document.Id = document.Id == Guid.Empty ? Guid.NewGuid() : document.Id;
                document.CreatedAt = DateTime.UtcNow;
                _documents[document.Id] = document;
                break;
                
            case ProcessingResult result:
                result.Id = result.Id == Guid.Empty ? Guid.NewGuid() : result.Id;
                result.CreatedAt = DateTime.UtcNow;
                _processingResults[result.Id] = result;
                break;
        }
        
        _changeTracker.Add(entity);
        _hasChanges = true;
    }

    /// <summary>
    /// Update entity in context (equivalent to DbContext.Update)
    /// </summary>
    public void Update<T>(T entity) where T : class
    {
        switch (entity)
        {
            case User user:
                if (_users.ContainsKey(user.Id))
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    _users[user.Id] = user;
                }
                break;
                
            case Document document:
                if (_documents.ContainsKey(document.Id))
                {
                    _documents[document.Id] = document;
                }
                break;
                
            case ProcessingResult result:
                if (_processingResults.ContainsKey(result.Id))
                {
                    _processingResults[result.Id] = result;
                }
                break;
        }
        
        _changeTracker.Add(entity);
        _hasChanges = true;
    }

    /// <summary>
    /// Remove entity from context (equivalent to DbContext.Remove)
    /// </summary>
    public void Remove<T>(T entity) where T : class
    {
        switch (entity)
        {
            case User user:
                _users.Remove(user.Id);
                break;
                
            case Document document:
                _documents.Remove(document.Id);
                break;
                
            case ProcessingResult result:
                _processingResults.Remove(result.Id);
                break;
        }
        
        _changeTracker.Add(entity);
        _hasChanges = true;
    }

    /// <summary>
    /// Save changes to the "database" (equivalent to DbContext.SaveChanges)
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Simulate async operation
        
        if (!_hasChanges)
            return 0;
            
        var changesCount = _changeTracker.Count;
        
        // Simulate database constraints and validation
        await ValidateChangesAsync();
        
        // Clear change tracker
        _changeTracker.Clear();
        _hasChanges = false;
        
        return changesCount;
    }

    /// <summary>
    /// Find entity by ID (equivalent to DbContext.Find)
    /// </summary>
    public async Task<T?> FindAsync<T>(Guid id) where T : class
    {
        await Task.CompletedTask; // Simulate async operation
        
        return typeof(T).Name switch
        {
            nameof(User) => _users.TryGetValue(id, out var user) ? user as T : null,
            nameof(Document) => _documents.TryGetValue(id, out var doc) ? doc as T : null,
            nameof(ProcessingResult) => _processingResults.TryGetValue(id, out var result) ? result as T : null,
            _ => null
        };
    }

    /// <summary>
    /// Seed initial data (equivalent to OnModelCreating seed data)
    /// </summary>
    public async Task SeedDataAsync()
    {
        if (!_users.Any())
        {
            // Seed admin user
            var adminUser = new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Email = "admin@adpa.local",
                PasswordHash = HashPassword("Admin123!"),
                DisplayName = "System Administrator", 
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            Add(adminUser);

            // Seed demo user
            var demoUser = new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Email = "demo@adpa.local", 
                PasswordHash = HashPassword("Demo123!"),
                DisplayName = "Demo User",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            Add(demoUser);
            
            await SaveChangesAsync();
        }
    }

    /// <summary>
    /// Execute raw SQL (simulation for complex queries)
    /// </summary>
    public async Task<IEnumerable<T>> ExecuteRawSqlAsync<T>(string sql, params object[] parameters)
    {
        await Task.CompletedTask; // Simulate async operation
        
        // In a real implementation, this would execute the SQL
        // For now, return empty collection
        return Enumerable.Empty<T>();
    }

    /// <summary>
    /// Begin database transaction (simulation)
    /// </summary>
    public async Task<IDisposable> BeginTransactionAsync()
    {
        await Task.CompletedTask;
        return new TransactionScope();
    }

    /// <summary>
    /// Get database statistics
    /// </summary>
    public async Task<Dictionary<string, object>> GetDatabaseStatsAsync()
    {
        await Task.CompletedTask;
        
        return new Dictionary<string, object>
        {
            ["UsersCount"] = _users.Count,
            ["DocumentsCount"] = _documents.Count,
            ["ProcessingResultsCount"] = _processingResults.Count,
            ["TotalEntities"] = _users.Count + _documents.Count + _processingResults.Count,
            ["HasPendingChanges"] = _hasChanges,
            ["DatabaseProvider"] = "InMemory (EF Core Ready)",
            ["LastUpdate"] = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Validate changes before saving (simulate EF Core validation)
    /// </summary>
    private async Task ValidateChangesAsync()
    {
        await Task.CompletedTask;
        
        // Check for duplicate emails
        var emails = _users.Values.Select(u => u.Email).ToList();
        if (emails.Count != emails.Distinct().Count())
        {
            throw new InvalidOperationException("Duplicate email addresses are not allowed");
        }
        
        // Validate required fields
        foreach (var user in _users.Values)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new InvalidOperationException($"User {user.Id} has missing required fields");
            }
        }
        
        foreach (var document in _documents.Values)
        {
            if (string.IsNullOrEmpty(document.FileName) || string.IsNullOrEmpty(document.ContentType))
            {
                throw new InvalidOperationException($"Document {document.Id} has missing required fields");
            }
        }
    }

    /// <summary>
    /// Simple password hashing (use proper hashing in production)
    /// </summary>
    private static string HashPassword(string password)
    {
        // Simplified password hashing - use proper BCrypt in production
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"hashed_{password}"));
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        _changeTracker.Clear();
        // In real EF Core, this would dispose the context
    }

    /// <summary>
    /// Simple transaction scope for simulation
    /// </summary>
    private class TransactionScope : IDisposable
    {
        public void Dispose()
        {
            // Transaction scope cleanup
        }
    }
}

/// <summary>
/// Extension methods for AdpaDbContext to make it more EF-like
/// </summary>
public static class AdpaDbContextExtensions
{
    /// <summary>
    /// Configure the context for dependency injection (ready for EF Core)
    /// </summary>
    public static IServiceCollection AddAdpaDbContext(this IServiceCollection services, Action<DbContextOptions>? options = null)
    {
        // When EF Core packages are available, replace with:
        // services.AddDbContext<AdpaDbContext>(options);
        
        services.AddScoped<AdpaDbContext>();
        return services;
    }
}

/// <summary>
/// Placeholder for EF Core DbContextOptions (when EF packages are available)
/// </summary>
public class DbContextOptions
{
    // This will be replaced with actual EF Core DbContextOptions
}