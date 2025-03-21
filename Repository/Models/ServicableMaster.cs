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
    }

    public class ServiveValidator : AbstractValidator<ServicableMasterDTO>
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
        private async Task<bool> IsUniqueServiceName(ServicableMasterDTO ServiceMaster, CancellationToken cancellationToken)
        {

            return !await _context.ServicableMaster.AnyAsync(x => x.ServiceName == ServiceMaster.ServiceName && x.IsActive == true, cancellationToken);


        }


        private async Task<bool> IsUniqueUpdateServiceName(ServicableMasterDTO ServiceMaster, CancellationToken cancellationToken)
        {

            return !await _context.ServicableMaster.AnyAsync(x => x.ServiceName == ServiceMaster.ServiceName && x.ServiceId != ServiceMaster.ServiceId && x.IsActive == true, cancellationToken);


        }
    }
}
