﻿using FluentValidation;
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
    public class VendorMaster : ICommonProperties
    {
        [Key]
        public int VendorId { get; set; }
        public string VendorName { get; set; } = String.Empty;
        public string VendorEmail { get; set; } = String.Empty;
        public string VendorPhone { get; set; } = String.Empty;
        public string CompanyName { get; set; } = String.Empty;
        public int ServiceId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class VendorMasterDTO
    {
        public string VendorName { get; set; } = String.Empty;
        public string VendorEmail { get; set; } = String.Empty;
        public string VendorPhone { get; set; } = String.Empty;
        public string CompanyName { get; set; } = String.Empty;
        public int ServiceId { get; set; }
    }

    public class VendorValidator : AbstractValidator<VendorMaster>
    {
        private readonly DbContextSql _context;
        public VendorValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.VendorName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Vendor Name is required")
                .NotEmpty().WithMessage("Vendor Name is required");
            RuleFor(x => x.VendorPhone)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Phone Number is required")
                .NotEmpty().WithMessage("Phone Number is required")
                .MinimumLength(10).WithMessage("Phone Number should be 10 numbers")
                .MaximumLength(10).WithMessage("Phone Number should be 10 numbers");
            RuleFor(x => x.ServiceId)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Service Id is required")
                .NotEmpty().WithMessage("Service Id is required");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsPhoneNumberExists)
                .When(x => x.VendorId == 0)
                .WithMessage("Another Vendor already registered with same phone number");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsPhoneNumberUpdateExists)
                .When(x => x.VendorId > 0)
                .WithMessage("Another Vendor already registered with same phone number updated");
        }
        private async Task<bool> IsPhoneNumberExists(VendorMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.VendorMaster.AnyAsync(x => x.VendorPhone == vm.VendorPhone && x.IsActive == true, cancellationToken);
        }

        private async Task<bool> IsPhoneNumberUpdateExists(VendorMaster vm, CancellationToken cancellationToken)
        {
            var data = await _context.VendorMaster.AnyAsync(x => x.VendorPhone == vm.VendorPhone && x.VendorId != vm.VendorId && x.IsActive == true, cancellationToken);
            return !await _context.VendorMaster.AnyAsync(x => x.VendorPhone == vm.VendorPhone && x.VendorId != vm.VendorId && x.IsActive == true, cancellationToken);
        }
    }
    public class VendorDeleteValidator : AbstractValidator<VendorMaster>
    {
        private readonly DbContextSql _context;
        public VendorDeleteValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(DoVendorExists)
                .When(x => x.IsActive == false)
                .WithMessage("You can't delete this Vendor, History already exists!");
        }
        private async Task<bool> DoVendorExists(VendorMaster rem, CancellationToken cancellationToken)
        {
            return !await _context.VendorHistoryMaster
                .Where(x => x.IsActive == true && x.CompanyId == rem.CompanyId && x.VendorId == rem.VendorId)
                .AnyAsync();
        }

    }
}
