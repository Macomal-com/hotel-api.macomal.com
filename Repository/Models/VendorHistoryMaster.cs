using FluentValidation;
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
    public class VendorHistoryMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int GivenById { get; set; }
        public int VendorId { get; set; }
        public int ServiceId { get; set; }
        public DateTime GivenDate  { get; set; }
        public string Remarks { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        [NotMapped]
        public string PhoneNo { get; set; } = String.Empty;
        [NotMapped]
        public string GivenBy { get; set; } = String.Empty;
    }
    public class VendorHistoryMasterDTO
    {
        public int GivenById { get; set; }
        public int VendorId { get; set; }
        public int ServiceId { get; set; }
        public DateTime GivenDate { get; set; }
        [NotMapped]
        public string PhoneNo { get; set; } = String.Empty;
        public string Remarks { get; set; } = String.Empty;
        [NotMapped]
        public string GivenBy { get; set; } = String.Empty;
    }

    public class VendorHistoryValidator : AbstractValidator<VendorHistoryMaster>
    {
        private readonly DbContextSql _context;
        public VendorHistoryValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.GivenBy)
                .NotNull().WithMessage("Staff Name is required")
                .NotEmpty().WithMessage("Staff Name is required");
            RuleFor(x => x.VendorId)
                .NotNull().WithMessage("Vendor is required")
                .NotEmpty().WithMessage("Vendor is required");
            RuleFor(x => x.ServiceId)
                .NotNull().WithMessage("Service is required")
                .NotEmpty().WithMessage("Service is required");
            RuleFor(x => x.GivenDate)
                .NotNull().WithMessage("Date is required")
                .NotEmpty().WithMessage("Date is required");
        }
    }
}
