// Validators/LoginDtoValidator.cs
using FluentValidation;
using TaskManagementSystem.API.Models.Auth;

namespace TaskManagementSystem.API.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен для заполнения")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для заполнения");
    }
}