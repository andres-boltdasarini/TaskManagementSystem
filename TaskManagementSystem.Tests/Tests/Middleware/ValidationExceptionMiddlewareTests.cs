// Tests/Middleware/ValidationExceptionMiddlewareTests.cs
using FluentValidation;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System.Text.Json;
using TaskManagementSystem.API.Middleware;

namespace TaskManagementSystem.API.Tests.Middleware;

[TestFixture]
public class ValidationExceptionMiddlewareTests
{
    private ValidationExceptionMiddleware _middleware;
    private DefaultHttpContext _context;

    [SetUp]
    public void SetUp()
    {
        _middleware = new ValidationExceptionMiddleware(next: (innerHttpContext) => Task.CompletedTask);
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();
    }

    [Test]
    public async Task InvokeAsync_NoException_CallsNextDelegate()
    {
        // Arrange
        bool nextCalled = false;
        var middleware = new ValidationExceptionMiddleware(next: (innerHttpContext) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_ValidationException_ReturnsBadRequest()
    {
        // Arrange
        var validationException = new ValidationException("Validation failed", new[]
        {
            new FluentValidation.Results.ValidationFailure("Email", "Email is required"),
            new FluentValidation.Results.ValidationFailure("Password", "Password is required")
        });

        var middleware = new ValidationExceptionMiddleware(next: (innerHttpContext) =>
            throw validationException);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(_context.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(_context.Response.ContentType, Is.EqualTo("application/json"));

        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.ContainsKey("message"), Is.True);
        Assert.That(response.ContainsKey("errors"), Is.True);
    }

    [Test]
    public async Task InvokeAsync_OtherException_ThrowsOriginalException()
    {
        // Arrange
        var middleware = new ValidationExceptionMiddleware(next: (innerHttpContext) =>
            throw new InvalidOperationException("Test exception"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await middleware.InvokeAsync(_context));
    }
}