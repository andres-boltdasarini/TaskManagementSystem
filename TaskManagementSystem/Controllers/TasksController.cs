using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Защищаем все endpoints контроллера
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
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
        var userId = GetCurrentUserId();
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync();
        
        return Ok(tasks);
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