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
    public class HourMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int Hour { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class HourMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public int Hour { get; set; }
    }

    public class HourValidator : AbstractValidator<HourMaster>
    {
        private readonly DbContextSql _context;
        public HourValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.Hour)
                .NotNull().WithMessage("Hour is required")
                .NotEmpty().WithMessage("Hour is required");
            RuleFor(x => x)
                .MustAsync(IsUniqueHour)
                .When(x => x.Id == 0)
                .WithMessage("Hour already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdateHour)
                .When(x => x.Id > 0)
                .WithMessage("Hour already exists");
        }
        private async Task<bool> IsUniqueHour(HourMaster cm, CancellationToken cancellationToken)
        {

            return !await _context.HourMaster.AnyAsync(x => x.Hour == cm.Hour && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateHour(HourMaster cm, CancellationToken cancellationToken)
        {
            return !await _context.HourMaster.AnyAsync(x => x.Hour == cm.Hour && x.Id != cm.Id && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
        }
    }
}
