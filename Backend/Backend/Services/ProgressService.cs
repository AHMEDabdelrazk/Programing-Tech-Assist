using TechRoad.API.Models;
using TechRoad.API.Models.DTOs;

namespace TechRoad.API.Services;

public class ProgressService : IProgressService
{
    private readonly IJsonStorageService _storage;

    public ProgressService(IJsonStorageService storage)
    {
        _storage = storage;
    }

    public int CalculateScore(bool basics, bool intermediate, bool advanced, int projects)
    {
        int score = 0;
        if (basics) score += 20;
        if (intermediate) score += 30;
        if (advanced) score += 30;

        int projectScore = Math.Min(Math.Max(0, projects) * 5, 20);
        score += projectScore;

        return Math.Clamp(score, 0, 100);
    }

    public UserProgress SaveProgress(string username, SaveProgressDto dto)
    {
        if (dto.TopicProgress.Count > 0)
        {
            var topicProgress = dto.TopicProgress
                .Where(topic => !string.IsNullOrWhiteSpace(topic.Level) && !string.IsNullOrWhiteSpace(topic.Topic))
                .GroupBy(topic => $"{topic.Level}\n{topic.Topic}", StringComparer.OrdinalIgnoreCase)
                .Select(group => group.Last())
                .Select(topic => new TopicProgress
                {
                    Level = topic.Level.Trim(),
                    Topic = topic.Topic.Trim(),
                    Completed = topic.Completed
                })
                .ToList();

            var topicScore = CalculateScore(topicProgress, dto.Projects);
            return _storage.SaveOrUpdateProgress(username, dto.Technology, topicProgress, dto.Projects, topicScore);
        }

        var score = CalculateScore(dto.Basics, dto.Intermediate, dto.Advanced, dto.Projects);

        return _storage.SaveOrUpdateProgress(
            username,
            dto.Technology,
            dto.Basics,
            dto.Intermediate,
            dto.Advanced,
            dto.Projects,
            score
        );
    }

    public int CalculateScore(IReadOnlyCollection<TopicProgress> topics, int projects)
    {
        var learningScore = 0.0;
        var levelWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["Basics"] = 20,
            ["Intermediate"] = 30,
            ["Advanced"] = 30
        };

        foreach (var level in levelWeights)
        {
            var levelTopics = topics.Where(topic => topic.Level.Equals(level.Key, StringComparison.OrdinalIgnoreCase)).ToList();
            if (levelTopics.Count > 0)
            {
                learningScore += levelTopics.Count(topic => topic.Completed) * level.Value / levelTopics.Count;
            }
        }

        var projectScore = Math.Min(Math.Max(0, projects) * 5, 20);
        return Math.Clamp((int)Math.Round(learningScore + projectScore), 0, 100);
    }

    public List<ProgressResponseDto> GetProgressForUser(string username)
    {
        var list = _storage.GetProgressForUser(username);
        return list.Select(MapToResponseDto).ToList();
    }

    public ProgressResponseDto? GetProgress(string username, string technology)
    {
        var progress = _storage.GetProgress(username, technology);
        return progress == null ? null : MapToResponseDto(progress);
    }

    public ProgressStatsDto GetStatsForUser(string username)
    {
        var userProgressList = _storage.GetProgressForUser(username);
        if (userProgressList.Count == 0)
        {
            return new ProgressStatsDto
            {
                TechnologiesTracked = 0,
                AverageScore = 0,
                CompletedRoadmaps = 0,
                TotalProjects = 0,
                TotalLevelsCompleted = 0
                ,TotalTopicsCompleted = 0
                ,TotalTopics = 0
            };
        }

        var totalScore = userProgressList.Sum(p => p.Score);
        var averageScore = (int)Math.Round((double)totalScore / userProgressList.Count);
        var completedRoadmaps = userProgressList.Count(p => p.Score >= 100);
        var totalProjects = userProgressList.Sum(p => p.Projects);
        var levelsCompleted = userProgressList.Sum(p => 
            (p.Basics ? 1 : 0) + (p.Intermediate ? 1 : 0) + (p.Advanced ? 1 : 0));
        var topics = userProgressList.SelectMany(p => p.TopicProgress).ToList();

        return new ProgressStatsDto
        {
            TechnologiesTracked = userProgressList.Count,
            AverageScore = averageScore,
            CompletedRoadmaps = completedRoadmaps,
            TotalProjects = totalProjects,
            TotalLevelsCompleted = levelsCompleted,
            TotalTopicsCompleted = topics.Count(topic => topic.Completed),
            TotalTopics = topics.Count
        };
    }

    private static ProgressResponseDto MapToResponseDto(UserProgress p)
    {
        return new ProgressResponseDto
        {
            Username = p.Username,
            Technology = p.Technology,
            Basics = p.Basics,
            Intermediate = p.Intermediate,
            Advanced = p.Advanced,
            Projects = p.Projects,
            Score = p.Score,
            TopicProgress = p.TopicProgress.Select(topic => new TopicProgressDto
            {
                Level = topic.Level,
                Topic = topic.Topic,
                Completed = topic.Completed
            }).ToList(),
            LastUpdated = p.LastUpdated
        };
    }
}
