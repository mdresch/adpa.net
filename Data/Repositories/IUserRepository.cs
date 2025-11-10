using ADPA.Models.Entities;
using System.Collections.Concurrent;

namespace ADPA.Data.Repositories;

/// <summary>
/// Interface for User repository
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

/// <summary>
/// In-memory implementation of User repository
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users;

    public InMemoryUserRepository()
    {
        _users = new ConcurrentDictionary<Guid, User>();
        SeedInitialData();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await Task.CompletedTask;
        return _users.TryGetValue(id, out var user) ? user : null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await Task.CompletedTask;
        return _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        await Task.CompletedTask;
        return _users.Values.ToList();
    }

    public async Task<User> CreateAsync(User user)
    {
        await Task.CompletedTask;
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _users.TryAdd(user.Id, user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        await Task.CompletedTask;
        user.UpdatedAt = DateTime.UtcNow;
        _users.TryUpdate(user.Id, user, _users[user.Id]);
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await Task.CompletedTask;
        return _users.TryRemove(id, out _);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.CompletedTask;
        return _users.ContainsKey(id);
    }

    private void SeedInitialData()
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
            CreatedAt = new DateTime(2025, 11, 8, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 11, 8, 0, 0, 0, DateTimeKind.Utc)
        };

        // Seed demo user
        var demoUser = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "demo@adpa.local",
            PasswordHash = HashPassword("Demo123!"),
            DisplayName = "Demo User",
            Role = "User",
            IsActive = true,
            CreatedAt = new DateTime(2025, 11, 8, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 11, 8, 0, 0, 0, DateTimeKind.Utc)
        };

        _users.TryAdd(adminUser.Id, adminUser);
        _users.TryAdd(demoUser.Id, demoUser);
    }

    private static string HashPassword(string password)
    {
        // Simple hash implementation for demo purposes
        // In production, use BCrypt or similar
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"hash_{password}_salt"));
    }
}