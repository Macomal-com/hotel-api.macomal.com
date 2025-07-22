using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserDetails
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SecurityQuestion { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int? CompanyId { get; set; } 
        public string CompanyName { get; set; } = string.Empty;
        public string DBName { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public int? BranchId { get; set; } 
        public int? CreatedBy { get; set; }
        public int AgentId { get; set; } = 0;
        public string Status { get; set; } = string.Empty;
        public int? City { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public int AccessId { get; set; } = 0;
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? Other1 { get; set; }
        public int? Other2 { get; set; }
        public string Other3 { get; set; } = string.Empty;
        public string Other4 { get; set; } = string.Empty;
        public string Other5 { get; set; } = string.Empty;
        public int RefUserId { get; set; }

        [NotMapped]
        public int MainUserId { get; set; }
        
    }

    public class UserDetailsDTO
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
    }
    public class UserValidator : AbstractValidator<UserDetails>
    {
        private readonly DbContextSql _context;
        public UserValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.UserName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("User Name is required")
                .NotEmpty().WithMessage("User Name is required");
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Password is required")
                .NotEmpty().WithMessage("Password is required");
            RuleFor(x => x.Roles).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Role is required")
                .NotEmpty().WithMessage("Role is required");
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Name is required")
                .NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.EmailId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("EmailId is required")
                .NotEmpty().WithMessage("EmailId is required");
            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueUserName)
                .When(x => x.UserId == 0)
                .WithMessage("UserName already exists");

            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueUpdateUserName)
                .When(x => x.UserId > 0)
                .WithMessage("UserName already exists");
        }
        private async Task<bool> IsUniqueUserName(UserDetails ud, CancellationToken cancellationToken)
        {
            return !await _context.UserDetails.AnyAsync(x => x.UserName == ud.UserName && x.IsActive == true && x.CompanyId == ud.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateUserName(UserDetails ud, CancellationToken cancellationToken)
        {
            return !await _context.UserDetails.AnyAsync(x => x.UserName == ud.UserName && x.UserId != ud.UserId && x.IsActive == true && x.CompanyId == ud.CompanyId, cancellationToken);
        }
    }
}
