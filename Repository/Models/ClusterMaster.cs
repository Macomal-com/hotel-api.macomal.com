using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;

namespace Repository.Models
{
    public class ClusterMaster : ICommonProperties
    {
        [Key]
        public int ClusterId { get; set; }
        public string ClusterName { get; set; } = String.Empty;
        public string ClusterDescription { get; set; } = String.Empty;
        public string ClusterLocation { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
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
        private readonly DbContextSql _context;
        public ClusterValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.ClusterName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Cluster Name is required")
                .NotEmpty().WithMessage("Cluster Name is required");
        

            RuleFor(x => x.ClusterLocation)
                .Cascade(CascadeMode.Stop)  
                .NotNull().WithMessage("Cluster Location is required")
                .NotEmpty().WithMessage("Cluster Location is required");

            RuleFor(x => x.NoOfProperties)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("No of properties is required")
                .GreaterThan(0).WithMessage("No of properties is required");

            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueClusterName)
                .When(x => x.ClusterId == 0)
                .WithMessage("Cluster Name already exists");

            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueUpdateClusterName)
                .When(x => x.ClusterId > 0)
                .WithMessage("Cluster Name already exists");

        }

        private async Task<bool> IsUniqueClusterName(ClusterMaster clusterMaster, CancellationToken cancellationToken)
        {
            
                return !await _context.ClusterMaster.AnyAsync(x => x.ClusterName == clusterMaster.ClusterName && x.IsActive == true, cancellationToken);
            

        }


        private async Task<bool> IsUniqueUpdateClusterName(ClusterMaster clusterMaster, CancellationToken cancellationToken)
        {

            return !await _context.ClusterMaster.AnyAsync(x => x.ClusterName == clusterMaster.ClusterName && x.ClusterId != clusterMaster.ClusterId && x.IsActive == true, cancellationToken);


        }
    }

}
