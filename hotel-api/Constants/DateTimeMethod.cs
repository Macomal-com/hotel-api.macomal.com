using System.Globalization;

namespace hotel_api.Constants
{
    public class DateTimeMethod
    {
        public static string ConvertDateTimeToString(DateTime date)
        {
            return date.ToString("dd-MM-yyyy hh:mm tt");
        }

        public static DateTime ConvertToDateTime(DateOnly date, string time)
        {
            if (time.Length == 5) time += ":00"; // Add seconds if missing
            return DateTime.ParseExact((date.ToString("yyyy-MM-dd")) + " " + time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static (DateOnly, string) GetAMinuteAfter(DateOnly date, string time)
        {
            DateTime dateTime = ConvertToDateTime(date, time);
            dateTime = dateTime.AddMinutes(1);

            DateOnly newDate = DateOnly.FromDateTime(dateTime);
            string newTime = dateTime.ToString("HH:mm");

            return (newDate, newTime);
        }

        public static (DateOnly, string) GetAMinuteBefore(DateOnly date, string time)
        {
            DateTime dateTime = ConvertToDateTime(date, time);
            dateTime = dateTime.AddMinutes(-1);

            DateOnly newDate = DateOnly.FromDateTime(dateTime);
            string newTime = dateTime.ToString("HH:mm");

            return (newDate, newTime);
        }

        public static (DateOnly, string) GetDateTime(DateTime date)
        {
            DateOnly datePart = DateOnly.FromDateTime(date);
            string timePart = date.TimeOfDay.ToString(@"hh\:mm\:ss");

            return (datePart, timePart);
        }


        public static (DateOnly, string) GetDateOnlyAndTime(DateTime date)
        {
            DateOnly datePart = DateOnly.FromDateTime(date.Date);
            string timePart = date.TimeOfDay.ToString(@"hh\:mm"); 

            return (datePart, timePart);
        }

        public static DateOnly GetADayBefore(DateOnly date)
        {
            return date.AddDays(-1);
        }


        public static int FindEarlyCheckInHourDifference(string defaultTime, string checkInTime)
        {
            TimeSpan checkinTime = TimeSpan.Parse(checkInTime);
            TimeSpan defaulttime = TimeSpan.Parse(defaultTime);

            if (checkinTime < defaulttime)
            {
                TimeSpan difference = defaulttime - checkinTime;
                int hoursDifference = (int)difference.TotalHours;
                return hoursDifference;
                
            }
            else
            {
                return -1;
            }
        }


        public static int FindLateCheckOutHourDifference(string defaultTime, string checkOutTime)
        {
            TimeSpan checkOuttime = TimeSpan.Parse(checkOutTime);
            TimeSpan defaulttime = TimeSpan.Parse(defaultTime);

            if (checkOuttime > defaulttime)
            {
                TimeSpan difference = checkOuttime - defaulttime;
                int hoursDifference = (int)difference.TotalHours;
                return hoursDifference;

            }
            else
            {
                return -1;
            }
        }

        public static int GetDifferenceInMinutes(DateOnly startDate, string startTime, DateOnly endDate, string endTime)
        {
            var start = DateTime.Parse($"{startDate:yyyy-MM-dd}T{startTime}");
            var end = DateTime.Parse($"{endDate:yyyy-MM-dd}T{endTime}");


            return (int)Math.Round((end - start).TotalMinutes);

          

        }

        public static (DateOnly CheckoutDate, string CheckoutTime) CalculateCheckoutDateTimeOnHour(DateTime checkInDateTime, int hoursToAdd)
        {
            
            DateTime checkOut = checkInDateTime.AddHours(hoursToAdd);

            DateOnly checkoutDate = DateOnly.FromDateTime(checkOut); // Correctly convert DateTime to DateOnly
            string checkoutTime = checkOut.ToString("HH:mm");         // Format time as string

            return (checkoutDate, checkoutTime);
        }


    }
}
