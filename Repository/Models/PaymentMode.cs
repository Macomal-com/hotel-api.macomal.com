using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class PaymentMode : ICommonProperties
    {
        [Key]
        public int PaymentId { get; set; }
        public string PaymentModeName { get; set; } = String.Empty;
        public string ProviderContact { get; set; } = String.Empty;
        public string ProviderEmail { get; set; } = String.Empty;
        public double TransactionCharges { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class PaymentModeDTO
    {
        public string PaymentModeName { get; set; } = String.Empty;
        public string ProviderContact { get; set; } = String.Empty;
        public string ProviderEmail { get; set; } = String.Empty;
        public double TransactionCharges { get; set; }
    }
}

