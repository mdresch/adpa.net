using Adpa.Api.Models;

namespace Adpa.Api.Services;

public interface IFeatureService
{
    Task<IEnumerable<Feature>> GetAllFeaturesAsync();
    Task<Feature?> GetFeatureByIdAsync(int id);
    Task<Feature> CreateFeatureAsync(Feature feature);
    Task<Feature?> UpdateFeatureAsync(int id, Feature feature);
    Task<bool> DeleteFeatureAsync(int id);
}
