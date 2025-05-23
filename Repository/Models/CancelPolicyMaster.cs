using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class CancelPolicyMaster : ICommonProperties
    {  
        public int Id { get; set; }
        public string PolicyCode { get; set; } = String.Empty;
        public string DeductionBy { get; set; } = String.Empty;
        public string ChargesApplicableOn { get; set; } = String.Empty;
        public decimal DeductionAmount { get; set; }
        public string CancellationTime { get; set; } = String.Empty;
        public int FromTime { get; set; }
        public int ToTime { get; set; }
        public int MinRoom { get; set; }
        public int MaxRoom { get; set; }
        public string PolicyDescription { get; set; } = String.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class CancelPolicyMasterDTO
    {
        public int Id { get; set; }
        public string PolicyCode { get; set; } = String.Empty;
        public string DeductionBy { get; set; } = String.Empty;
        public string ChargesApplicableOn { get; set; } = String.Empty;
        public decimal DeductionAmount { get; set; }
        public string CancellationTime { get; set; } = String.Empty;
        public int FromTime { get; set; }
        public int ToTime { get; set; }
        public int MinRoom { get; set; }
        public int MaxRoom { get; set; }
        public string PolicyDescription { get; set; } = String.Empty;
    }
    public class CancelPolicyValidator : AbstractValidator<CancelPolicyMaster>
    {
        private DbContextSql _context;
        public CancelPolicyValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.PolicyCode)
                .NotEmpty().WithMessage("Policy Code is required")
                .NotNull().WithMessage("Policy Code is required");

            RuleFor(x => x.PolicyDescription)
                .NotEmpty().WithMessage("Policy Description is required")
                .NotNull().WithMessage("Policy Description is required");

            //RuleFor(x => x.CancellationTime)
            //   .NotEmpty().WithMessage("Cancellation Time is required")
            //   .NotNull().WithMessage("Cancellation Time is required");

            //RuleFor(x => x.MaxRoom)
            //    .NotNull().WithMessage("Max Room is required")
            //    .NotEmpty().WithMessage("Max Room is required")
            //    .GreaterThan(0).WithMessage("Max Room is required");

            //RuleFor(x => x.MinRoom)
            //    .NotNull().WithMessage("Min Room is required")
            //    .NotEmpty().WithMessage("Min Room is required")
            //    .GreaterThan(0).WithMessage("Min Room is required");

            RuleFor(x => x)
                .MustAsync(IsUniquePolicyCode)
                .When(x => x.Id == 0)
                .WithMessage("Policy Code must be unique");
        }

        private async Task<bool> IsUniquePolicyCode(CancelPolicyMaster master, CancellationToken cancellationToken)
        {

            return !await _context.CancelPolicyMaster.AnyAsync(x => x.PolicyCode == master.PolicyCode && x.IsActive == true, cancellationToken);
        }
    }
}
