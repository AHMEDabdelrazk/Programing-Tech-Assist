using Moq;
using TechRoad.API.Models;
using TechRoad.API.Services;
using Xunit;

namespace TechRoad.Tests;

public class ProgressCalculationTests
{
    private readonly ProgressService _service;

    public ProgressCalculationTests()
    {
        var mockStorage = new Mock<IJsonStorageService>();
        _service = new ProgressService(mockStorage.Object);
    }

    [Fact]
    public void CalculateScore_WhenNothingCompleted_ShouldReturnZero()
    {
        var score = _service.CalculateScore(basics: false, intermediate: false, advanced: false, projects: 0);
        Assert.Equal(0, score);
    }

    [Fact]
    public void CalculateScore_WhenBasicsOnly_ShouldReturn20()
    {
        var score = _service.CalculateScore(basics: true, intermediate: false, advanced: false, projects: 0);
        Assert.Equal(20, score);
    }

    [Fact]
    public void CalculateScore_WithPartialTopicCompletion_ShouldWeightTopicsWithinTheirLevels()
    {
        var topics = new List<TopicProgress>
        {
            new() { Level = "Basics", Topic = "Syntax", Completed = true },
            new() { Level = "Basics", Topic = "Types", Completed = false },
            new() { Level = "Advanced", Topic = "Architecture", Completed = true }
        };

        var score = _service.CalculateScore(topics, projects: 0);

        Assert.Equal(40, score);
    }

    [Fact]
    public void CalculateScore_WhenIntermediateOnly_ShouldReturn30()
    {
        var score = _service.CalculateScore(basics: false, intermediate: true, advanced: false, projects: 0);
        Assert.Equal(30, score);
    }

    [Fact]
    public void CalculateScore_WhenAdvancedOnly_ShouldReturn30()
    {
        var score = _service.CalculateScore(basics: false, intermediate: false, advanced: true, projects: 0);
        Assert.Equal(30, score);
    }

    [Fact]
    public void CalculateScore_WhenAllLevelsCompletedNoProjects_ShouldReturn80()
    {
        var score = _service.CalculateScore(basics: true, intermediate: true, advanced: true, projects: 0);
        Assert.Equal(80, score);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(3, 15)]
    [InlineData(4, 20)]
    [InlineData(10, 20)] // Capped at 20
    public void CalculateScore_ProjectsScoreCalculation_ShouldRespect20PointCap(int projects, int expectedScore)
    {
        var score = _service.CalculateScore(basics: false, intermediate: false, advanced: false, projects: projects);
        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public void CalculateScore_WhenEverythingCompleted_ShouldReturn100()
    {
        var score = _service.CalculateScore(basics: true, intermediate: true, advanced: true, projects: 4);
        Assert.Equal(100, score);
    }

    [Fact]
    public void CalculateScore_WithExcessiveProjects_ShouldNeverExceed100()
    {
        var score = _service.CalculateScore(basics: true, intermediate: true, advanced: true, projects: 999);
        Assert.Equal(100, score);
    }
}
