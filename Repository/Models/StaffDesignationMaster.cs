using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class StaffDesignationMaster
    {
        [Key]
        public int Int { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
    }
}
