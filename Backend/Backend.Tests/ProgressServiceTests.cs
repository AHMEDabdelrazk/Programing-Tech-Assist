using Moq;
using TechRoad.API.Models;
using TechRoad.API.Models.DTOs;
using TechRoad.API.Services;
using Xunit;

namespace TechRoad.Tests;

public class ProgressServiceTests
{
    private readonly Mock<IJsonStorageService> _mockStorage = new();
    private readonly ProgressService _service;

    public ProgressServiceTests()
    {
        _service = new ProgressService(_mockStorage.Object);
    }

    [Fact]
    public void SaveProgress_ShouldCalculateAuthoritativeScoreAndSave()
    {
        var username = "Alice";
        var dto = new SaveProgressDto
        {
            Technology = "React",
            Basics = true,
            Intermediate = true,
            Advanced = false,
            Projects = 2
        };

        _mockStorage.Setup(s => s.SaveOrUpdateProgress(
            username,
            dto.Technology,
            dto.Basics,
            dto.Intermediate,
            dto.Advanced,
            dto.Projects,
            60 // 20 + 30 + 10 = 60
        )).Returns(new UserProgress
        {
            Username = username,
            Technology = dto.Technology,
            Basics = dto.Basics,
            Intermediate = dto.Intermediate,
            Advanced = dto.Advanced,
            Projects = dto.Projects,
            Score = 60
        });

        var result = _service.SaveProgress(username, dto);

        Assert.NotNull(result);
        Assert.Equal(60, result.Score);
        Assert.Equal("React", result.Technology);
    }

    [Fact]
    public void GetStatsForUser_WithMultipleTrackedTechnologies_ShouldCalculateCorrectMetrics()
    {
        var username = "Bob";
        var progressList = new List<UserProgress>
        {
            new() { Username = username, Technology = "C#", Basics = true, Intermediate = true, Advanced = true, Projects = 4, Score = 100 },
            new() { Username = username, Technology = "SQL", Basics = true, Intermediate = false, Advanced = false, Projects = 0, Score = 20 }
        };

        _mockStorage.Setup(s => s.GetProgressForUser(username)).Returns(progressList);

        var stats = _service.GetStatsForUser(username);

        Assert.Equal(2, stats.TechnologiesTracked);
        Assert.Equal(60, stats.AverageScore); // (100 + 20) / 2 = 60
        Assert.Equal(1, stats.CompletedRoadmaps); // Only C# is >= 100
        Assert.Equal(4, stats.TotalProjects);
        Assert.Equal(4, stats.TotalLevelsCompleted); // 3 for C# + 1 for SQL
    }
}
