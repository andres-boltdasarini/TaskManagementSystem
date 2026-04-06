// Validators/TaskItemValidator.cs
using FluentValidation;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Validators;

public class TaskItemValidator : AbstractValidator<TaskItem>
{
    public TaskItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название задачи обязательно")
            .MaximumLength(200).WithMessage("Название задачи не может превышать 200 символов")
            .MinimumLength(3).WithMessage("Название задачи должно содержать минимум 3 символа");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание не может превышать 2000 символов");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId обязателен");
    }
}