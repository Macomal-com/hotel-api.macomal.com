using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class PaymentDetails :  ICommonProperties
    {

        [Key]
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string ReservationNo { get; set; } = string.Empty;
        public DateOnly PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string PaymentReferenceNo { get; set; } = string.Empty;
        public string PaidBy { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public double Other1 { get; set; }
        public string Other2 { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsReceived { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public string PaymentFormat { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public decimal PaymentLeft { get; set; }
        public int CompanyId { get; set; }

        public decimal TransactionCharges { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal TransactionAmount { get; set; }

        [NotMapped]
        public bool IsEditable { get; set; }

        [NotMapped]
        public decimal TotalAmount { get; set; }


        [NotMapped]
        public string RoomNo { get; set; } = string.Empty;

        [NotMapped]
        public List<RoomsList> RoomsList { get; set; } = new List<RoomsList>();

        [NotMapped]
        public List<InvoiceHistory> InvoiceHistories = new List<InvoiceHistory>();
    }

    public class RoomsList
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
    }
    public class PaymentDetailsDTO
    {

        public DateOnly PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string PaymentReferenceNo { get; set; } = string.Empty;
        public decimal PaymentAmount { get; set; }
        public string PaymentFormat { get; set; } = string.Empty;
        public decimal TransactionCharges { get; set; }
        public string TransactionType { get; set; } = string.Empty;
    }

}
