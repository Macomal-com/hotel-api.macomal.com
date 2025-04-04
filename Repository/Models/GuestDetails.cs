using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class GuestDetails : ICommonProperties
    {
        [Key]
        public int GuestID { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GuestImage { get; set; } = string.Empty;
        public int Other1 { get; set; }
        public string Other2 { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

}
