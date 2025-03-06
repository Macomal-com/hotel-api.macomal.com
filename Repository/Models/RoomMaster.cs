using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomMaster
    {
        [Key]
        public int RoomId { get; set; }
        public int FloorId { get; set; }
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
        public int RoomNo { get; set; }
        public int RoomTypeId { get; set; }
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class RoomMasterDTO
    {
        [Key]
        public int FloorId { get; set; }
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
        public int RoomNo { get; set; }
        public int RoomTypeId { get; set; }
        public string Description { get; set; } = String.Empty;
    }
}
