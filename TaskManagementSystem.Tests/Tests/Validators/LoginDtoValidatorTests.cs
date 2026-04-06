// Tests/Validators/LoginDtoValidatorTests.cs
using FluentValidation.TestHelper;
using NUnit.Framework;
using TaskManagementSystem.API.Models.Auth;
using TaskManagementSystem.API.Validators;

namespace TaskManagementSystem.API.Tests.Validators;

[TestFixture]
public class LoginDtoValidatorTests
{
    private LoginDtoValidator _validator;
    private LoginDto _validDto;

    [SetUp]
    public void SetUp()
    {
        _validator = new LoginDtoValidator();
        _validDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "password123"
        };
    }

    [Test]
    public void Validate_ValidDto_ShouldNotHaveErrors()
    {
        // Act
        var result = _validator.TestValidate(_validDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [TestCase("")]
    [TestCase(null)]
    [TestCase("invalid")]
    public void Validate_InvalidEmail_ShouldHaveError(string email)
    {
        // Arrange
        _validDto.Email = email;

        // Act
        var result = _validator.TestValidate(_validDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    [TestCase("")]
    [TestCase(null)]
    public void Validate_EmptyPassword_ShouldHaveError(string password)
    {
        // Arrange
        _validDto.Password = password;

        // Act
        var result = _validator.TestValidate(_validDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}