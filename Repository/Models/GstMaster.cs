using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class GstMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public decimal TaxPercentage { get; set; }
        public string ApplicableServices { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }

        public string GstType { get; set; } = string.Empty;
        public decimal RangeStart { get; set; }
        public decimal RangeEnd { get; set; }

        [NotMapped]
        public List<GstRangeMaster> ranges { get; set; } = new List<GstRangeMaster>();
    }
    public class GstMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public decimal TaxPercentage { get; set; }
        public string ApplicableServices { get; set; } = string.Empty;
        public string GstType { get; set; } = string.Empty;

        public List<GstRangeMaster> ranges { get; set; } = new List<GstRangeMaster>();

    }

    public class GstRangeMaster : ICommonProperties
    {
        [Key]
        public int RangeId { get; set; }

        public decimal TaxPercentage { get; set; }
        public decimal RangeStart { get; set; }
        public decimal RangeEnd { get; set; }
        public int GstId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
    }
    public class GstValidator : AbstractValidator<GstMaster>
    {
        private readonly DbContextSql _context;
        public GstValidator(DbContextSql context)
        {
            _context = context;
            
            RuleFor(x => x.ApplicableServices)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Applicale Services is required")
                .NotEmpty().WithMessage("Applicale Services is required");

            RuleFor(x => x.GstType)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("GST Type is required")
                .NotEmpty().WithMessage("GST Type is required");
            RuleFor(x => x.TaxPercentage)
                .NotNull().WithMessage("Tax Percentage is required")
                .NotEmpty().WithMessage("Tax Percentage is required")
                .When(x => x.GstType == "Single");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueService)
                .When(x => x.Id == 0)
                .WithMessage("Service already exists");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueUpdateService)
                .When(x => x.Id > 0)
                .WithMessage("Service already exists");

            RuleFor(x => x.ranges)
                .Cascade(CascadeMode.Stop)
               .NotEmpty() // Ensure that the OrderItems collection is not empty
               .WithMessage("Must have one range")
            //   .Must(x=> !x.All(r => r.TaxPercentage == 0 && r.RangeStart == 0 && r.RangeEnd == 0))
               .When(x => x.GstType == "Multiple");

            RuleFor(o => o.ranges)
                .Cascade(CascadeMode.Stop)
               .ForEach(oi => oi.SetValidator(new GstRangeValidator()))
            .When(x => x.GstType == "Multiple");



        }
        private async Task<bool> IsUniqueService(GstMaster gm, CancellationToken cancellationToken)
        {

            return !await _context.GstMaster.AnyAsync(x => x.ApplicableServices == gm.ApplicableServices && x.IsActive == true && x.CompanyId == gm.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateService(GstMaster gm, CancellationToken cancellationToken)
        {
            return !await _context.GstMaster.AnyAsync(x => x.ApplicableServices == gm.ApplicableServices && x.Id != gm.Id && x.IsActive == true && x.CompanyId == gm.CompanyId, cancellationToken);
        }
    }

    public class GstRangeValidator : AbstractValidator<GstRangeMaster>
    {
        public GstRangeValidator()
        {
            RuleFor(x => x.TaxPercentage)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Tax Percentage is required")
                .NotEmpty().WithMessage("Tax Percentage is required")
                .GreaterThanOrEqualTo(0).WithMessage("Tax Percentage cannot be negative")
                .When(x => x.RangeStart > 0 || x.RangeEnd > 0);

            RuleFor(x => x.RangeStart)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(0).WithMessage("Start Range cannot be negative")
                .LessThan(x => x.RangeEnd).WithMessage("Start Range should be less than end range")
                .When(x => x.TaxPercentage > 0);

            RuleFor(x => x.RangeEnd)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("End Range is required")
                .NotEmpty().WithMessage("End Range is required")
                .GreaterThanOrEqualTo(0).WithMessage("End Range cannot be negative")
                .When(x => x.TaxPercentage > 0);

            
        }
    }
}
