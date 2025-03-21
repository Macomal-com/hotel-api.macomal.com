using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class StaffManagementMaster : ICommonProperties
    {
        [Key]
        public int StaffId { get; set; }
        public string StaffName { get; set; } = String.Empty;
        public string StaffRole { get; set; } = String.Empty;
        public string PhoneNo { get; set; } = String.Empty;
        public double Salary { get; set; }
        public int PropertyId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class StaffManagementMasterDTO
    {
        public string StaffName { get; set; } = String.Empty;
        public string StaffRole { get; set; } = String.Empty;
        public string PhoneNo { get; set; } = String.Empty;
        public double Salary { get; set; }
        public string PropertyId { get; set; } = String.Empty;
    }
}
