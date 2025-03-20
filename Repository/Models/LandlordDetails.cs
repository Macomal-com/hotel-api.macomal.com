﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;

namespace Repository.Models
{
    public class LandlordDetails : ICommonProperties
    {
        [Key]
        public int LandlordId { get; set; }
        public string LandlordName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public double CommissionPercentage { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }

    }
    public class LandlordDetailsDTO
    {
        [Key]
        public string LandlordName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public double CommissionPercentage { get; set; }
    }

    public class LandlordValidator : AbstractValidator<LandlordDetails>
    {
        public readonly DbContextSql _context;

        public LandlordValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.LandlordName)
                .NotEmpty().WithMessage("Landlord Name is required")
                .NotNull().WithMessage("Landlord Name is required");

            RuleFor(x => x.PhoneNumber)
                .NotNull().WithMessage("Phone Number is required")
                .NotEmpty().WithMessage("Phone Number is required")
                .MinimumLength(10).WithMessage("Phone Number should be 10 numbers")
                .MaximumLength(10).WithMessage("Phone Number should be 10 numbers");

            RuleFor(x => x)
                .MustAsync(IsPhoneNumberExists)
                .When(x => x.LandlordId == 0)
                .WithMessage("Another Landlord already registered with same phone number");

            RuleFor(x => x)
                .MustAsync(IsPhoneNumberUpdateExists)
                .When(x => x.LandlordId > 0)
                .WithMessage("Another Landlord already registered with same phone number");
        }

        private async Task<bool> IsPhoneNumberExists(LandlordDetails master, CancellationToken cancellationToken)
        {

            return !await _context.LandlordDetails.AnyAsync(x => x.PhoneNumber == master.PhoneNumber && x.IsActive == true, cancellationToken);


        }

        private async Task<bool> IsPhoneNumberUpdateExists(LandlordDetails master, CancellationToken cancellationToken)
        {

            return !await _context.LandlordDetails.AnyAsync(x => x.PhoneNumber == master.PhoneNumber && x.LandlordId != master.LandlordId && x.IsActive == true, cancellationToken);


        }
    }
}
