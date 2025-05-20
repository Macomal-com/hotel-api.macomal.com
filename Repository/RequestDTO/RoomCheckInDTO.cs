using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.RequestDTO
{
    public class RoomCheckInDTO
    {
        public List<int> rooms { get; set; } = new List<int>();
        public GuestDetails GuestDetails { get; set; } = new GuestDetails();
        public bool IsSingleRoom { get; set; }
        public bool IsCheckIn { get; set; }

        public string ReservationNo { get; set; } = string.Empty;
    }

    public class RoomEditDTO
    {
       

        public DateOnly ReservationDate { get; set; }
        public string ReservationTime { get; set; } = string.Empty;

        public DateOnly CheckInDate { get; set; }
        public string CheckInTime { get; set; } = string.Empty;

        public DateOnly CheckOutDate { get; set; }

        public string CheckOutTime { get; set; } = string.Empty;

        public int RoomTypeId { get; set; }

        public int RoomId { get; set; }
        public int BookingId { get; set; }

        public string GstType { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal Discount { get; set; } 
        public int NoOfHours { get; set; }
        public int NoOfNights { get; set; }

        public string ValueChanged { get; set; } = string.Empty;

    }
}
