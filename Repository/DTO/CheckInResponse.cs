using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class CheckInResponse
    {
        public ReservationDetails? ReservationDetails { get; set; }
        public List<BookingDetailCheckInDTO>? BookingDetailCheckInDTO { get; set; }
       
        public GuestDetails? GuestDetails { get; set; }
        public List<PaymentDetails>?  PaymentDetails { get; set; }
    }
}
