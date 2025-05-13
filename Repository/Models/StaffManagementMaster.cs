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
    public class StaffManagementMaster : ICommonProperties
    {
        [Key]
        public int StaffId { get; set; }
        public string StaffName { get; set; } = String.Empty;
        public string StaffDesignation { get; set; } = String.Empty;
        public string PhoneNo { get; set; } = String.Empty;
        public double Salary { get; set; }
        public string Department { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class StaffManagementMasterDTO
    {
        public string StaffName { get; set; } = String.Empty;
        public string StaffDesignation { get; set; } = String.Empty;
        public string PhoneNo { get; set; } = String.Empty;
        public double Salary { get; set; }
        public string Department { get; set; } = String.Empty;
    }

    public class StaffValidator : AbstractValidator<StaffManagementMaster>
    {
        private readonly DbContextSql _context;
        public StaffValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.StaffName)
                .NotNull().WithMessage("Staff Name is required")
                .NotEmpty().WithMessage("Staff Name is required");
            RuleFor(x => x.PhoneNo)
                .NotNull().WithMessage("Phone Number is required")
                .NotEmpty().WithMessage("Phone Number is required")
                .MinimumLength(10).WithMessage("Phone Number should be 10 numbers")
                .MaximumLength(10).WithMessage("Phone Number should be 10 numbers");
            RuleFor(x => x)
                .MustAsync(IsPhoneNumberExists)
                .When(x => x.StaffId == 0)
                .WithMessage("Another Staff Member already registered with same phone number");

            RuleFor(x => x)
                .MustAsync(IsPhoneNumberUpdateExists)
                .When(x => x.StaffId > 0)
                .WithMessage("Another Staff Member already registered with same phone number");
        }
        private async Task<bool> IsPhoneNumberExists(StaffManagementMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.StaffManagementMaster.AnyAsync(x => x.PhoneNo == vm.PhoneNo && x.IsActive == true, cancellationToken);
        }

        private async Task<bool> IsPhoneNumberUpdateExists(StaffManagementMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.StaffManagementMaster.AnyAsync(x => x.PhoneNo == vm.PhoneNo && x.StaffId != vm.StaffId && x.IsActive == true, cancellationToken);
        }
    }
}
