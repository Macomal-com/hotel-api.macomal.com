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

        public bool IsLateCheckOut { get; set; }
        public string LateCheckOutPolicyName { get; set; } = string.Empty;

        public string LateCheckOutDeductionBy { get; set; } = string.Empty;
        public string LateCheckOutApplicableOn { get; set; }
        = string.Empty;

        public int LateCheckOutFromHour { get; set; }
        public int LateCheckOutToHour { get; set; }

        public decimal LateCheckOutCharges { get; set; }

        public string DiscountType { get; set; } = string.Empty;

        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal BookingAmountWithoutDiscount { get; set; }
    }

    
}
