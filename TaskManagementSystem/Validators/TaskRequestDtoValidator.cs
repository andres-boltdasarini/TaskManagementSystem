// Validators/TaskRequestDtoValidator.cs
using FluentValidation;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Validators;

public class TaskRequestDtoValidator : AbstractValidator<TaskRequestDto>
{
    public TaskRequestDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Страница должна быть больше или равна 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Размер страницы должен быть от 1 до 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Поисковый запрос не может превышать 100 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrEmpty(sortBy) ||
                   new[] { "title", "createdat", "completed" }.Contains(sortBy?.ToLower()))
            .WithMessage("Сортировка возможна только по полям: title, createdat, completed");
    }
}