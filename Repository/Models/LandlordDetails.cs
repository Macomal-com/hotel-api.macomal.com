using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Repository.Models
{
    public class LandlordDetails : ICommonParams
    {
        [Key]
        public int LandlordId { get; set; }
        public string LandlordName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public double CommissionPercentage { get; set; }
        public string CreatedDate { get; set; } = string.Empty;
        public string UpdatedDate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }

    }
    public class LandlordDetailsDTO
    {
        [Key]
        public string LandlordName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public double CommissionPercentage { get; set; }
    }
}
