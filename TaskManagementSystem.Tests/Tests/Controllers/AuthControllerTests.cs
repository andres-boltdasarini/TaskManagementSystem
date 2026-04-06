// Tests/Controllers/AuthControllerTests.cs
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TaskManagementSystem.API.Controllers;
using TaskManagementSystem.API.Models.Auth;
using TaskManagementSystem.API.Services;

namespace TaskManagementSystem.API.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<UserService> _mockUserService;
    private AuthController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockUserService = new Mock<UserService>(null!, null!);
        _controller = new AuthController(_mockUserService.Object);
    }

    [Test]
    public async Task Register_ValidUser_ReturnsOkWithToken()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "new@example.com",
            Name = "John",
            LastName = "Doe",
            Password = "Test123!"
        };

        var expectedResponse = new AuthResponse
        {
            Token = "test-token",
            Email = registerDto.Email,
            Name = registerDto.Name,
            LastName = registerDto.LastName
        };

        _mockUserService.Setup(x => x.RegisterAsync(registerDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        var response = okResult.Value as AuthResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Token, Is.EqualTo(expectedResponse.Token));
    }

    [Test]
    public async Task Register_ExistingUser_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Name = "John",
            LastName = "Doe",
            Password = "Test123!"
        };

        _mockUserService.Setup(x => x.RegisterAsync(registerDto))
            .ReturnsAsync((AuthResponse?)null);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        var expectedResponse = new AuthResponse
        {
            Token = "test-token",
            Email = loginDto.Email,
            Name = "John",
            LastName = "Doe"
        };

        _mockUserService.Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        var response = okResult.Value as AuthResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Token, Is.EqualTo(expectedResponse.Token));
    }

    [Test]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        _mockUserService.Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync((AuthResponse?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
        Assert.That(unauthorizedResult!.StatusCode, Is.EqualTo(401));
    }
}