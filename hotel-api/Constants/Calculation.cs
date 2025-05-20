using Repository.Models;
using System;
using System.Globalization;

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

        public static decimal CalculateCGST(decimal gstAmount)
        {
            return RoundOffDecimal(gstAmount / 2);
        }

        public static decimal CalculatePercentage(decimal amount, decimal percentage)
        {
            return RoundOffDecimal((amount * percentage) / 100);
        }

        
        public static decimal RoundOffDecimal(decimal value)
        {
            return Math.Round(value, 2);
        }




        //public static DateTime ConvertToDateTime(DateTime date, string time)
        //{
        //    if (time.Length == 5) time += ":00"; // Add seconds if missing
        //    return DateTime.ParseExact((date.ToString("yyyy-MM-dd")) + " " + time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        //}



        public static int CalculateNights(DateOnly checkIn, DateOnly checkOut)
        {
            TimeSpan duration = checkOut.ToDateTime(TimeOnly.MinValue) - checkIn.ToDateTime(TimeOnly.MinValue);
            return duration.Days == 0 ? 1 : duration.Days;
        }

        public static int FindNightsAndHours(DateOnly checkIn, string checkInTime, DateOnly checkOut, string checkOutTime, string checkOutFormat)
        {
            if (checkOutFormat == Constants.Hour24Format)
            {
                var checkInDateTime = DateTime.Parse($"{checkIn:yyyy-MM-dd}T{checkInTime}");
                var checkOutDateTime = DateTime.Parse($"{checkOut:yyyy-MM-dd}T{checkOutTime}");

                var timeDifference = checkOutDateTime - checkInDateTime;
                return (int)Math.Ceiling(timeDifference.TotalDays); // Always round up
            }
            else if (checkOutFormat == Constants.NightFormat)
            {
                var timeDifference = checkOut.ToDateTime(new TimeOnly(0, 0)) - checkIn.ToDateTime(new TimeOnly(0, 0));
                var days = (int)timeDifference.TotalDays;
                return days == 0 ? 1 : days;
            }
            

            return 0; // default fallback if format is unrecognized
        }

        



        public static int CalculateHour(DateOnly checkIn,string checkInTime,  DateOnly checkOut,string checkOutTime)
        {
            string format = "yyyy/MM/dd'T'HH:mm";
            var culture = CultureInfo.InvariantCulture;

            DateTime checkInDateTime = DateTime.ParseExact($"{checkIn:yyyy/MM/dd}T{checkInTime}", format, culture);
            DateTime checkOutDateTime = DateTime.ParseExact($"{checkOut:yyyy/MM/dd}T{checkOutTime}", format, culture);

            TimeSpan diff = checkOutDateTime - checkInDateTime;

            int hours = (int)diff.TotalHours;
            return hours;


        }





    }
}
