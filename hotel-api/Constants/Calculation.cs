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


        
    
        public static DateTime ConvertToDateTime(DateTime date, string time)
        {
            if (time.Length == 5) time += ":00"; // Add seconds if missing
            return DateTime.ParseExact((date.ToString("yyyy-MM-dd")) + " " + time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static int CalculateNights(DateTime checkIn, DateTime checkOut)
        {
            TimeSpan timeDifference = checkOut - checkIn;
            return (int)timeDifference.TotalDays == 0 ? 1 : (int)timeDifference.TotalDays;
        }
    
        

        public static DateTime GetADayBefore(DateTime date)
        {
           
            DateTime previousDate = date.AddDays(-1);

            return previousDate;
        }

        public static (DateTime,string) GetAMinuteAfter(DateTime date, string time)
        {

            DateTime previousDate = ConvertToDateTime(date, time);
            previousDate = previousDate.AddMinutes(1);

            return (Convert.ToDateTime(previousDate.ToString("yyyy-MM-dd")), previousDate.ToString("HH:mm"));
        }

        public static (DateTime, string) GetDateTime(DateTime date)
        {
            DateTime datePart = date.Date;
            string timePart = date.TimeOfDay.ToString(); ;

            return (datePart, timePart);
        }
    }
}
