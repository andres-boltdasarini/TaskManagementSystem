// Tests/Services/UserServiceTests.cs
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.API.Models.Auth;
using TaskManagementSystem.API.Services;

namespace TaskManagementSystem.API.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private AppDbContext _context;
    private Mock<JwtService> _mockJwtService;
    private UserService _userService;
    private DbContextOptions<AppDbContext> _options;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(_options);
        _mockJwtService = new Mock<JwtService>(MockBehavior.Default, null!);
        _userService = new UserService(_context, _mockJwtService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Name = "Jane",
            LastName = "Smith",
            Password = "Test123!"
        };

        var expectedToken = "test-jwt-token";
        _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedToken);

        // Act
        var result = await _userService.RegisterAsync(registerDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Token, Is.EqualTo(expectedToken));
        Assert.That(result.Email, Is.EqualTo(registerDto.Email));
        Assert.That(result.Name, Is.EqualTo(registerDto.Name));
        Assert.That(result.LastName, Is.EqualTo(registerDto.LastName));

        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser.Name, Is.EqualTo(registerDto.Name));
    }

    [Test]
    public async Task RegisterAsync_ExistingEmail_ReturnsNull()
    {
        // Arrange
        var existingUser = new User
        {
            Email = "existing@example.com",
            Name = "Existing",
            LastName = "User",
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(existingUser);
        await _context.SaveChangesAsync();

        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Name = "New",
            LastName = "User",
            Password = "Test123!"
        };

        // Act
        var result = await _userService.RegisterAsync(registerDto);

        // Assert
        Assert.That(result, Is.Null);
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var password = "Test123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "John",
            LastName = "Doe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = password
        };

        var expectedToken = "test-jwt-token";
        _mockJwtService.Setup(x => x.GenerateToken(It.Is<User>(u => u.Id == user.Id)))
            .Returns(expectedToken);

        // Act
        var result = await _userService.LoginAsync(loginDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Token, Is.EqualTo(expectedToken));
        Assert.That(result.Email, Is.EqualTo(user.Email));
        Assert.That(result.Name, Is.EqualTo(user.Name));
    }

    [Test]
    [TestCase("wrong@example.com", "Test123!")]
    [TestCase("test@example.com", "WrongPassword!")]
    public async Task LoginAsync_InvalidCredentials_ReturnsNull(string email, string password)
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "John",
            LastName = "Doe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!")
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        // Act
        var result = await _userService.LoginAsync(loginDto);

        // Assert
        Assert.That(result, Is.Null);
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "John",
            LastName = "Doe",
            PasswordHash = "hash"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.GetUserByIdAsync(user.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
        Assert.That(result.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task GetUserByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Act
        var result = await _userService.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }
}