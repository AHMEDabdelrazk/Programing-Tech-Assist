using TechRoad.API.Models;
using TechRoad.API.Models.DTOs;

namespace TechRoad.API.Services;

public interface IProgressService
{
    int CalculateScore(bool basics, bool intermediate, bool advanced, int projects);
    UserProgress SaveProgress(string username, SaveProgressDto dto);
    List<ProgressResponseDto> GetProgressForUser(string username);
    ProgressResponseDto? GetProgress(string username, string technology);
    ProgressStatsDto GetStatsForUser(string username);
}
