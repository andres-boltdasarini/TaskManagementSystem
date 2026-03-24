using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem { Id = Guid.NewGuid(), Title = "Изучить С", Description = "Пройдите курс", IsCompleted = false },
            new TaskItem { Id = Guid.NewGuid(), Title = "Создать проект", Description = "Начать реализацию Task Manager", IsCompleted = true }
        );
    }
}