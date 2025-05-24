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

        public DateOnly InvoiceDate { get; set; } = DateOnly.FromDateTime(DateTime.Now)
;
        public string InvoiceNo { get; set; } = string.Empty;

        public string CheckOutDiscountType { get; set; } = string.Empty;

        public decimal CheckOutDiscount { get; set; }

       
    }


    public class CalculateCancelAmountRequest
    {
        
        public List<int> BookingIds { get; set; } = new List<int>();
        public string ReservationNo { get; set; } = string.Empty;

        public DateTime CancelDate { get; set; }

        public string InvoiceNo { get; set; } = string.Empty;
    }
}
