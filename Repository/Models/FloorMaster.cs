using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class FloorMaster
    {
        [Key]
        public int FloorId { get; set; }
        public int FloorNumber { get; set; }
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public string CreatedBy { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class FloorMasterDTO
    {
        [Key]
        public int FloorNumber { get; set; }
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
    }
}
