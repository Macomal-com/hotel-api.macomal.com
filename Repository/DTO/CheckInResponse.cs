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
        public List<BookingDetail>? BookingDetails { get; set; }
       
        public GuestDetails? GuestDetails { get; set; }
        public List<PaymentDetails>?  PaymentDetails { get; set; }

        public PaymentCheckInSummary PaymentSummary { get; set; } = new PaymentCheckInSummary();

        public bool IsSingleRoom { get; set; }
    }


    public class PaymentCheckInSummary
    {
        public decimal RoomAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal EarlyCheckIn { get; set; }
        public decimal LateCheckOut { get; set; }
        public decimal RoomServicesAmount { get; set; }
        public decimal TransactionCharges { get; set; }
        public decimal AgentServiceCharge { get; set; }

        public decimal TotalAmount { get; set; }

        
        public decimal AdvanceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal RefundAmount { get; set; }

        
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

        public decimal EarlyCheckIn { get; set; }

        public decimal LateCheckOut { get; set; }
    }


    public class CancelBookingResponse
    {
        public ReservationDetails? ReservationDetails { get; set; }
        public List<BookingDetail> bookingDetails { get; set; } = new List<BookingDetail>();
        public GuestDetails? GuestDetails { get; set; }
        public List<PaymentDetails> PaymentDetails { get; set; } = new List<PaymentDetails>();

        public CancelSummary CancelSummary { get; set; } = new CancelSummary();
        public string InvoiceNo { get; set; } = string.Empty;
        public DateOnly InvoiceDate { get; set; }
        public string InvoiceName { get; set; } = string.Empty;

        public bool IsAllCancel { get; set; } 
    }

    public class CancelSummary
    {
        public decimal AgentAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal TotalPaid { get; set; }

        public decimal TotalRooms { get; set; }

        public decimal CancelAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal RefundAmount { get; set; }

        public decimal ResidualAmount { get; set; }
    }
}
