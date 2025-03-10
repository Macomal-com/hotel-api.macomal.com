using System.ComponentModel.DataAnnotations;

namespace Repository.Models
{
    public class BedTypeMaster : ICommonParams
    {
        [Key]
        public int BedTypeId { get; set; }
        public string BedType { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class BedTypeMasterDTO
    {
        [Key]
        public string BedType { get; set; } = String.Empty;
    }
}
