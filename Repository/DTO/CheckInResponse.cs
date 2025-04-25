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
        public decimal TotalRoomAmount { get; set; } //Room Amount
        public decimal TotalGstAmount { get; set; } //Room Gst
        public decimal TotalAmount { get; set; } //Total Room Amount

        public decimal AgentServiceCharge { get; set; } //Agent Service
        public decimal AgentServiceGst { get; set; } //Agent GST
        public decimal AgentServiceTotal { get; set; } //Agent Total Service  //agent service charge + agent service gst
        public decimal TotalPayable { get; set; } //Total Payable //total amount + agent service charge
        public decimal AgentPaid { get; set; } //Agent Adjusted // paid to agent amount
        
        public decimal AdvanceAmount { get; set; } //Advance
        public decimal ReceivedAmount { get; set; }//Amount Adjusted
        public decimal BalanceAmount { get; set; }//Balance
        public decimal RefundAmount { get; set; }//Refund
       
        

        public decimal TotalAllAmount { get; set; } //Total Room Amount

        //advance services
        public decimal RoomServiceAmount { get; set; }
        public decimal RoomServiceTaxAmount { get; set; }
        public decimal TotalRoomServicesAmount { get; set; }

        //total tax = advance services tax + room service tax
        public decimal TotalTaxAmount { get; set; }
    }
}
