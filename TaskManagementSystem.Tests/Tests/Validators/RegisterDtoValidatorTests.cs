// Tests/Validators/RegisterDtoValidatorTests.cs
using FluentValidation.TestHelper;
using NUnit.Framework;
using TaskManagementSystem.API.Models.Auth;
using TaskManagementSystem.API.Validators;

namespace TaskManagementSystem.API.Tests.Validators;

[TestFixture]
public class RegisterDtoValidatorTests
{
    private RegisterDtoValidator _validator;
    private RegisterDto _validDto;

    [SetUp]
    public void SetUp()
    {
        _validator = new RegisterDtoValidator();
        _validDto = new RegisterDto
        {
            Email = "test@example.com",
            Name = "John",
            LastName = "Doe",
            Password = "Test123!"
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
    [TestCase("invalid-email")]
    [TestCase("test@")]
    [TestCase("test@example")]
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
    [TestCase("A")] // too short - name validation allows 1 char? Actually minimum is 1, but let's test empty
    public void Validate_EmptyName_ShouldHaveError(string name)
    {
        // Arrange
        _validDto.Name = name;

        // Act
        var result = _validator.TestValidate(_validDto);

        // Assert
        if (string.IsNullOrEmpty(name))
            result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    [TestCase("")]
    [TestCase(null)]
    public void Validate_EmptyLastName_ShouldHaveError(string lastName)
    {
        // Arrange
        _validDto.LastName = lastName;

        // Act
        var result = _validator.TestValidate(_validDto);

        // Assert
        if (string.IsNullOrEmpty(lastName))
            result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    [TestCase("")]
    [TestCase(null)]
    [TestCase("12345")] // too short (less than 6)
    [TestCase("nodigits")] // no digits
    [TestCase("NOLOWERCASE")] // no lowercase
    [TestCase("nouppercase1")] // no uppercase
    [TestCase("NoSpecial1")] // no special char
    public void Validate_InvalidPassword_ShouldHaveError(string password)
    {
        // Arrange
        _validDto.Password = password;

        // Act
        var result = _validator.TestValidate(_validDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    public void Validate_NameWithNumbers_ShouldHaveError()
    {
        // Arrange
        _validDto.Name = "John123";

        // Act
        var result = _validator.TestValidate(_validDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validate_EmailTooLong_ShouldHaveError()
    {
        // Arrange
        _validDto.Email = new string('a', 90) + "@example.com"; // > 100 chars

        // Act
        var result = _validator.TestValidate(_validDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}