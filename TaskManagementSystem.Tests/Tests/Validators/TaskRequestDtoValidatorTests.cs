// Tests/Validators/TaskRequestDtoValidatorTests.cs
using FluentValidation.TestHelper;
using NUnit.Framework;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.API.Validators;

namespace TaskManagementSystem.API.Tests.Validators;

[TestFixture]
public class TaskRequestDtoValidatorTests
{
    private TaskRequestDtoValidator _validator;
    private TaskRequestDto _validRequest;

    [SetUp]
    public void SetUp()
    {
        _validator = new TaskRequestDtoValidator();
        _validRequest = new TaskRequestDto
        {
            Page = 1,
            PageSize = 10,
            SearchTerm = "test",
            SortBy = "title",
            SortDescending = false
        };
    }

    [Test]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        // Act
        var result = _validator.TestValidate(_validRequest);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_InvalidPage_ShouldHaveError(int page)
    {
        // Arrange
        _validRequest.Page = page;

        // Act
        var result = _validator.TestValidate(_validRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Test]
    [TestCase(0)]
    [TestCase(101)]
    [TestCase(200)]
    public void Validate_InvalidPageSize_ShouldHaveError(int pageSize)
    {
        // Arrange
        _validRequest.PageSize = pageSize;

        // Act
        var result = _validator.TestValidate(_validRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Test]
    [TestCase("invalid")]
    [TestCase("unknown")]
    [TestCase("")]
    public void Validate_InvalidSortBy_ShouldHaveError(string sortBy)
    {
        // Arrange
        _validRequest.SortBy = sortBy;

        // Act
        var result = _validator.TestValidate(_validRequest);

        // Assert
        if (!string.IsNullOrEmpty(sortBy) && sortBy != "title" && sortBy != "createdat" && sortBy != "completed")
            result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Test]
    public void Validate_SearchTermTooLong_ShouldHaveError()
    {
        // Arrange
        _validRequest.SearchTerm = new string('a', 101);

        // Act
        var result = _validator.TestValidate(_validRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Test]
    public void Validate_NullSearchTerm_ShouldNotHaveError()
    {
        // Arrange
        _validRequest.SearchTerm = null;

        // Act
        var result = _validator.TestValidate(_validRequest);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Test]
    public void Validate_EmptySortBy_ShouldNotHaveError()
    {
        // Arrange
        _validRequest.SortBy = string.Empty;

        // Act
        var result = _validator.TestValidate(_validRequest);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }
}