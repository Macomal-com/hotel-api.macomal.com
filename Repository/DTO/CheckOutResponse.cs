using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class CheckOutResponse
    {
        public ReservationDetails? ReservationDetails { get; set; }
        public List<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
      
        public PaymentSummary PaymentSummary { get; set; } = new PaymentSummary();
    }
}
