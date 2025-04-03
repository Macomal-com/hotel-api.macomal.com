using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class RoomRateResponse
    {
        public decimal BookingAmount { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal TotalRoomAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal AgentCommissionPercentage {get;set;}
        public decimal AgentCommisionAmount { get; set; }
        public decimal TcsPercentage { get; set; }
        public decimal TdsPercentage { get; set; }
        public decimal TcsAmount { get; set; }
        public decimal TdsAmount { get; set; }

        public int NoOfRooms { get; set; }
        public decimal AllRoomAmount { get; set; }
        public decimal AllRoomGst { get; set; }
        public string AgentGstType { get; set; } = string.Empty;

        public List<BookedRoomRate> BookedRoomRates = new List<BookedRoomRate>();

    }
}
