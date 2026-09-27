namespace TechRoad.API.Models;

public class Technology
{
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
    public string Color { get; set; } = "";

    public List<RoadmapLevel> Levels { get; set; } = new();
}