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
        public List<int> BookingIds { get; set; } = new List<int>();
        public ReservationDetails ReservationDetails { get; set; } = new ReservationDetails();

        public DateTime EarlyCheckOutDate { get; set; }
    }
}
