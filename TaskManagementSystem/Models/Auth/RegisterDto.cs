// Models/Auth/RegisterDto.cs
namespace TaskManagementSystem.API.Models.Auth;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}