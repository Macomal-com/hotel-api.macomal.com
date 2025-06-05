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
    public class SubGroupMaster : ICommonProperties
    {
        [Key]
        public int SubGroupId { get; set; }
        public string SubGroupName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int GroupId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class SubGroupMasterDTO
    {
        [Key]
        public int SubGroupId { get; set; }
        public string SubGroupName { get; set; } = String.Empty;
        public int GroupId { get; set; }
        public string Description { get; set; } = String.Empty;

    }

    public class SubGroupValidator : AbstractValidator<SubGroupMaster>
    {
        private readonly DbContextSql _context;
        public SubGroupValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.SubGroupName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("SubGroup Name cannot be null")
                .NotEmpty().WithMessage("SubGroup Name is required");
            RuleFor(x => x.GroupId)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Group Name cannot be null")
                .NotEmpty().WithMessage("Group Name is required");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueSubGroupName)
                .When(x => x.SubGroupId == 0)
                .WithMessage("SubGroup Name already exists");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueUpdateSubgroupName)
                .When(x => x.SubGroupId > 0)
                .WithMessage("SubGroup Name already exists");
        }
        private async Task<bool> IsUniqueSubGroupName(SubGroupMaster subGroupMaster, CancellationToken cancellationToken)
        {
            return !await _context.SubGroupMaster.AnyAsync(x => x.SubGroupName == subGroupMaster.SubGroupName && x.GroupId == subGroupMaster.GroupId && x.IsActive == true && x.CompanyId == subGroupMaster.CompanyId, cancellationToken);
        }

        private async Task<bool> IsUniqueUpdateSubgroupName(SubGroupMaster subGroupMaster, CancellationToken cancellationToken)
        {
            return !await _context.SubGroupMaster.AnyAsync(x => x.SubGroupName == subGroupMaster.SubGroupName && x.SubGroupId != subGroupMaster.SubGroupId && x.IsActive == true && x.CompanyId == subGroupMaster.CompanyId, cancellationToken);
        }
    }

    public class SubGroupDeleteValidator : AbstractValidator<SubGroupMaster>
    {
        private readonly DbContextSql _context;
        public SubGroupDeleteValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(DoSubGroupExists)
                .When(x => x.IsActive == false)
                .WithMessage("You can't delete this Subgroup, service already exists!");
        }
        private async Task<bool> DoSubGroupExists(SubGroupMaster groupMaster, CancellationToken cancellationToken)
        {
            return !await _context.ServicableMaster.Where(x => x.IsActive == true && x.CompanyId == groupMaster.CompanyId && x.SubGroupId == groupMaster.SubGroupId).AnyAsync();

        }
    }
}
