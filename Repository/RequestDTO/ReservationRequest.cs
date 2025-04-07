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

        public ReservationDetailsDTO? ReservationDetailsDTO { get; set; }
        public List<BookingDetailDTO>? BookingDetailsDTO { get; set; }
        public PaymentDetailsDTO? PaymentDetailsDTO { get; set; }

        public GuestDetailsDTO? GuestDetailsDTO { get; set; }

        public PaymentDetailsDTO? AgentPaymentDetailsDTO { get; set; }
    }

    public class RoomData
    {
        public int RoomId { get; set; }
        public string? RoomNo { get; set; }

        public List<BookedRoomRate> roomRates { get; set; } = new List<BookedRoomRate>();
    }
}
