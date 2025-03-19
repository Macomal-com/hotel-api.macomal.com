using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ServicableMaster : ICommonParams
    {
        [Key]
        public int ServiceId { get; set; }
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public string ServiceName { get; set; } = String.Empty;
        public string ServiceDescription { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class ServicableMasterDTO
    {
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public string ServiceName { get; set; } = String.Empty;
        public string ServiceDescription { get; set; } = String.Empty;
    }

    public class ServiveValidator : AbstractValidator<ServicableMasterDTO>
    {
        public ServiveValidator()
        {
            RuleFor(x => x.ServiceName)
                .NotNull().WithMessage("Service Name cannot be null")
                .NotEmpty().WithMessage("Service Name is required");
            RuleFor(x => x.GroupId)
                .NotNull().WithMessage("Group Id cannot be null")
                .NotEmpty().WithMessage("Group Id Name is required");
            RuleFor(x => x.SubGroupId)
                .NotNull().WithMessage("SubGroup Id Name cannot be null")
                .NotEmpty().WithMessage("SubGroup Id Name is required");
        }
    }
}
