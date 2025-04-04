using Repository.RequestDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class BookingDetail : ICommonProperties
    {
        [Key]
        public int BookingId { get; set; }
        public int GuestId { get; set; }
        public int RoomId { get; set; }
        public int RoomTypeId { get; set; }
        [NotMapped]
        public string RoomCategoryName { get; set; } = string.Empty;

        public DateTime CheckInDate { get; set; } 
        public string CheckInTime { get; set; } = string.Empty;
        public DateTime CheckOutDate { get; set; }
        public string CheckOutTime { get; set; } = string.Empty;
        public DateTime CheckInDateTime { get; set; }
        public DateTime CheckOutDateTime { get; set; }
        public string CheckoutFormat { get; set; } = string.Empty;
        public int NoOfNights { get; set; }
        public int NoOfHours { get; set; }
        public int HourId { get; set; }
        public int AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public int CommissionPercentage { get; set; }
        public decimal CommissionAmount { get; set; }
        public string GstType { get; set; } = string.Empty;
        public int AgentGstPercentage { get; set; }
        public decimal AgentGstAmount { get; set; }
        public int Tcs { get; set; }
        public decimal TcsAmount { get; set; }
        public int Tds { get; set; }
        public decimal TdsAmount { get; set; }
        public DateTime AgentPaidDate { get; set; }
        public int RoomCount { get; set; }
        public int Pax { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public decimal TotalRoomAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal TotalAmount { get; set; }
        public string ReservationNo { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public string BookingSource { get; set; } = string.Empty;
        public int PrimaryGuestId { get; set; }
        public decimal InitialBalanceAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public string AdvanceReceiptNo { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public int UserId { get; set; }

        public int CompanyId { get; set; }

        [NotMapped]
        public List<RoomData> RoomsList = new List<RoomData>();

        [NotMapped]
        public int NoOfRooms { get; set; }

        
    }



}
