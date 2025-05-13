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
}
