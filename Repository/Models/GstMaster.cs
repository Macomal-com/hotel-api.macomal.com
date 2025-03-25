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
    public class GstMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int TaxPercentage { get; set; }
        public string ApplicaleServices { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class GstMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public int TaxPercentage { get; set; }
        public string ApplicaleServices { get; set; } = String.Empty;
    }
    public class GstValidator : AbstractValidator<GstMaster>
    {
        private readonly DbContextSql _context;
        public GstValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.TaxPercentage)
                .NotNull().WithMessage("Tax Percentage is required")
                .NotEmpty().WithMessage("Tax Percentage is required");
            RuleFor(x => x.ApplicaleServices)
                .NotNull().WithMessage("Applicale Services is required")
                .NotEmpty().WithMessage("Applicale Services is required");
            RuleFor(x => x)
                .MustAsync(IsUniqueService)
                .When(x => x.Id == 0)
                .WithMessage("Service already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdateService)
                .When(x => x.Id > 0)
                .WithMessage("Service already exists");
        }
        private async Task<bool> IsUniqueService(GstMaster gm, CancellationToken cancellationToken)
        {

            return !await _context.GstMaster.AnyAsync(x => x.ApplicaleServices == gm.ApplicaleServices && x.IsActive == true && x.CompanyId == gm.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateService(GstMaster gm, CancellationToken cancellationToken)
        {
            return !await _context.GstMaster.AnyAsync(x => x.ApplicaleServices == gm.ApplicaleServices && x.Id != gm.Id && x.IsActive == true && x.CompanyId == gm.CompanyId, cancellationToken);
        }
    }
}
