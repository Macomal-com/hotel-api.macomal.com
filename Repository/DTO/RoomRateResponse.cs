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

        public decimal BookingAmount { get; set; } //room rate
        public decimal GstPercentage { get; set; } //gst %
        public decimal TotalAmount { get; set; } // all gst + all rooms amount
        public decimal GstAmount { get; set; } //gst amount
        
        public int NoOfRooms { get; set; } 
        public decimal AllRoomAmount { get; set; } //no of rooms * total room amounty
        public decimal AllRoomGst { get; set; } //no of rooms * gstamount
        

        public List<BookedRoomRate> BookedRoomRates = new List<BookedRoomRate>();

    }
}
