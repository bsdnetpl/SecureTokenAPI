using FluentValidation;
using System.Collections.Generic;
using System.ComponentModel;

namespace SecureTokenAPI.Models
    {
    public class UserToken
        {
        public int Id { get; set; }
        public Guid Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; } = "user";
        }

    public class UserTokenValidator : AbstractValidator<UserToken>
        {
        public UserTokenValidator()
            {
            // Walidacja UserName
            RuleFor(user => user.UserName)
                .NotEmpty().WithMessage("Username cannot be empty.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.");

            // Walidacja Password
            RuleFor(user => user.Password)
                 .NotEmpty().WithMessage("Password cannot be empty.")
                 .MinimumLength(10).WithMessage("Password must be at least 10 characters long.")
                 .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                 .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                 .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
                 .Matches(@"[!@#$%^&*()\-_=\[\]{};:'"",<>\./?]").WithMessage("Password must contain at least one special character.");


            // Walidacja Email
            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email address format.");

            // Walidacja Role
            RuleFor(user => user.Role)
                .NotEmpty().WithMessage("Role cannot be empty.")
                .Must(role => role == "user" || role == "admin")
                .WithMessage("Role must be either 'user' or 'admin'.");
            }
        }
    }
