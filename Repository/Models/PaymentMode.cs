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
    public class PaymentMode : ICommonProperties
    {
        [Key]
        public int PaymentId { get; set; }
        public string PaymentModeName { get; set; } = String.Empty;
        public string ProviderContact { get; set; } = String.Empty;
        public string ProviderEmail { get; set; } = String.Empty;
        public double TransactionCharges { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class PaymentModeDTO
    {
        public string PaymentModeName { get; set; } = String.Empty;
        public string ProviderContact { get; set; } = String.Empty;
        public string ProviderEmail { get; set; } = String.Empty;
        public double TransactionCharges { get; set; }
    }
    public class PaymentModeValidator : AbstractValidator<PaymentMode>
    {
        public readonly DbContextSql _context;

        public PaymentModeValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.PaymentModeName)
                .NotEmpty().WithMessage("Payment Mode is required")
                .NotNull().WithMessage("Payment Mode is required");

            RuleFor(x => x.ProviderContact)
                .NotNull().WithMessage("Phone Number is required")
                .NotEmpty().WithMessage("Phone Number is required")
                .MinimumLength(10).WithMessage("Phone Number should be 10 numbers")
                .MaximumLength(10).WithMessage("Phone Number should be 10 numbers");

            RuleFor(x => x)
                .MustAsync(IsPhoneNumberExists)
                .When(x => x.PaymentId == 0)
                .WithMessage("Another Provider already registered with same phone number");

            RuleFor(x => x)
                .MustAsync(IsPhoneNumberUpdateExists)
                .When(x => x.PaymentId > 0)
                .WithMessage("Another Provider already registered with same phone number");
        }

        private async Task<bool> IsPhoneNumberExists(PaymentMode master, CancellationToken cancellationToken)
        {

            return !await _context.PaymentMode.AnyAsync(x => x.ProviderContact == master.ProviderContact && x.IsActive == true, cancellationToken);


        }

        private async Task<bool> IsPhoneNumberUpdateExists(PaymentMode master, CancellationToken cancellationToken)
        {

            return !await _context.PaymentMode.AnyAsync(x => x.ProviderContact == master.ProviderContact && x.PaymentId != master.PaymentId && x.IsActive == true, cancellationToken);


        }
    }
}

