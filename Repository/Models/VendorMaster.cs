using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class VendorMaster : ICommonParams
    {
        [Key]
        public int VendorId { get; set; }
        public string VendorName { get; set; } = String.Empty;
        public string VendorEmail { get; set; } = String.Empty;
        public string VendorPhone { get; set; } = String.Empty;
        public int ServiceId { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
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
}
