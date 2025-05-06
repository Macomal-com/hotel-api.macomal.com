using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomAvailability : ICommonProperties
    {
        public int Id { get; set; }
        public DateTime CheckInDate { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public DateTime CheckOutDate { get; set; }
        public string CheckOutTime { get; set; } = string.Empty;
        public DateTime CheckInDateTime { get; set; }
        public DateTime CheckOutDateTime { get; set; }
        public string ReservationNo { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public string RoomStatus { get; set; } = string.Empty;
        public int RoomTypeId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }

        public string ServiceStatus { get; set; } = string.Empty;
    }

    public class RoomAvailabilityDTO
    {
        public DateTime CheckInDate { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public DateTime CheckOutDate { get; set; }
        public string CheckOutTime { get; set; } = string.Empty;
        public DateTime CheckInDateTime { get; set; }
        public DateTime CheckOutDateTime { get; set; }
        public string ReservationNo { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public string RoomStatus { get; set; } = string.Empty;
        public int RoomTypeId { get; set; }
    }
}
