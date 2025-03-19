using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class SubGroupMaster : ICommonParams
    {
        [Key]
        public int SubGroupId { get; set; }
        public string SubGroupName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int GroupId { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class SubGroupMasterDTO
    {
        [Key]
        public string SubGroupName { get; set; } = String.Empty;
        public int GroupId { get; set; }
        public string Description { get; set; } = String.Empty;

    }

    public class SubGroupValidator : AbstractValidator<SubGroupMasterDTO>
    {
        public SubGroupValidator()
        {
            RuleFor(x => x.SubGroupName)
                .NotNull().WithMessage("SubGroup Name cannot be null")
                .NotEmpty().WithMessage("SubGroup Name is required");
            RuleFor(x => x.GroupId)
                .NotNull().WithMessage("Group Name cannot be null")
                .NotEmpty().WithMessage("Group Name is required");
        }
    }
}
