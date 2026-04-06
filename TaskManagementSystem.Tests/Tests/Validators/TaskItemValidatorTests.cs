// Tests/Validators/TaskItemValidatorTests.cs
using FluentValidation.TestHelper;
using NUnit.Framework;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.API.Validators;

namespace TaskManagementSystem.API.Tests.Validators;

[TestFixture]
public class TaskItemValidatorTests
{
    private TaskItemValidator _validator;
    private TaskItem _validTask;

    [SetUp]
    public void SetUp()
    {
        _validator = new TaskItemValidator();
        _validTask = new TaskItem
        {
            Title = "Valid Task Title",
            Description = "Valid description",
            UserId = Guid.NewGuid()
        };
    }

    [Test]
    public void Validate_ValidTask_ShouldNotHaveErrors()
    {
        // Act
        var result = _validator.TestValidate(_validTask);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [TestCase("")]
    [TestCase(null)]
    [TestCase("AB")] // less than 3 chars
    public void Validate_InvalidTitle_ShouldHaveError(string title)
    {
        // Arrange
        _validTask.Title = title;

        // Act
        var result = _validator.TestValidate(_validTask);

        // Assert
        if (string.IsNullOrEmpty(title) || title.Length < 3)
            result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_TitleTooLong_ShouldHaveError()
    {
        // Arrange
        _validTask.Title = new string('A', 201);

        // Act
        var result = _validator.TestValidate(_validTask);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_DescriptionTooLong_ShouldHaveError()
    {
        // Arrange
        _validTask.Description = new string('A', 2001);

        // Act
        var result = _validator.TestValidate(_validTask);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Validate_EmptyUserId_ShouldHaveError()
    {
        // Arrange
        _validTask.UserId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(_validTask);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}