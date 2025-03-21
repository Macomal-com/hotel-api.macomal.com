using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class VendorMaster : ICommonProperties
    {
        [Key]
        public int VendorId { get; set; }
        public string VendorName { get; set; } = String.Empty;
        public string VendorEmail { get; set; } = String.Empty;
        public string VendorPhone { get; set; } = String.Empty;
        public int ServiceId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class VendorMasterDTO
    {
        public string VendorName { get; set; } = String.Empty;
        public string VendorEmail { get; set; } = String.Empty;
        public string VendorPhone { get; set; } = String.Empty;
        public int ServiceId { get; set; }
    }

    public class VendorValidator : AbstractValidator<VendorMasterDTO>
    {
        public VendorValidator()
        {
            RuleFor(x => x.VendorName)
                .NotNull().WithMessage("Vendor Name cannot be null")
                .NotEmpty().WithMessage("Vendor Name is required");
            RuleFor(x => x.VendorEmail)
                .NotNull().WithMessage("Email cannot be null")
                .NotEmpty().WithMessage("Email is required");
            RuleFor(x => x.VendorPhone)
                .NotNull().WithMessage("Phone Number cannot be null")
                .NotEmpty().WithMessage("Phone Number is required");
            RuleFor(x => x.ServiceId)
                .NotNull().WithMessage("Service Id cannot be null")
                .NotEmpty().WithMessage("Service Id is required");
        }
    }
}
