using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomCancelHistory : ICommonProperties
    {
        [Key]
        public int CancelId { get; set; }
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public string ReservationNo { get; set; } = string.Empty;
        public int PolicyId { get; set; }
        public string PolicyCode { get; set; } = string.Empty;
        public string PolicyDescription { get; set; } = string.Empty;
        public string DeductionBy { get; set; } = string.Empty;
        public string ChargesApplicableOn { get; set; } = string.Empty;
        public string CancellationTime { get; set; } = string.Empty;
        public int FromTime { get; set; }
        public int ToTime { get; set; }
        public int NoOfRooms { get; set; }
        public int CancelHours { get; set; }
        public DateTime CancelFromDate { get; set; }
        public DateTime CancelToDate { get; set; }
        public string CancelFormat { get; set; } = string.Empty;
        public DateOnly InvoiceDate { get; set; } = new DateOnly(1900, 01, 01);
        public string InvoiceNo { get; set; } = string.Empty;
        public decimal CancelAmount { get; set; }
        public decimal CancelPercentage { get; set; }
        public DateTime CreatedDate {get;set;}
        public DateTime UpdatedDate {get;set;}
        public bool IsActive {get;set;}
        public int CompanyId {get;set;}
        public int UserId {get;set;}
    }

}
