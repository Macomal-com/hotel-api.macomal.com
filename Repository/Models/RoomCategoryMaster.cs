using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomCategoryMaster : ICommonParams
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int MinPax { get; set; }
        public int MaxPax { get; set; }
        public int BedTypeId { get; set; }
        public int NoOfRooms { get; set; }
        public string PlanDetails { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }

    }
    public class RoomCategoryMasterDTO
    {
        [Key]
        public string Type { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int MinPax { get; set; }
        public int MaxPax { get; set; }
        public int BedTypeId { get; set; }
        public int NoOfRooms { get; set; }
        public string PlanDetails { get; set; } = String.Empty;

    }
}
