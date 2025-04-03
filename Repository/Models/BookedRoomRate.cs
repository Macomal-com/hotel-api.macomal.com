using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class BookedRoomRate : ICommonProperties
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public int RoomId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal RoomRate { get; set; }

        public DateTime BookingDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
    }

}
