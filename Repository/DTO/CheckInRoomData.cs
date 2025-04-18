using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class CheckInRoomData
    {
        public BookingDetail? BookingDetail { get; set; } 
        public PaymentSummary PaymentSummary { get; set; } = new PaymentSummary();
    }
}
