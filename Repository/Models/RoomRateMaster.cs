using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomRateMaster : ICommonParams
    {
        [Key]
        public int RoomRateId { get; set; }
        public int RoomTypeId { get; set; }
        public double Rate { get; set; }
        public string StartDate { get; set; } = String.Empty;
        public string ToDate { get; set; } = String.Empty;
        public string Gst { get; set; } = String.Empty;
        public string DiscountPrice { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class RoomRateMasterDTO
    {
        public int RoomTypeId { get; set; }
        public double Rate { get; set; }
        public string StartDate { get; set; } = String.Empty;
        public string ToDate { get; set; } = String.Empty;
        public string Gst { get; set; } = String.Empty;
        public string DiscountPrice { get; set; } = String.Empty;
    }
}
