// Validators/RegisterDtoValidator.cs
using FluentValidation;
using TaskManagementSystem.API.Models.Auth;

namespace TaskManagementSystem.API.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен для заполнения")
            .EmailAddress().WithMessage("Некорректный формат email")
            .MaximumLength(100).WithMessage("Email не может превышать 100 символов");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя обязательно для заполнения")
            .MaximumLength(50).WithMessage("Имя не может превышать 50 символов")
            .Matches("^[a-zA-Zа-яА-Я\\s-]+$").WithMessage("Имя может содержать только буквы, пробелы и дефисы");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна для заполнения")
            .MaximumLength(50).WithMessage("Фамилия не может превышать 50 символов")
            .Matches("^[a-zA-Zа-яА-Я\\s-]+$").WithMessage("Фамилия может содержать только буквы, пробелы и дефисы");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для заполнения")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов")
            .MaximumLength(100).WithMessage("Пароль не может превышать 100 символов")
            .Matches("[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches("[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches("[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Matches("[^a-zA-Z0-9]").WithMessage("Пароль должен содержать хотя бы один специальный символ");
    }
}