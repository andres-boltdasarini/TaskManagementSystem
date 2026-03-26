// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.API.Models.Auth;
using TaskManagementSystem.API.Services;

namespace TaskManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var result = await _userService.RegisterAsync(registerDto);
        
        if (result == null)
            return BadRequest(new { message = "Пользователь с таким email уже существует" });

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var result = await _userService.LoginAsync(loginDto);
        
        if (result == null)
            return Unauthorized(new { message = "Неверный email или пароль" });

        return Ok(result);
    }
}