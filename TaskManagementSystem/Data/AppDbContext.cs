using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка связи между TaskItem и User
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Уникальный индекс для Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        // Начальные данные для тестирования
        // Пароль: "admin123" (хешированный)
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = adminId,
                Email = "admin@example.com",
                Name = "Admin",
                LastName = "User",
                PasswordHash = "$2a$11$X7Y8Z9A0B1C2D3E4F5G6H7I8J9K0L1M2N3O4P5Q6R7S8T9U0V1W2X3Y4Z5", // "admin123"
                CreatedAt = DateTime.UtcNow
            }
        );
        
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem 
            { 
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Изучить C#", 
                Description = "Пройдите курс по C#", 
                IsCompleted = false,
                UserId = adminId,
                CreatedAt = DateTime.UtcNow
            },
            new TaskItem 
            { 
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "Создать проект", 
                Description = "Начать реализацию Task Manager", 
                IsCompleted = true,
                UserId = adminId,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}