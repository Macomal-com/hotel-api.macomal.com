using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class WhatsAppCredentials
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string AuthKey { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
    }

}
