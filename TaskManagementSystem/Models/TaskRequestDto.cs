// Models/TaskRequestDto.cs
namespace TaskManagementSystem.API.Models;

public class TaskRequestDto
{
    // Пагинация
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Фильтрация
    public bool? IsCompleted { get; set; }
    public string? SearchTerm { get; set; } // Поиск по Title и Description
    public string? SortBy { get; set; } // "title", "createdat", "completed"
    public bool SortDescending { get; set; } = false;
}