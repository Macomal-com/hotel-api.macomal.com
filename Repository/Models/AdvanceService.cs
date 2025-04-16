using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class AdvanceService
    {
        [Key]
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = String.Empty;
        public decimal ServicePrice { get; set; }
        public int Quantity { get; set; }
        public string GstType { get; set; } = String.Empty;
        public decimal TotalAmount { get; set; }
        public decimal GST { get; set; }
        public decimal GstAmount { get; set; }
        public decimal IGST { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal CGST { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SGST { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal TotalServicePrice { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceTime { get; set; } = String.Empty;
        public string KotNo { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsActive { get; set; }
    }
}
