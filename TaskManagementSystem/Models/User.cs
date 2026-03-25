namespace TaskManagementSystem.API.Models;

public class User
{
public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

        [Required]
    public string Name { get; set; } = string.Empty;

        [Required]
    public string LastName { get; set; } = string.Empty ;

        [Required]
    public string PasswordHash { get; set; } = string.Empty ;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

     // Навигационное свойство для задач
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}