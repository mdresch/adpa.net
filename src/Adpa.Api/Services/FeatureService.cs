using Adpa.Api.Models;

namespace Adpa.Api.Services;

public class FeatureService : IFeatureService
{
    private readonly List<Feature> _features = new()
    {
        new Feature { Id = 1, Name = "User Authentication", Description = "Secure login and registration system", Status = "In Progress" },
        new Feature { Id = 2, Name = "Dashboard Analytics", Description = "Real-time data visualization and reporting", Status = "Pending" },
        new Feature { Id = 3, Name = "API Integration", Description = "RESTful API endpoints for external services", Status = "Completed" },
        new Feature { Id = 4, Name = "Notification System", Description = "Push notifications and email alerts", Status = "Pending" },
        new Feature { Id = 5, Name = "Data Export", Description = "Export data in multiple formats (CSV, PDF, Excel)", Status = "In Progress" }
    };

    public Task<IEnumerable<Feature>> GetAllFeaturesAsync()
    {
        return Task.FromResult<IEnumerable<Feature>>(_features);
    }

    public Task<Feature?> GetFeatureByIdAsync(int id)
    {
        var feature = _features.FirstOrDefault(f => f.Id == id);
        return Task.FromResult(feature);
    }

    public Task<Feature> CreateFeatureAsync(Feature feature)
    {
        feature.Id = _features.Any() ? _features.Max(f => f.Id) + 1 : 1;
        feature.CreatedAt = DateTime.UtcNow;
        _features.Add(feature);
        return Task.FromResult(feature);
    }

    public Task<Feature?> UpdateFeatureAsync(int id, Feature feature)
    {
        var existingFeature = _features.FirstOrDefault(f => f.Id == id);
        if (existingFeature == null)
            return Task.FromResult<Feature?>(null);

        existingFeature.Name = feature.Name;
        existingFeature.Description = feature.Description;
        existingFeature.Status = feature.Status;
        
        return Task.FromResult<Feature?>(existingFeature);
    }

    public Task<bool> DeleteFeatureAsync(int id)
    {
        var feature = _features.FirstOrDefault(f => f.Id == id);
        if (feature == null)
            return Task.FromResult(false);

        _features.Remove(feature);
        return Task.FromResult(true);
    }
}
