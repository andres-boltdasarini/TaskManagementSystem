// Tests/Controllers/TasksControllerTests.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using TaskManagementSystem.API.Controllers;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.API.Validators;

namespace TaskManagementSystem.API.Tests.Controllers;

[TestFixture]
public class TasksControllerTests
{
    private AppDbContext _context;
    private TasksController _controller;
    private Guid _userId;
    private DbContextOptions<AppDbContext> _options;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(_options);
        _userId = Guid.NewGuid();

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
            new Claim("sub", _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var validators = new TaskItemValidator();
        var taskRequestValidator = new TaskRequestDtoValidator();

        _controller = new TasksController(_context, validators, taskRequestValidator);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<TaskItem> CreateTestTask(string title = "Test Task", bool isCompleted = false)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test Description",
            IsCompleted = isCompleted,
            UserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        return task;
    }

    [Test]
    public async Task GetTasks_ReturnsPagedResponse()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            await CreateTestTask($"Task {i}");
        }

        // Act
        var result = await _controller.GetTasks(page: 1, pageSize: 10);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var response = okResult!.Value as PagedResponseDto<TaskItem>;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Items.Count(), Is.EqualTo(10));
        Assert.That(response.TotalCount, Is.EqualTo(15));
        Assert.That(response.TotalPages, Is.EqualTo(2));
        Assert.That(response.HasNextPage, Is.True);
        Assert.That(response.HasPreviousPage, Is.False);
    }

    [Test]
    public async Task GetTasks_WithIsCompletedFilter_ReturnsFilteredTasks()
    {
        // Arrange
        await CreateTestTask("Completed Task", true);
        await CreateTestTask("Incomplete Task", false);
        await CreateTestTask("Another Completed", true);

        // Act
        var result = await _controller.GetTasks(isCompleted: true);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as PagedResponseDto<TaskItem>;

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Items.All(t => t.IsCompleted == true), Is.True);
        Assert.That(response.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetTasks_WithSearchTerm_FiltersTasks()
    {
        // Arrange
        await CreateTestTask("Important Meeting");
        await CreateTestTask("Shopping List");
        await CreateTestTask("Meeting with team");

        // Act
        var result = await _controller.GetTasks(searchTerm: "Meeting");

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as PagedResponseDto<TaskItem>;

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.TotalCount, Is.EqualTo(2));
        Assert.That(response.Items.Any(t => t.Title.Contains("Meeting")), Is.True);
    }

    [Test]
    public async Task GetTasks_WithSorting_ReturnsSortedTasks()
    {
        // Arrange
        await CreateTestTask("B Task");
        await CreateTestTask("A Task");
        await CreateTestTask("C Task");

        // Act
        var result = await _controller.GetTasks(sortBy: "title", sortDescending: false);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as PagedResponseDto<TaskItem>;

        Assert.That(response, Is.Not.Null);
        var titles = response!.Items.Select(t => t.Title).ToList();
        Assert.That(titles, Is.Ordered.Ascending);
    }

    [Test]
    public async Task GetTask_ExistingTask_ReturnsTask()
    {
        // Arrange
        var task = await CreateTestTask();

        // Act
        var result = await _controller.GetTask(task.Id);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var returnedTask = okResult!.Value as TaskItem;
        Assert.That(returnedTask, Is.Not.Null);
        Assert.That(returnedTask!.Id, Is.EqualTo(task.Id));
        Assert.That(returnedTask.Title, Is.EqualTo(task.Title));
    }

    [Test]
    public async Task GetTask_NonExistingTask_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetTask(Guid.NewGuid());

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetTask_TaskFromAnotherUser_ReturnsNotFound()
    {
        // Arrange
        var otherUserTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Other User Task",
            UserId = Guid.NewGuid()
        };
        await _context.Tasks.AddAsync(otherUserTask);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTask(otherUserTask.Id);

        // Assert
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task CreateTask_ValidTask_ReturnsCreated()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "New Task",
            Description = "Task Description",
            IsCompleted = false
        };

        // Act
        var result = await _controller.CreateTask(newTask);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.StatusCode, Is.EqualTo(201));

        var createdTask = createdResult.Value as TaskItem;
        Assert.That(createdTask, Is.Not.Null);
        Assert.That(createdTask!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(createdTask.Title, Is.EqualTo(newTask.Title));
        Assert.That(createdTask.UserId, Is.EqualTo(_userId));
        Assert.That(createdTask.CreatedAt, Is.Not.EqualTo(default(DateTime)));

        var savedTask = await _context.Tasks.FindAsync(createdTask.Id);
        Assert.That(savedTask, Is.Not.Null);
    }

    [Test]
    public async Task CreateTask_InvalidTask_ReturnsBadRequest()
    {
        // Arrange
        var invalidTask = new TaskItem
        {
            Title = "", // Invalid - empty title
            Description = "Description"
        };

        // Act
        var result = await _controller.CreateTask(invalidTask);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateTask_ValidUpdate_ReturnsNoContent()
    {
        // Arrange
        var existingTask = await CreateTestTask();

        var updatedTask = new TaskItem
        {
            Id = existingTask.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            IsCompleted = true
        };

        // Act
        var result = await _controller.UpdateTask(existingTask.Id, updatedTask);

        // Assert
        Assert.That(result, Is.TypeOf<NoContentResult>());

        var taskFromDb = await _context.Tasks.FindAsync(existingTask.Id);
        Assert.That(taskFromDb, Is.Not.Null);
        Assert.That(taskFromDb!.Title, Is.EqualTo("Updated Title"));
        Assert.That(taskFromDb.Description, Is.EqualTo("Updated Description"));
        Assert.That(taskFromDb.IsCompleted, Is.True);
    }

    [Test]
    public async Task UpdateTask_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var existingTask = await CreateTestTask();
        var updatedTask = new TaskItem
        {
            Id = Guid.NewGuid(), // Different ID
            Title = "Updated Title"
        };

        // Act
        var result = await _controller.UpdateTask(existingTask.Id, updatedTask);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdateTask_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updatedTask = new TaskItem
        {
            Id = nonExistingId,
            Title = "Updated Title"
        };

        // Act
        var result = await _controller.UpdateTask(nonExistingId, updatedTask);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateTask_TaskFromAnotherUser_ReturnsNotFound()
    {
        // Arrange
        var otherUserTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Other User Task",
            UserId = Guid.NewGuid()
        };
        await _context.Tasks.AddAsync(otherUserTask);
        await _context.SaveChangesAsync();

        var updatedTask = new TaskItem
        {
            Id = otherUserTask.Id,
            Title = "Updated Title"
        };

        // Act
        var result = await _controller.UpdateTask(otherUserTask.Id, updatedTask);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteTask_ExistingTask_ReturnsNoContent()
    {
        // Arrange
        var task = await CreateTestTask();

        // Act
        var result = await _controller.DeleteTask(task.Id);

        // Assert
        Assert.That(result, Is.TypeOf<NoContentResult>());

        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        Assert.That(deletedTask, Is.Null);
    }

    [Test]
    public async Task DeleteTask_NonExistingTask_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteTask(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteTask_TaskFromAnotherUser_ReturnsNotFound()
    {
        // Arrange
        var otherUserTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Other User Task",
            UserId = Guid.NewGuid()
        };
        await _context.Tasks.AddAsync(otherUserTask);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteTask(otherUserTask.Id);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetTasksAdvanced_ValidRequest_ReturnsPagedResponse()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            await CreateTestTask($"Task {i}", i % 2 == 0);
        }

        var request = new TaskRequestDto
        {
            Page = 2,
            PageSize = 10,
            IsCompleted = true
        };

        // Act
        var result = await _controller.GetTasksAdvanced(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as PagedResponseDto<TaskItem>;

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Items.All(t => t.IsCompleted == true), Is.True);
        Assert.That(response.Page, Is.EqualTo(2));
        Assert.That(response.PageSize, Is.EqualTo(10));
    }
}