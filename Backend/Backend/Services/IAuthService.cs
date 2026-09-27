using TechRoad.API.Models.DTOs;

namespace TechRoad.API.Services;

public interface IAuthService
{
    (bool Success, string Message, AuthResponseDto? Response) Register(RegisterRequestDto request);
    (bool Success, string Message, AuthResponseDto? Response) Login(LoginRequestDto request);
    UserProfileDto? GetProfile(string username);
}
