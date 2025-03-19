using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Repository.Models
{
    public class ClusterMaster : ICommonParams
    {
        [Key]
        public int ClusterId { get; set; }
        public string ClusterName { get; set; } = String.Empty;
        public string ClusterDescription { get; set; } = String.Empty;
        public string ClusterLocation { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public int NoOfProperties { get; set; }
    }

    public class ClusterDTO
    {
        public string ClusterName { get; set; } = String.Empty;
        public string ClusterDescription { get; set; } = String.Empty;
        public string ClusterLocation { get; set; } = String.Empty;
        public int NoOfProperties { get; set; }

    }

    public class ClusterValidator : AbstractValidator<ClusterDTO>
    {
        public ClusterValidator()
        {
            RuleFor(x => x.ClusterName)
                .NotNull().WithMessage("Cluster Name cannot be null")
                .NotEmpty().WithMessage("Cluster Name is required");

            RuleFor(x => x.ClusterDescription)
                .NotNull().WithMessage("Cluster Description cannot be null")
                .NotEmpty().WithMessage("Cluster Description is required");

            RuleFor(x => x.ClusterLocation)
                .NotNull().WithMessage("Cluster Location cannot be null")
                .NotEmpty().WithMessage("Cluster Location is required");
        }
    }

}
