using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ReservationDetails : ICommonProperties
    {
        [Key]
        public int ReservationId { get; set; }
        public string ReservationNo { get; set; } = string.Empty;

        public int AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;

        public string AgentGstType { get; set; } = string.Empty;
        public decimal CommissionPercentage { get; set; }

        public decimal CommissionAmount { get; set; }

        public decimal Tcs { get; set; }
        public decimal TcsAmount { get; set; }
        public decimal Tds { get; set; }

        public decimal TdsAmount { get; set; }

        public decimal AgentServiceCharge { get; set; }

        public string AgentServiceGstType { get; set; } = string.Empty;

        public decimal AgentServiceGstPercentage { get; set; }

        public decimal AgentServiceGstAmount { get; set; }
        public decimal AgentTotalServiceCharge { get; set; }

        public string AgentReferenceId { get; set; } = string.Empty;
        public string BookingSource { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public int UserId { get; set; }
        public int CompanyId { get; set; }

        public decimal TotalRoomPayment { get; set; }
        public decimal TotalGst { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ReservationDetailsDTO
    {
        public string ReservationNo { get; set; } = string.Empty;
        public int AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;

        public string AgentGstType { get; set; } = string.Empty;
        public decimal CommissionPercentage { get; set; }

        public decimal CommissionAmount { get; set; }

        public decimal Tcs { get; set; }
        public decimal TcsAmount { get; set; }
        public decimal Tds { get; set; }

        public decimal TdsAmount { get; set; }

        public decimal AgentServiceCharge { get; set; }

        public string AgentServiceGstType { get; set; } = string.Empty;


        public string AgentReferenceId { get; set; } = string.Empty;
        public string BookingSource { get; set; } = string.Empty;

        public bool IsCheckIn { get; set; }
    }

  

}
