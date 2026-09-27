using TechRoad.API.Models;

namespace TechRoad.API.Services;

public interface IRoadmapService
{
    List<Category> GetAllCategories();
    Category? GetCategoryByName(string name);
    Technology? GetTechnologyByName(string techName);
}
