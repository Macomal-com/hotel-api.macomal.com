using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class InvoiceHistory : ICommonProperties
    {
        [Key]
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; } 
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string ReservationNo { get; set; } = string.Empty;
        public int RoomId { get; set; }
        
        public decimal PaymentAmount { get; set; }
        
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal PaymentAmountUsed { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal PaymentLeft { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public decimal BalanceAmount { get; set; }

        public DateTime CreatedDate { get ; set; }
        public DateTime UpdatedDate { get ; set; }
        public bool IsActive { get ; set; }
        public int CompanyId { get ; set; }
        public int UserId { get ; set; }
    }
}
