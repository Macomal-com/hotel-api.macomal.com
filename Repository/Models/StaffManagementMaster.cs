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
    public class StaffManagementMaster : ICommonProperties
    {
        [Key]
        public int StaffId { get; set; }
        public string StaffName { get; set; } = String.Empty;
        public int DesignationId { get; set; }
        public string PhoneNo { get; set; } = String.Empty;
        public double Salary { get; set; }
        public int DepartmentId { get; set; }
        public int VendorId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class StaffManagementMasterDTO
    {
        public string StaffName { get; set; } = String.Empty;
        public int DesignationId { get; set; }
        public string StaffDesignation { get; set; } = String.Empty;
        public string PhoneNo { get; set; } = String.Empty;
        public double Salary { get; set; }
        public string Department { get; set; } = String.Empty;
        public int DepartmentId { get; set; }
        public int VendorId { get; set; }
    }

    public class StaffValidator : AbstractValidator<StaffManagementMaster>
    {
        private readonly DbContextSql _context;
        public StaffValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.StaffName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Staff Name is required")
                .NotEmpty().WithMessage("Staff Name is required");
            RuleFor(x => x.PhoneNo)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Phone Number is required")
                .NotEmpty().WithMessage("Phone Number is required")
                .MinimumLength(10).WithMessage("Phone Number should be 10 numbers")
                .MaximumLength(10).WithMessage("Phone Number should be 10 numbers");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsPhoneNumberExists)
                .When(x => x.StaffId == 0 && x.VendorId != 0)
                .WithMessage("Another Staff Member already registered with same phone number");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsPhoneNumberUpdateExists)
                .When(x => x.StaffId > 0 && x.VendorId != 0)
                .WithMessage("Another Staff Member already registered with same phone number");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsPhoneNumberExistsVendor)
                .When(x => x.StaffId == 0 && x.VendorId == 0)
                .WithMessage("Another Staff Member already registered with same phone number");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsPhoneNumberUpdateExistsVendor)
                .When(x => x.StaffId > 0 && x.VendorId == 0)
                .WithMessage("Another Staff Member already registered with same phone number");
        }
        private async Task<bool> IsPhoneNumberExists(StaffManagementMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.StaffManagementMaster.AnyAsync(x => x.PhoneNo == vm.PhoneNo && x.VendorId != 0 && x.IsActive == true, cancellationToken);
        }

        private async Task<bool> IsPhoneNumberUpdateExists(StaffManagementMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.StaffManagementMaster.AnyAsync(x => x.PhoneNo == vm.PhoneNo && x.VendorId != 0 && x.StaffId != vm.StaffId && x.IsActive == true, cancellationToken);
        }

        private async Task<bool> IsPhoneNumberExistsVendor(StaffManagementMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.StaffManagementMaster.AnyAsync(x => x.PhoneNo == vm.PhoneNo && x.VendorId == 0 && x.IsActive == true, cancellationToken);
        }

        private async Task<bool> IsPhoneNumberUpdateExistsVendor(StaffManagementMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.StaffManagementMaster.AnyAsync(x => x.PhoneNo == vm.PhoneNo && x.VendorId == 0 && x.StaffId != vm.StaffId && x.IsActive == true, cancellationToken);
        }
    }
}
