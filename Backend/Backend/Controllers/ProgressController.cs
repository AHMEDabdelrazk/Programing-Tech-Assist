using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechRoad.API.Models.DTOs;
using TechRoad.API.Services;

namespace TechRoad.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProgressResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var username = GetAuthenticatedUsername();
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not identified." });
        }

        var list = _progressService.GetProgressForUser(username);
        return Ok(list);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(ProgressStatsDto), StatusCodes.Status200OK)]
    public IActionResult GetStats()
    {
        var username = GetAuthenticatedUsername();
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not identified." });
        }

        var stats = _progressService.GetStatsForUser(username);
        return Ok(stats);
    }

    [HttpGet("{technology}")]
    [ProducesResponseType(typeof(ProgressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByTech(string technology)
    {
        var username = GetAuthenticatedUsername();
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not identified." });
        }

        var item = _progressService.GetProgress(username, technology);
        if (item == null)
        {
            return NotFound(new { message = $"No progress found for {technology}" });
        }

        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProgressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Save([FromBody] SaveProgressDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = GetAuthenticatedUsername();
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "User not identified." });
        }

        var saved = _progressService.SaveProgress(username, dto);

        return Ok(new ProgressResponseDto
        {
            Username = saved.Username,
            Technology = saved.Technology,
            Basics = saved.Basics,
            Intermediate = saved.Intermediate,
            Advanced = saved.Advanced,
            Projects = saved.Projects,
            Score = saved.Score,
            TopicProgress = saved.TopicProgress.Select(topic => new TopicProgressDto
            {
                Level = topic.Level,
                Topic = topic.Topic,
                Completed = topic.Completed
            }).ToList(),
            LastUpdated = saved.LastUpdated
        });
    }

    private string? GetAuthenticatedUsername()
    {
        return User.FindFirstValue(ClaimTypes.Name) 
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.Identity?.Name;
    }
}