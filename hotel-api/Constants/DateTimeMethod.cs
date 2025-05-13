using System.Globalization;

namespace hotel_api.Constants
{
    public class DateTimeMethod
    {
        public static string ConvertDateTimeToString(DateTime date)
        {
            return date.ToString("dd-MM-yyyy hh:mm tt");
        }

        public static DateTime ConvertToDateTime(DateTime date, string time)
        {
            if (time.Length == 5) time += ":00"; // Add seconds if missing
            return DateTime.ParseExact((date.ToString("yyyy-MM-dd")) + " " + time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static (DateTime, string) GetAMinuteAfter(DateTime date, string time)
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


        public static (DateOnly, string) GetDateOnlyAndTime(DateTime date)
        {
            DateOnly datePart = DateOnly.FromDateTime(date.Date);
            string timePart = date.TimeOfDay.ToString(@"hh\:mm"); 

            return (datePart, timePart);
        }

        public static DateTime GetADayBefore(DateTime date)
        {

            DateTime previousDate = date.AddDays(-1);

            return previousDate;
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
    }
}
