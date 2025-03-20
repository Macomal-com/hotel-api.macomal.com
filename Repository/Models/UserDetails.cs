using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserDetails
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SecurityQuestion { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int? CompanyId { get; set; } 
        public string CompanyName { get; set; } = string.Empty;
        public string DBName { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public int? BranchId { get; set; } 
        public int? CreatedBy { get; set; }
        public int AgentId { get; set; } = 0;
        public string Status { get; set; } = string.Empty;
        public int? City { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public int AccessId { get; set; } = 0;
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? Other1 { get; set; }
        public int? Other2 { get; set; }
        public string Other3 { get; set; } = string.Empty;
        public string Other4 { get; set; } = string.Empty;
        public string Other5 { get; set; } = string.Empty;

        
    }
}
