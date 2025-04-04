using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.RequestDTO
{
    public class ReservationRequest
    {
        public List<BookingDetail>? BookingDetails { get; set; }
        public PaymentDetails? PaymentDetails { get; set; }

        public GuestDetails? GuestDetails { get; set; }
    }

    public class RoomData
    {
        public int RoomId { get; set; }
        public string? RoomNo { get; set; }
    }
}
