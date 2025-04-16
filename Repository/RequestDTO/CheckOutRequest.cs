using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.RequestDTO
{
    public class CheckOutRequest
    {
        public List<BookingDetail> bookingDetails { get; set; } = new List<BookingDetail>();
        public string ReservationNo { get; set; } = string.Empty;
        
    }
}
