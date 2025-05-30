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
    public class AgentDetails: ICommonProperties
    {
        [Key]
        public int AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string AgentType { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Commission { get; set; }
        public int Tcs { get; set; }
        public int Tds { get; set; }
        public string GstNo { get; set; } = string.Empty;
        public int GstPercentage { get; set; }
        public string GstType { get; set; } = string.Empty;
        public string ContractFile { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class AgentDetailsDTO
    {
        public string AgentName { get; set; } = string.Empty;
        public string AgentType { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Commission { get; set; }
        public int Tcs { get; set; }
        public int Tds { get; set; }
        public string GstNo { get; set; } = string.Empty;
        public int GstPercentage { get; set; }
        public string GstType { get; set; } = string.Empty;
    }

    public class AgentDetailValidator : AbstractValidator<AgentDetails>
    {
        private readonly DbContextSql _context;
        public AgentDetailValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.AgentName)
                .NotNull().WithMessage("Agent Name is required")
                .NotEmpty().WithMessage("Agent Name is required");

            RuleFor(x => x.AgentType)
                .NotNull().WithMessage("Agent Type is required")
                .NotEmpty().WithMessage("Agent Type is required");

            //RuleFor(x => x.GstNo)
            //    .NotEmpty().WithMessage("GST No is required")
            //    .NotNull().WithMessage("GST No is required");

            //RuleFor(x => x.GstNo)
            //    .Length(15)
            //    .WithMessage("GST No length should be 15 numbers");

            RuleFor(x => x.Commission)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Commission cannot be negative");

            RuleFor(x => x.Tds)
                .GreaterThanOrEqualTo(0)
                .WithMessage("TDS cannot be negative");

            RuleFor(x => x.Tcs)
                .GreaterThanOrEqualTo(0)
                .WithMessage("TCS cannot be negative");
            RuleFor(x => x)
                .MustAsync(IsUniqueNameNumber)
                .When(x => x.AgentId == 0)
                .WithMessage("Name or Number must be unique");

            RuleFor(x => x)
               .MustAsync(IsUniqueNameNumberUpdate)
               .When(x => x.AgentId > 0)
               .WithMessage("Name or Number must be unique");
        }

        private async Task<bool> IsUniqueNameNumber(AgentDetails cm, CancellationToken cancellationToken)
        {
            return !await _context.AgentDetails.AnyAsync(x => x.IsActive == true && x.CompanyId == cm.CompanyId && x.AgentName == cm.AgentName && x.ContactNo == cm.ContactNo, cancellationToken);
        }

        private async Task<bool> IsUniqueNameNumberUpdate(AgentDetails cm, CancellationToken cancellationToken)
        {
            return !await _context.AgentDetails.AnyAsync(x => x.IsActive == true && x.CompanyId == cm.CompanyId && x.AgentName == cm.AgentName && x.ContactNo == cm.ContactNo && x.AgentId != cm.AgentId, cancellationToken);
        }


    }
}
