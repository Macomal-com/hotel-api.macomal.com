namespace hotel_api.Constants
{
    public class Calculation
    {
        public static decimal CalculateGst(decimal amount, int gst)
        {
            return (gst / amount) * 100;
        }
    }
}
