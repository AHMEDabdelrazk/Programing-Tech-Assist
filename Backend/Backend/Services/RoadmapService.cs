using TechRoad.API.Models;

namespace TechRoad.API.Services;

public class RoadmapService : IRoadmapService
{
    private readonly IJsonStorageService _storage;

    public RoadmapService(IJsonStorageService storage)
    {
        _storage = storage;
    }

    public List<Category> GetAllCategories()
    {
        return _storage.GetRoadmaps();
    }

    public Category? GetCategoryByName(string name)
    {
        return _storage.GetCategoryByName(name);
    }

    public Technology? GetTechnologyByName(string techName)
    {
        return _storage.GetTechnologyByName(techName);
    }
}
