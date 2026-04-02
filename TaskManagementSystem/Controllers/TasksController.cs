using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub");
        
        return Guid.Parse(userIdClaim?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<TaskItem>>> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isCompleted = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "createdat",
        [FromQuery] bool sortDescending = false)
    {
        var userId = GetCurrentUserId();
        
        // Базовый запрос
        var query = _context.Tasks
            .Where(t => t.UserId == userId)
            .AsQueryable();
        
        // Применяем фильтрацию по статусу
        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }
        
        // Применяем поиск по Title и Description
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(t => 
                t.Title.ToLower().Contains(searchTerm) || 
                (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }
        
        // Получаем общее количество до пагинации
        var totalCount = await query.CountAsync();
        
        // Применяем сортировку
        query = ApplySorting(query, sortBy, sortDescending);
        
        // Применяем пагинацию
        var tasks = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        // Формируем ответ с пагинацией
        var response = new PagedResponseDto<TaskItem>(tasks, totalCount, page, pageSize);
        
        return Ok(response);
    }
    
    // Альтернативный метод с использованием DTO
    [HttpGet("advanced")]
    public async Task<ActionResult<PagedResponseDto<TaskItem>>> GetTasksAdvanced([FromQuery] TaskRequestDto request)
    {
        var userId = GetCurrentUserId();
        
        // Валидация
        if (request.Page < 1) request.Page = 1;
        if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;
        
        // Базовый запрос
        var query = _context.Tasks
            .Where(t => t.UserId == userId)
            .AsQueryable();
        
        // Фильтрация по статусу
        if (request.IsCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == request.IsCompleted.Value);
        }
        
        // Поиск
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t => 
                t.Title.ToLower().Contains(searchTerm) || 
                (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }
        
        // Подсчет общего количества
        var totalCount = await query.CountAsync();
        
        // Сортировка
        query = ApplySorting(query, request.SortBy ?? "createdat", request.SortDescending);
        
        // Пагинация
        var tasks = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
        
        return Ok(new PagedResponseDto<TaskItem>(tasks, totalCount, request.Page, request.PageSize));
    }

    private IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, string? sortBy, bool sortDescending)
    {
        sortBy = sortBy?.ToLower();
        
        return sortBy switch
        {
            "title" => sortDescending 
                ? query.OrderByDescending(t => t.Title) 
                : query.OrderBy(t => t.Title),
            "completed" => sortDescending 
                ? query.OrderByDescending(t => t.IsCompleted) 
                : query.OrderBy(t => t.IsCompleted),
            "createdat" => sortDescending 
                ? query.OrderByDescending(t => t.CreatedAt) 
                : query.OrderBy(t => t.CreatedAt),
            _ => sortDescending 
                ? query.OrderByDescending(t => t.CreatedAt) 
                : query.OrderBy(t => t.CreatedAt)
        };
    }

    [HttpGet("GetTask/{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(Guid id)
    {
        var userId = GetCurrentUserId();
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        
        if (task == null)
        {
            return NotFound();
        }
        
        return Ok(task);
    }
   
    [HttpPost("CreateTask")]
    public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
    {
        var userId = GetCurrentUserId();
        
        task.Id = Guid.NewGuid();
        task.CreatedAt = DateTime.UtcNow;
        task.UserId = userId;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("UpdateTask/{id}")]
    public async Task<ActionResult> UpdateTask(Guid id, TaskItem updatedTask)
    {
        if (id != updatedTask.Id)
        {
            return BadRequest();
        }
        
        var userId = GetCurrentUserId();
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        
        if (task == null)
        {
            return NotFound();
        }

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.IsCompleted = updatedTask.IsCompleted;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("DeleteTask/{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = GetCurrentUserId();
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        
        if (task == null)
        {
            return NotFound();
        }
        
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}