using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ADPA.Data;

/// <summary>
/// Design-time factory for EF Core migrations
/// </summary>
public class AdpaEfDbContextFactory : IDesignTimeDbContextFactory<AdpaEfDbContext>
{
    public AdpaEfDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdpaEfDbContext>();
        
        // Use default connection string for migrations
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AdpaDb;Trusted_Connection=true;MultipleActiveResultSets=true;");
        
        return new AdpaEfDbContext(optionsBuilder.Options);
    }
}