using Microsoft.AspNetCore.Mvc;
using TechRoad.API.Models;
using TechRoad.API.Services;

namespace TechRoad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoadmapController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;

    public RoadmapController(IRoadmapService roadmapService)
    {
        _roadmapService = roadmapService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var roadmaps = _roadmapService.GetAllCategories();
        return Ok(roadmaps);
    }

    [HttpGet("{categoryName}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCategory(string categoryName)
    {
        var category = _roadmapService.GetCategoryByName(categoryName);
        if (category == null)
        {
            return NotFound(new { message = $"Category '{categoryName}' not found." });
        }

        return Ok(category);
    }

    [HttpGet("tech/{techName}")]
    [ProducesResponseType(typeof(Technology), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTechnology(string techName)
    {
        var tech = _roadmapService.GetTechnologyByName(techName);
        if (tech == null)
        {
            return NotFound(new { message = $"Technology '{techName}' not found." });
        }

        return Ok(tech);
    }
}