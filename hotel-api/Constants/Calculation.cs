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
                return (netAmount, gstAmount);
            }
            else 
            {
                return (amount,(gst / amount) * 100);
            }
                
        }

        public static decimal CalculatePercentage(decimal amount, decimal percentage)
        {
            return (amount * percentage) / 100;
        }
    }
}
