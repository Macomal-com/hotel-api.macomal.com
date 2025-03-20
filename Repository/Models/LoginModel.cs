using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginModalValidator : AbstractValidator<LoginModel>
    {
        public LoginModalValidator()
        {
            RuleFor(x => x.Username)
                .NotNull().WithMessage("User Name is required")
                .NotEmpty().WithMessage("User Name is required");

            RuleFor(x => x.Password)
                .NotNull().WithMessage("Password is required")
                .NotEmpty().WithMessage("Password  is required");
        }
    }
}
