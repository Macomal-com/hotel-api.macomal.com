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
    public class ServicableMaster : ICommonProperties
    {
        [Key]
        public int ServiceId { get; set; }
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public string ServiceName { get; set; } = String.Empty;
        public string ServiceDescription { get; set; } = String.Empty;
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public string TaxType { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class ServicableMasterDTO
    {
        public int ServiceId { get; set; }
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public string ServiceName { get; set; } = String.Empty;
        public string ServiceDescription { get; set; } = String.Empty;
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public string TaxType { get; set; } = String.Empty;
    }

    public class ServicesDTO
    {
        public int ServiceId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int SubGroupId { get; set; }
        public string SubGroupName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public decimal Amount { get; set; } // amount that show in table
        public decimal Discount { get; set; }
        public string TaxType { get; set; } = string.Empty;

        public decimal GstPercentage { get; set; }
        public decimal GstAmount { get; set; }
        public decimal IgstPercentage { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal SgstPercentage { get; set; }

        public decimal SgstAmount { get; set; }
        public decimal CgstPercentage { get; set; }
        public decimal CgstAmount { get; set; }

        public decimal InclusiveTotalAmount { get; set; } //inclusive total amount
        public decimal ExclusiveTotalAmount { get; set; } //exclusive total amount
        public decimal ServicePrice { get; set; } // set to service price in advanceservices

        public int Quantity { get; set; }

        public decimal DiscountAmount { get; set; }
        public int BookingId { get; set; }
        public string KotNo { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateOnly ServiceDate { get; set; }
        public string ServiceTime { get; set; } = string.Empty;
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ServiveValidator : AbstractValidator<ServicableMaster>
    {
        private readonly DbContextSql _context;
        public ServiveValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.ServiceName)
                .NotNull().WithMessage("Service Name cannot be null")
                .NotEmpty().WithMessage("Service Name is required");
            RuleFor(x => x.GroupId)
                .NotNull().WithMessage("Group Id cannot be null")
                .NotEmpty().WithMessage("Group Id Name is required");
            RuleFor(x => x.SubGroupId)
                .NotNull().WithMessage("SubGroup Id Name cannot be null")
                .NotEmpty().WithMessage("SubGroup Id Name is required");
            RuleFor(x => x)
                .MustAsync(IsUniqueServiceName)
                .When(x => x.ServiceId == 0)
                .WithMessage("Service Name already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdateServiceName)
                .When(x => x.ServiceId > 0)
                .WithMessage("Service Name already exists");
        }
        private async Task<bool> IsUniqueServiceName(ServicableMaster serviceMaster, CancellationToken cancellationToken)
        {
            return !await _context.ServicableMaster.AnyAsync(x => x.ServiceName == serviceMaster.ServiceName && x.SubGroupId == serviceMaster.SubGroupId && x.IsActive == true && x.CompanyId == serviceMaster.CompanyId && x.UserId == serviceMaster.UserId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateServiceName(ServicableMaster serviceMaster, CancellationToken cancellationToken)
        {
            return !await _context.ServicableMaster.AnyAsync(x => x.ServiceName == serviceMaster.ServiceName && x.ServiceId != serviceMaster.ServiceId && x.IsActive == true, cancellationToken);
        }
    }
}
