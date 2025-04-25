using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.RequestDTO
{
    public class CalculateRoomRateRequest
    {
        public Dictionary<int, DateTime> Bookings = new Dictionary<int, DateTime>();
        //public List<int> BookingIds { get; set; } = new List<int>();
        public ReservationDetails ReservationDetails { get; set; } = new ReservationDetails();

        //public DateTime EarlyCheckOutDate { get; set; }
    }
}
