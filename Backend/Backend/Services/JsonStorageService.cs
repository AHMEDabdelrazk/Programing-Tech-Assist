using System.Text.Json;
using TechRoad.API.Models;

namespace TechRoad.API.Services;

public class JsonStorageService : IJsonStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ILogger<JsonStorageService> _logger;

    private readonly ReaderWriterLockSlim _usersLock = new();
    private readonly ReaderWriterLockSlim _progressLock = new();
    private readonly ReaderWriterLockSlim _roadmapsLock = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public JsonStorageService(
        IWebHostEnvironment env,
        IPasswordHasherService passwordHasher,
        ILogger<JsonStorageService> logger)
    {
        _env = env;
        _passwordHasher = passwordHasher;
        _logger = logger;

        EnsureDataDirectoryExists();
        MigratePlaintextPasswordsIfNeeded();
    }

    private string DataDirectory => Path.Combine(_env.ContentRootPath, "Data");
    private string UsersFile => Path.Combine(DataDirectory, "users.json");
    private string RoadmapsFile => Path.Combine(DataDirectory, "roadmaps.json");
    private string ProgressFile => Path.Combine(DataDirectory, "progress.json");

    private void EnsureDataDirectoryExists()
    {
        if (!Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
        }
    }

    #region Users Storage

    public List<User> GetUsers()
    {
        _usersLock.EnterReadLock();
        try
        {
            if (!File.Exists(UsersFile))
                return new List<User>();

            var json = File.ReadAllText(UsersFile);
            if (string.IsNullOrWhiteSpace(json))
                return new List<User>();

            return JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new List<User>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading users file.");
            return new List<User>();
        }
        finally
        {
            _usersLock.ExitReadLock();
        }
    }

    public User? GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var users = GetUsers();
        return users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public User? GetUserByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        var users = GetUsers();
        return users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public void SaveUser(User user)
    {
        _usersLock.EnterWriteLock();
        try
        {
            var users = ReadUsersInternal();
            var existingIndex = users.FindIndex(u => 
                string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                users[existingIndex] = user;
            }
            else
            {
                users.Add(user);
            }

            WriteUsersInternal(users);
        }
        finally
        {
            _usersLock.ExitWriteLock();
        }
    }

    public void SaveUsers(List<User> users)
    {
        _usersLock.EnterWriteLock();
        try
        {
            WriteUsersInternal(users);
        }
        finally
        {
            _usersLock.ExitWriteLock();
        }
    }

    private List<User> ReadUsersInternal()
    {
        if (!File.Exists(UsersFile))
            return new List<User>();

        var json = File.ReadAllText(UsersFile);
        if (string.IsNullOrWhiteSpace(json))
            return new List<User>();

        return JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new List<User>();
    }

    private void WriteUsersInternal(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, _jsonOptions);
        SafeWriteAtomic(UsersFile, json);
    }

    private void MigratePlaintextPasswordsIfNeeded()
    {
        try
        {
            var users = GetUsers();
            var needsMigration = false;

            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.PasswordHash) && !_passwordHasher.IsHashed(user.PasswordHash))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(user.PasswordHash);
                    needsMigration = true;
                }
            }

            if (needsMigration)
            {
                SaveUsers(users);
                _logger.LogInformation("Successfully migrated legacy plaintext passwords to secure BCrypt hashes.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to migrate legacy passwords during initialization.");
        }
    }

    #endregion

    #region Roadmaps Storage

    public List<Category> GetRoadmaps()
    {
        _roadmapsLock.EnterReadLock();
        try
        {
            if (!File.Exists(RoadmapsFile))
                return new List<Category>();

            var json = File.ReadAllText(RoadmapsFile);
            if (string.IsNullOrWhiteSpace(json))
                return new List<Category>();

            return JsonSerializer.Deserialize<List<Category>>(json, _jsonOptions) ?? new List<Category>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading roadmaps file.");
            return new List<Category>();
        }
        finally
        {
            _roadmapsLock.ExitReadLock();
        }
    }

    public Category? GetCategoryByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var categories = GetRoadmaps();
        return categories.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public Technology? GetTechnologyByName(string techName)
    {
        if (string.IsNullOrWhiteSpace(techName)) return null;
        var categories = GetRoadmaps();
        foreach (var cat in categories)
        {
            var tech = cat.Technologies.FirstOrDefault(t => 
                string.Equals(t.Name, techName, StringComparison.OrdinalIgnoreCase));
            if (tech != null) return tech;
        }
        return null;
    }

    #endregion

    #region Progress Storage

    public List<UserProgress> GetAllProgress()
    {
        _progressLock.EnterReadLock();
        try
        {
            return ReadProgressInternal();
        }
        finally
        {
            _progressLock.ExitReadLock();
        }
    }

    public List<UserProgress> GetProgressForUser(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return new List<UserProgress>();

        var all = GetAllProgress();
        return all.Where(p => string.Equals(p.Username, username, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public UserProgress? GetProgress(string username, string technology)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(technology))
            return null;

        var userProgress = GetProgressForUser(username);
        return userProgress.FirstOrDefault(p => 
            string.Equals(p.Technology, technology, StringComparison.OrdinalIgnoreCase));
    }

    public UserProgress SaveOrUpdateProgress(
        string username,
        string technology,
        bool basics,
        bool intermediate,
        bool advanced,
        int projects,
        int score)
    {
        return SaveOrUpdateProgress(username, technology, new List<TopicProgress>(), projects, score, basics, intermediate, advanced);
    }

    public UserProgress SaveOrUpdateProgress(
        string username,
        string technology,
        List<TopicProgress> topicProgress,
        int projects,
        int score)
    {
        var basics = topicProgress.Any(p => p.Level.Equals("Basics", StringComparison.OrdinalIgnoreCase) && p.Completed);
        var intermediate = topicProgress.Any(p => p.Level.Equals("Intermediate", StringComparison.OrdinalIgnoreCase) && p.Completed);
        var advanced = topicProgress.Any(p => p.Level.Equals("Advanced", StringComparison.OrdinalIgnoreCase) && p.Completed);
        return SaveOrUpdateProgress(username, technology, topicProgress, projects, score, basics, intermediate, advanced);
    }

    private UserProgress SaveOrUpdateProgress(
        string username,
        string technology,
        List<TopicProgress> topicProgress,
        int projects,
        int score,
        bool basics,
        bool intermediate,
        bool advanced)
    {
        _progressLock.EnterWriteLock();
        try
        {
            var list = ReadProgressInternal();
            var existing = list.FirstOrDefault(x =>
                string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Technology, technology, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Basics = basics;
                existing.Intermediate = intermediate;
                existing.Advanced = advanced;
                existing.Projects = projects;
                existing.Score = score;
                existing.TopicProgress = topicProgress;
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                existing = new UserProgress
                {
                    Username = username,
                    Technology = technology,
                    Basics = basics,
                    Intermediate = intermediate,
                    Advanced = advanced,
                    Projects = projects,
                    Score = score,
                    TopicProgress = topicProgress,
                    LastUpdated = DateTime.UtcNow
                };
                list.Add(existing);
            }

            var json = JsonSerializer.Serialize(list, _jsonOptions);
            SafeWriteAtomic(ProgressFile, json);

            return existing;
        }
        finally
        {
            _progressLock.ExitWriteLock();
        }
    }

    public bool DeleteProgress(string username, string technology)
    {
        _progressLock.EnterWriteLock();
        try
        {
            var list = ReadProgressInternal();
            var countRemoved = list.RemoveAll(x =>
                string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Technology, technology, StringComparison.OrdinalIgnoreCase));

            if (countRemoved > 0)
            {
                var json = JsonSerializer.Serialize(list, _jsonOptions);
                SafeWriteAtomic(ProgressFile, json);
                return true;
            }
            return false;
        }
        finally
        {
            _progressLock.ExitWriteLock();
        }
    }

    private List<UserProgress> ReadProgressInternal()
    {
        if (!File.Exists(ProgressFile))
            return new List<UserProgress>();

        var json = File.ReadAllText(ProgressFile);
        if (string.IsNullOrWhiteSpace(json))
            return new List<UserProgress>();

        return JsonSerializer.Deserialize<List<UserProgress>>(json, _jsonOptions) ?? new List<UserProgress>();
    }

    #endregion

    private static void SafeWriteAtomic(string filePath, string content)
    {
        var tempFile = filePath + ".tmp";
        File.WriteAllText(tempFile, content);

        if (File.Exists(filePath))
        {
            File.Replace(tempFile, filePath, null);
        }
        else
        {
            File.Move(tempFile, filePath);
        }
    }
}