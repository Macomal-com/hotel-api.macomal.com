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
    public class GroupMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = String.Empty;
        public string GroupName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int Other1 { get; set; }
        public int Other2 { get; set; }
        public decimal GST { get; set; }
        public decimal IGST { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
    }

    public class GroupMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = String.Empty;
        public string GroupName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public decimal GST { get; set; }
        public decimal IGST { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
    }

    public class GroupValidator : AbstractValidator<GroupMaster>
    {
        private readonly DbContextSql _context;
        public GroupValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.Code)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Code is required")
                .NotEmpty().WithMessage("Code is required");
            RuleFor(x => x.GroupName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Group Name is required")
                .NotEmpty().WithMessage("Group Name is required");

            RuleFor(x => x.GST)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(0).WithMessage("Gst can't be negative");

            RuleFor(x => x.GST)
                .Cascade(CascadeMode.Stop)
                .LessThanOrEqualTo(100).WithMessage("Gst can't be more than 100");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueGroupCode)
                .When(x => x.Id == 0)
                .WithMessage("Group Code already exists");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueUpdateGroupCode)
                .When(x => x.Id > 0)
                .WithMessage("Group Code already exists");
        }
        private async Task<bool> IsUniqueGroupCode(GroupMaster groupMaster, CancellationToken cancellationToken)
        {
            
            return !await _context.GroupMaster.AnyAsync(x => x.Code == groupMaster.Code && x.IsActive == true && x.CompanyId == groupMaster.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateGroupCode(GroupMaster groupMaster, CancellationToken cancellationToken)
        {
            return !await _context.GroupMaster.AnyAsync(x => x.Code == groupMaster.Code && x.Id != groupMaster.Id && x.IsActive == true && x.CompanyId == groupMaster.CompanyId, cancellationToken);
        }
    }

    public class GroupDeleteValidator : AbstractValidator<GroupMaster>
    {
        private readonly DbContextSql _context;
        public GroupDeleteValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(DoSubGroupExists)
                .When(x => x.IsActive == false)
                .WithMessage("You can't delete this Group, sub-group already exists!");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(DoSubGroupInServiceExists)
                .When(x => x.IsActive == false)
                .WithMessage("You can't delete this Group, service already exists!");
        }
        private async Task<bool> DoSubGroupExists(GroupMaster groupMaster, CancellationToken cancellationToken)
        {
            return !await _context.SubGroupMaster.Where(x => x.IsActive == true && x.CompanyId == groupMaster.CompanyId && x.GroupId == groupMaster.Id).AnyAsync();

        }
        private async Task<bool> DoSubGroupInServiceExists(GroupMaster groupMaster, CancellationToken cancellationToken)
        {
            return !await _context.ServicableMaster.Where(x => x.IsActive == true && x.CompanyId == groupMaster.CompanyId && x.ServiceId == groupMaster.Id).AnyAsync();
        }
    }
}
