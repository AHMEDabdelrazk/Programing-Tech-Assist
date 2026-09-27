using System.ComponentModel.DataAnnotations;

namespace TechRoad.API.Models.DTOs;

public class SaveProgressDto
{
    [Required(ErrorMessage = "Technology name is required")]
    public string Technology { get; set; } = string.Empty;

    public bool Basics { get; set; }

    public bool Intermediate { get; set; }

    public bool Advanced { get; set; }

    [Range(0, 100, ErrorMessage = "Projects count must be between 0 and 100")]
    public int Projects { get; set; }

    public List<TopicProgressDto> TopicProgress { get; set; } = new();
}

public class TopicProgressDto
{
    [Required]
    public string Level { get; set; } = string.Empty;

    [Required]
    public string Topic { get; set; } = string.Empty;

    public bool Completed { get; set; }
}

public class ProgressResponseDto
{
    public string Username { get; set; } = string.Empty;
    public string Technology { get; set; } = string.Empty;
    public bool Basics { get; set; }
    public bool Intermediate { get; set; }
    public bool Advanced { get; set; }
    public int Projects { get; set; }
    public int Score { get; set; }
    public List<TopicProgressDto> TopicProgress { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ProgressStatsDto
{
    public int TechnologiesTracked { get; set; }
    public int AverageScore { get; set; }
    public int CompletedRoadmaps { get; set; }
    public int TotalProjects { get; set; }
    public int TotalLevelsCompleted { get; set; }
    public int TotalTopicsCompleted { get; set; }
    public int TotalTopics { get; set; }
}
