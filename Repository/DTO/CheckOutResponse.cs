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
      
        public PaymentCheckOutSummary PaymentSummary { get; set; } = new PaymentCheckOutSummary();
        public GuestDetails? GuestDetails { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;

        public List<PaymentDetails> PaymentDetails = new List<PaymentDetails>();

        public DateOnly InvoiceDate { get; set; } = DateOnly.FromDateTime(DateTime.Now)
;
        public string InvoiceName { get; set; } = string.Empty;

        public string CheckOutDiscountType { get; set; } = string.Empty;

        public decimal CheckOutDiscount { get; set; }
        public string CheckOutFormat { get; set; } = string.Empty;

        public CompanyDetails PropertyDetails = new CompanyDetails();

        public string PageName = "CheckOutPage";

    }

    public class InvoiceData
    {
        public ReservationDetails? ReservationDetails { get; set; }
        public List<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
        public GuestDetails? GuestDetails { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateOnly InvoiceDate { get; set; } = DateOnly.FromDateTime(DateTime.Now)
;
        public string InvoiceName { get; set; } = string.Empty;
        public PaymentCheckOutSummary PaymentSummary { get; set; } = new PaymentCheckOutSummary();
        public CompanyDetails PropertyDetails = new CompanyDetails();

        public string PageName = "CheckOutPage";
        public DateTime CancelDate { get; set; } = DateTime.Now;
        public CancelSummary CancelSummary { get; set; } = new CancelSummary();
    }
}
