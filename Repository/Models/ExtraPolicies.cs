using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ExtraPolicies : ICommonProperties
    {
        [Key]
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;        
        public int FromHour { get; set; }
        public int ToHour { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
    }
    public class ExtraPoliciesDTO
    {
        public string PolicyName { get; set; } = string.Empty;
        public int FromHour { get; set; }
        public int ToHour { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
    public class ExtrapolicyValidator : AbstractValidator<ExtraPolicies>
    {
        private readonly DbContextSql _context;
        public ExtrapolicyValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.PolicyName)
                .NotNull().WithMessage("Policy Name is required")
                .NotEmpty().WithMessage("Policy Name is required");
            RuleFor(x => x.Status)
                .NotNull().WithMessage("Status is required")
                .NotEmpty().WithMessage("Status is required");
            RuleFor(x => x.ToHour)
                .NotNull().WithMessage("ToHour is required")
                .NotEmpty().WithMessage("ToHour is required");
            RuleFor(x => x.Amount)
                .NotNull().WithMessage("Amount is required")
                .NotEmpty().WithMessage("Amount is required");
            RuleFor(x => x)
                .MustAsync(IsUniquePolicyName)
                .When(x => x.PolicyId == 0)
                .WithMessage("Hour already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdatePolicyName)
                .When(x => x.PolicyId > 0)
                .WithMessage("Hour already exists");
        }
        private async Task<bool> IsUniquePolicyName(ExtraPolicies cm, CancellationToken cancellationToken)
        {

            return !await _context.ExtraPolicies.AnyAsync(x => x.PolicyName == cm.PolicyName && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdatePolicyName(ExtraPolicies cm, CancellationToken cancellationToken)
        {
            return !await _context.ExtraPolicies.AnyAsync(x => x.PolicyName == cm.PolicyName && x.PolicyId != cm.PolicyId && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
        }
    }
}
