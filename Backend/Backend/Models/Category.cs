namespace TechRoad.API.Models;

public class Category
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public string Guide { get; set; } = "";

    public List<Technology> Technologies { get; set; } = new();
}