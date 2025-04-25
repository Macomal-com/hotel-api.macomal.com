using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ServicesStatus
    {
        [Key]
        public int Id { get; set; }
        public string RoomStatusValue { get; set; } = String.Empty;
        public string RoomStatus { get; set; } = String.Empty;
        public string Colour { get; set; } = String.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public int CompanyId { get; set; }
        public int CreatedBy { get; set; }

    }
}
