using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class HouseKeeping : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int RoomTypeId { get; set; }
        public int RoomId { get; set; }
        public string RoomStatus { get; set; } = string.Empty;
        public string ServiceStatus { get; set; } = string.Empty;
        public DateOnly ServiceDate { get; set; }
        public string ServiceTime { get; set; } = string.Empty;
        public DateTime ServiceDateTime { get; set; }
        public int ServiceBy { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        
    }

}
