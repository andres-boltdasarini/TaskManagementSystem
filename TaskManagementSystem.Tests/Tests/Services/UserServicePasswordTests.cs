// Tests/Services/UserServicePasswordTests.cs
using NUnit.Framework;
using System.Reflection;
using TaskManagementSystem.API.Services;

namespace TaskManagementSystem.API.Tests.Services;

[TestFixture]
public class UserServicePasswordTests
{
    [Test]
    public void HashPassword_ReturnsDifferentHashForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // We need to use reflection or make the method public for testing
        // For now, we'll test through a test subclass or make internal methods visible

        // Alternative: Create a testable version
        var userService = CreateUserServiceWithPrivateMethods();

        // Act
        var hash1 = InvokeHashPassword(userService, password);
        var hash2 = InvokeHashPassword(userService, password);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
        Assert.That(hash1, Does.StartWith("$2"));
        Assert.That(hash2, Does.StartWith("$2"));
    }

    private object CreateUserServiceWithPrivateMethods()
    {
        // This is a workaround for testing private methods
        // In production, consider making internal methods visible to test assembly
        // using InternalsVisibleTo attribute
        var constructor = typeof(UserService).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(Data.AppDbContext), typeof(JwtService) },
            null);

        return constructor!.Invoke(new object[] { null!, null! });
    }

    private string InvokeHashPassword(object userService, string password)
    {
        var method = typeof(UserService).GetMethod("HashPassword",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (string)method!.Invoke(userService, new object[] { password })!;
    }
}