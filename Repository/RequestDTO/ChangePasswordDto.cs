using FluentValidation;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.RequestDTO
{
    public class ChangePasswordDto
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.UserId).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Invalid Userid");


            RuleFor(x => x.NewPassword).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("New password is required")
                .NotEmpty().WithMessage("New password is required");


            RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
               .NotNull().WithMessage("Confirm password is required")
               .NotEmpty().WithMessage("Confirm password is required");

            RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
              .NotNull().WithMessage("Confirm password is required")
              .NotEmpty().WithMessage("Confirm password is required");

            RuleFor(x => x).Cascade(CascadeMode.Stop)
              .Must(x => x.NewPassword == x.ConfirmPassword)
             .WithMessage("Mismatch new password and confirm password");
             
        }
    }
}
