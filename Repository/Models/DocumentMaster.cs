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
    public class DocumentMaster
    {
        [Key]
        public int DocId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string Prefix1 { get; set; } = string.Empty;
        public string Prefix2 { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public int Number { get; set; }
        public int LastNumber { get; set; }
        public string FinancialYear { get; set; } = string.Empty;
        public int CompanyId { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int CreatedBy { get;set; }

        public string Separator { get; set; } = string.Empty;
    }

    public class DocumentMasterDTO
    {
        public string Type { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string Prefix1 { get; set; } = string.Empty;
        public string Prefix2 { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public int Number { get; set; }


        public string Separator { get; set; } = string.Empty;
    }

    public class DocumentMasterValidator : AbstractValidator<DocumentMaster>
    {
        private DbContextSql _context;

        public DocumentMasterValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.Type)
                .NotNull().WithMessage("Type is required")
                .NotEmpty().WithMessage("Type is required");

            RuleFor(x => x.Prefix)
                .NotNull().WithMessage("Prefix is required")
                .NotEmpty().WithMessage("Prefix is required");

            RuleFor(x => x.Separator)
               .NotNull().WithMessage("Separator is required")
               .NotEmpty().WithMessage("Separator is required");

            RuleFor(x => x)
                .MustAsync(UniquePrefix)
                .When(x => x.DocId == 0)
                .WithMessage("Prefix already defined for this type");

            RuleFor(x => x)
                .MustAsync(UniquePrefixUpdate)
                .When(x => x.DocId > 0)
                .WithMessage("Prefix already defined for this type");

        }

        private async Task<bool> UniquePrefix(DocumentMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.DocumentMaster.AnyAsync(x => x.IsActive == true && x.CompanyId == vm.CompanyId && x.FinancialYear == vm.FinancialYear && x.Type == vm.Type, cancellationToken);
        }

        private async Task<bool> UniquePrefixUpdate(DocumentMaster vm, CancellationToken cancellationToken)
        {
            return !await _context.DocumentMaster.AnyAsync(x => x.IsActive == true && x.CompanyId == vm.CompanyId && x.FinancialYear == vm.FinancialYear && x.Type == vm.Type && x.DocId != vm.DocId, cancellationToken);
        }
    }
}
