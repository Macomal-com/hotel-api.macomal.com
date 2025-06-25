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
    public class AssetMaster : ICommonProperties
    {
        [Key]
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AssetSize { get; set; } = string.Empty;
        public string AssetRemarks { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class AssetMasterDTO
    {
        public string AssetName { get; set; } = string.Empty;
        public string AssetSize { get; set; } = string.Empty;
        public string AssetRemarks { get; set; } = string.Empty;
    }

    public class AssetValidator : AbstractValidator<AssetMaster>
    {
        private readonly DbContextSql _context;
        public AssetValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.AssetName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Asset Name is required")
                .NotEmpty().WithMessage("Asset Name is required");

            //RuleFor(x => x.AssetSize)
            //    .Cascade(CascadeMode.Stop)
            //    .NotNull().WithMessage("Asset Size is required")
            //    .NotEmpty().WithMessage("Asset Size is required");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueName)
                .When(x => x.AssetId == 0)
                .WithMessage("Name must be unique");

            RuleFor(x => x).Cascade(CascadeMode.Stop)
               .MustAsync(IsUniqueNameUpdate)
               .When(x => x.AssetId > 0)
               .WithMessage("Name must be unique");
        }

        private async Task<bool> IsUniqueName(AssetMaster cm, CancellationToken cancellationToken)
        {
            return !await _context.AssetMaster.AnyAsync(x => x.IsActive == true && x.CompanyId == cm.CompanyId && x.AssetName == cm.AssetName, cancellationToken);
        }

        private async Task<bool> IsUniqueNameUpdate(AssetMaster cm, CancellationToken cancellationToken)
        {
            return !await _context.AssetMaster.AnyAsync(x => x.IsActive == true && x.CompanyId == cm.CompanyId && x.AssetName == cm.AssetName && x.AssetId != cm.AssetId, cancellationToken);
        }


    }
}
