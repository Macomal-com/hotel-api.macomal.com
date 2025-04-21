using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class AddPaymentResponse
    {
        public ReservationDetails? ReservationDetails { get; set; }
        public GuestDetails? GuestDetails { get; set; }
        public List<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
        public List<PaymentDetails>? PaymentDetails { get; set; }
        public PaymentSummary PaymentSummary { get; set; } = new PaymentSummary();
    }
}
