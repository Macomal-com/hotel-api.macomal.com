using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System.ComponentModel.DataAnnotations;

namespace Repository.Models
{
    public class BedTypeMaster : ICommonProperties
    {
        [Key]
        public int BedTypeId { get; set; }
        public string BedType { get; set; } = String.Empty;
        public string BedTypeDescription { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class BedTypeMasterDTO
    {
        [Key]
        public int BedTypeId { get; set; }
        public string BedType { get; set; } = String.Empty;
        public string BedTypeDescription { get; set; } = String.Empty;

    }

    public class BedTypeValidator : AbstractValidator<BedTypeMaster>
    {
        private readonly DbContextSql _context;

        public BedTypeValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.BedType)
                .NotNull().WithMessage("Bed Type cannot be null")
                .NotEmpty().WithMessage("Bed Type is required");
            RuleFor(x => x)
                .MustAsync(IsUniqueBedTypeName)
                .When(x => x.BedTypeId == 0)
                .WithMessage("BedType name already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdateBedTypeName)
                .When(x => x.BedTypeId > 0)
                .WithMessage("BedType Name already exists");

        }
        private async Task<bool> IsUniqueBedTypeName(BedTypeMaster bedTypeMaster, CancellationToken cancellationToken)
        {
            var data = await _context.BedTypeMaster.AnyAsync(x => x.BedType == bedTypeMaster.BedType && x.IsActive == true && x.CompanyId == bedTypeMaster.CompanyId, cancellationToken);
            return !await _context.BedTypeMaster.AnyAsync(x => x.BedType == bedTypeMaster.BedType && x.IsActive == true && x.CompanyId == bedTypeMaster.CompanyId, cancellationToken);

        }


        private async Task<bool> IsUniqueUpdateBedTypeName(BedTypeMaster bedTypeMaster, CancellationToken cancellationToken)
        {

            return !await _context.BedTypeMaster.AnyAsync(x => x.BedType == bedTypeMaster.BedType && x.BedTypeId != bedTypeMaster.BedTypeId && x.IsActive == true && x.CompanyId == bedTypeMaster.CompanyId, cancellationToken);


        }

    }

}
