using System.ComponentModel.DataAnnotations;

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
        public string ModifiedDate { get; set; } = String.Empty;
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
}
