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
    public class VendorServiceMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class VendorServiceMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
    }

    public class VendorServiceValidator : AbstractValidator<VendorServiceMaster>
    {
        private readonly DbContextSql _context;
        public VendorServiceValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.Name)
                .NotNull().WithMessage("Name is required")
                .NotEmpty().WithMessage("Name is required");
            RuleFor(x => x)
                .MustAsync(IsUniqueName)
                .When(x => x.Id == 0)
                .WithMessage("Service already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdateName)
                .When(x => x.Id > 0)
                .WithMessage("Service already exists");
        }
        private async Task<bool> IsUniqueName(VendorServiceMaster vs, CancellationToken cancellationToken)
        {

            return !await _context.VendorServiceMaster.AnyAsync(x => x.Name == vs.Name && x.IsActive == true && x.CompanyId == vs.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateName(VendorServiceMaster vs, CancellationToken cancellationToken)
        {
            return !await _context.VendorServiceMaster.AnyAsync(x => x.Name == vs.Name && x.Id != vs.Id && x.IsActive == true && x.CompanyId == vs.CompanyId, cancellationToken);
        }
    }
}
