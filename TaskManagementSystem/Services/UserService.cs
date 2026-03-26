// Services/UserService.cs
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.API.Models.Auth;

namespace TaskManagementSystem.API.Services;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public UserService(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterDto registerDto)
    {
        // Проверка существующего пользователя
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        
        if (existingUser != null)
            return null;

        // Создание нового пользователя
        var user = new User
        {
            Email = registerDto.Email,
            Name = registerDto.Name,
            LastName = registerDto.LastName,
            PasswordHash = HashPassword(registerDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Генерация токена
        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            LastName = user.LastName
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            return null;

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            LastName = user.LastName
        };
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    private string HashPassword(string password)
    {
        // Используем BCrypt для хеширования пароля
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}