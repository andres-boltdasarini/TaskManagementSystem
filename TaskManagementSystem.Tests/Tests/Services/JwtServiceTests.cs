// Tests/Services/JwtServiceTests.cs
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.API.Services;
using TaskManagementSystem.API.Settings;

namespace TaskManagementSystem.API.Tests.Services;

[TestFixture]
public class JwtServiceTests
{
    private JwtService _jwtService;
    private JwtSettings _jwtSettings;
    private User _testUser;

    [SetUp]
    public void SetUp()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-that-is-at-least-32-chars-long!!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationHours = 24
        };

        var options = Options.Create(_jwtSettings);
        _jwtService = new JwtService(options);

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "John",
            LastName = "Doe"
        };
    }

    [Test]
    public void GenerateToken_ValidUser_ReturnsValidJwtToken()
    {
        // Act
        var token = _jwtService.GenerateToken(_testUser);

        // Assert
        Assert.That(token, Is.Not.Null.And.Not.Empty);

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        Assert.That(jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value,
            Is.EqualTo(_testUser.Id.ToString()));
        Assert.That(jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value,
            Is.EqualTo(_testUser.Email));
        Assert.That(jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value,
            Is.EqualTo(_testUser.Name));
        Assert.That(jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value,
            Is.EqualTo(_testUser.LastName));
    }

    [Test]
    public void GenerateToken_ValidUser_ContainsCorrectIssuerAndAudience()
    {
        // Act
        var token = _jwtService.GenerateToken(_testUser);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        Assert.That(jsonToken.Issuer, Is.EqualTo(_jwtSettings.Issuer));
        Assert.That(jsonToken.Audiences.First(), Is.EqualTo(_jwtSettings.Audience));
    }

    [Test]
    public void GenerateToken_ValidUser_TokenHasExpiration()
    {
        // Act
        var token = _jwtService.GenerateToken(_testUser);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        Assert.That(jsonToken.ValidTo, Is.GreaterThan(DateTime.UtcNow));
        Assert.That(jsonToken.ValidTo, Is.LessThanOrEqualTo(DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours + 1)));
    }
}