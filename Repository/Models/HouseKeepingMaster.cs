using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    class HouseKeepingMaster
    {
        public class HouseKeeping
        {
            [Key]
            public int Id { get; set; }
            public int CatID { get; set; }
            public int FloorId { get; set; }
            public int RoomId { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime FromDate { get; set; } = DateTime.Now;
            public DateTime ToDate { get; set; } = DateTime.Now;
            public string Time { get; set; } = string.Empty;
            public int ServiceBy { get; set; }
            public string CheckedBy { get; set; } = string.Empty;
            public string CleanDirtyBy { get; set; } = string.Empty;
            public string LostFound { get; set; } = string.Empty;
            public string ElectricalWork { get; set; } = string.Empty;
            public string HeatLight { get; set; } = string.Empty;
            public string DoorLock { get; set; } = string.Empty;
            public string BreakingRoom { get; set; } = string.Empty;
            public string Remarks { get; set; } = string.Empty;
            public DateTime OperationDate { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; } = DateTime.MinValue;
            public int Other1 { get; set; }
            public string Other2 { get; set; } = string.Empty;
            public string Other3 { get; set; } = string.Empty;
            public int CompanyId { get; set; }
            public bool IsActive { get; set; }
            public string ServiceName { get; set; } = string.Empty;
            public string RoomNo { get; set; } = string.Empty;
            public string TowerId { get; set; } = string.Empty;

            [NotMapped]
            public string FromTime { get; set; } = string.Empty;

            [NotMapped]
            public string ToTime { get; set; } = string.Empty;
        }
        public class HouseKeepingDTO
        {
            public int Id { get; set; }
            public DateTime FromDate { get; set; } = DateTime.Now;
            public DateTime ToDate { get; set; } = DateTime.Now;
            public string FromTime { get; set; } = string.Empty;
            public string ToTime { get; set; } = string.Empty;
        }
    }
}
