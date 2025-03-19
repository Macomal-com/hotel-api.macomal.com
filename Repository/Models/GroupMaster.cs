using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class GroupMaster : ICommonParams
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = String.Empty;
        public string GroupName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int Other1 { get; set; }
        public int Other2 { get; set; }
    }

    public class GroupMasterDTO
    {
        [Key]
        public string Code { get; set; } = String.Empty;
        public string GroupName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
    }

    public class GroupValidator : AbstractValidator<GroupMasterDTO>
    {
        public GroupValidator()
        {
            RuleFor(x => x.Code)
                .NotNull().WithMessage("Code cannot be null")
                .NotEmpty().WithMessage("Code is required");
            RuleFor(x => x.GroupName)
                .NotNull().WithMessage("Group Name cannot be null")
                .NotEmpty().WithMessage("Group Name is required");
        }
    }
}
