using TechRoad.API.Models;

namespace TechRoad.API.Services;

public interface IJsonStorageService
{
    List<User> GetUsers();
    User? GetUserByEmail(string email);
    User? GetUserByUsername(string username);
    void SaveUser(User user);
    void SaveUsers(List<User> users);

    List<Category> GetRoadmaps();
    Category? GetCategoryByName(string name);
    Technology? GetTechnologyByName(string techName);

    List<UserProgress> GetAllProgress();
    List<UserProgress> GetProgressForUser(string username);
    UserProgress? GetProgress(string username, string technology);
    UserProgress SaveOrUpdateProgress(string username, string technology, bool basics, bool intermediate, bool advanced, int projects, int score);
    UserProgress SaveOrUpdateProgress(string username, string technology, List<TopicProgress> topicProgress, int projects, int score);
    bool DeleteProgress(string username, string technology);
}
