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

        public decimal BookingAmount { get; set; } //all days room rate
        public decimal TotalBookingAmount { get; set; } // all gst + all rooms amount
        public decimal GstAmount { get; set; } //all days gst amount
        public int NoOfNights { get; set; }
        public int NoOfRooms { get; set; } 
        public decimal AllRoomsAmount { get; set; } //no of rooms * total room amounty
        public decimal AllRoomsGst { get; set; } //no of rooms * gstamount

        public decimal TotalRoomsAmount { get; set; } // (no of rooms * total room amount) + (no of rooms * gst amount)

        public List<BookedRoomRate> BookedRoomRates = new List<BookedRoomRate>();

        public bool IsEarlyCheckIn { get; set; }
        public string EarlyCheckInPolicyName { get; set; } = string.Empty;

        public string EarlyCheckInDeductionBy { get; set; } = string.Empty;
        public string EarlyCheckInApplicableOn { get; set; }
        = string.Empty;

        public int EarlyCheckInFromHour { get; set; } 
        public int EarlyCheckInToHour { get; set; }

        public decimal EarlyCheckInCharges { get; set; }
    }

    public class ExtraPolicyCharges
    {
        public bool IsEarlyCheckIn { get; set; }
        public string EarlyCheckInPolicyName { get; set; } = string.Empty;

        public string EarlyCheckInDeductionBy { get; set; } = string.Empty;
        public string EarlyCheckInApplicableOn { get; set; }
        = string.Empty;

        public string EarlyCheckInFromHour { get; set; } = string.Empty;
        public string EarlyCheckInToHour { get; set; } = string.Empty;

        public decimal EarlyCheckInCharges { get; set; }
    }
}
