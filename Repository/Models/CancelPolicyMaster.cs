using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class CancelPolicyMaster
    {  
        public int Id { get; set; }
        public string PolicyCode { get; set; } = String.Empty;
        public string DeductionBy { get; set; } = String.Empty;
        public string ChargesApplicableOn { get; set; } = String.Empty;
        public decimal DeductionAmount { get; set; }
        public string CancellationTime { get; set; } = String.Empty;
        public int FromTime { get; set; }
        public int ToTime { get; set; }
        public int MinRoom { get; set; }
        public int MaxRoom { get; set; }
        public string PolicyDescription { get; set; } = String.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
}
