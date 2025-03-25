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
    public class CommissionMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; } = String.Empty;
        public string Charge { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class CommissionMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; } = String.Empty;
        public string Charge { get; set; } = String.Empty;
    }

    public class CommissionValidator : AbstractValidator<CommissionMaster>
    {
        private readonly DbContextSql _context;
        public CommissionValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.Type)
                .NotNull().WithMessage("Type is required")
                .NotEmpty().WithMessage("Type is required");
            RuleFor(x => x.Charge)
                .NotNull().WithMessage("Charge is required")
                .NotEmpty().WithMessage("Charge is required");
            RuleFor(x => x)
                .MustAsync(IsUniqueType)
                .When(x => x.Id == 0)
                .WithMessage("Commission already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdateType)
                .When(x => x.Id > 0)
                .WithMessage("Commission already exists");
        }
        private async Task<bool> IsUniqueType(CommissionMaster cm, CancellationToken cancellationToken)
        {

            return !await _context.CommissionMaster.AnyAsync(x => x.Type == cm.Type && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateType(CommissionMaster cm, CancellationToken cancellationToken)
        {
            return !await _context.CommissionMaster.AnyAsync(x => x.Type == cm.Type && x.Id != cm.Id && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
        }
    }
}
