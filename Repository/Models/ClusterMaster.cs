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

    public class ClusterValidator : AbstractValidator<ClusterMaster>
    {
        public ClusterValidator()
        {
            RuleFor(x => x.ClusterName).NotEmpty().WithMessage("Cluster Name is required");
            RuleFor(x => x.ClusterDescription).NotEmpty().WithMessage("Cluster Description is required");
            RuleFor(x => x.ClusterLocation).NotEmpty().WithMessage("Cluster Location is required");
        }
    }
}
