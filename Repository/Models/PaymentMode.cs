﻿using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
        public string TransactionType { get; set; } = String.Empty;
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
        public string TransactionType { get; set; } = String.Empty;
    }
    public class PaymentModeValidator : AbstractValidator<PaymentMode>
    {
        public readonly DbContextSql _context;

        public PaymentModeValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.PaymentModeName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Payment Mode is required")
                .NotNull().WithMessage("Payment Mode is required");
            //RuleFor(x => x.TransactionCharges)
            //    .NotEmpty().WithMessage("Transaction Charges is required")
            //    .NotNull().WithMessage("Transaction Charges is required");

            RuleFor(x => x.TransactionType)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Transaction Type is required")
                .When(x => x.TransactionCharges > 0)
                .NotNull().WithMessage("Transaction Type is required")
                .When(x=>x.TransactionCharges > 0);
            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(IsModeExists)
                .When(x => x.PaymentId == 0)
                .WithMessage("Mode Name already registered");

            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(IsModeUpdateExists)
                .When(x => x.PaymentId > 0)
                .WithMessage("Mode Name already registered");
        }
        private async Task<bool> IsModeExists(PaymentMode master, CancellationToken cancellationToken)
        {

            return !await _context.PaymentMode.AnyAsync(x => x.PaymentModeName == master.PaymentModeName && x.IsActive == true && x.CompanyId == master.CompanyId, cancellationToken);


        }

        private async Task<bool> IsModeUpdateExists(PaymentMode master, CancellationToken cancellationToken)
        {

            return !await _context.PaymentMode.AnyAsync(x => x.PaymentModeName == master.PaymentModeName && x.CompanyId == master.CompanyId && x.PaymentId != master.PaymentId && x.IsActive == true, cancellationToken);


        }
    }
}

