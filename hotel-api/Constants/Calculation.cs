﻿using Repository.Models;

namespace hotel_api.Constants
{
    public class Calculation
    {
        public static (decimal NetAmount, decimal GstAmount) CalculateGst(decimal amount, decimal gst, string gstType)
        {
            if(gstType == Constants.Inclusive)
            {
                decimal gstAmount = (amount * gst) / (100 + gst);
                decimal netAmount = amount - gstAmount;
                return (RoundOffDecimal(netAmount), RoundOffDecimal(gstAmount));
            }
            else 
            {
                return (RoundOffDecimal(amount), RoundOffDecimal((amount * gst) / 100));
            }
                
        }

        public static decimal CalculatePercentage(decimal amount, decimal percentage)
        {
            return RoundOffDecimal((amount * percentage) / 100);
        }

        public static decimal RoundOffDecimal(decimal value)
        {
            return Math.Round(value, 2);
        }


        public static (decimal RoomAmout, decimal TotalGst, decimal TotalAmount) CalculateTotalRoomAmount(List<BookingDetailDTO> bookings)
        {
            decimal roomAmount = 0;
            decimal gst = 0;
            decimal totalAmount = 0;
            foreach(var item in bookings)
            {
                int rooms = item.NoOfRooms;
                while(rooms > 0)
                {
                    roomAmount = roomAmount + item.BookingAmount;
                    gst = gst + item.GstAmount;
                    totalAmount = totalAmount + item.TotalBookingAmount;
                    rooms--;
                }
            }

            roomAmount = RoundOffDecimal(roomAmount);
            gst = RoundOffDecimal(gst);
            totalAmount = RoundOffDecimal(totalAmount);

            return (roomAmount, gst, totalAmount);
        }
    }
}
