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

        public PaymentSummary PaymentSummary { get; set; } = new PaymentSummary();
    }

    public class PaymentSummary
    {
        public decimal TotalRoomAmount { get; set; }
        public decimal TotalGstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AgentPaid { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal RefundAmount { get; set; }
    }
}
