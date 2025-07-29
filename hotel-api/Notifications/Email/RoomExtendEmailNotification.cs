using Repository.Models;
using RepositoryModels.Repository;

namespace hotel_api.Notifications.Email
{
    public class RoomExtendEmailNotification
    {
        private readonly DbContextSql _context;
        private CompanyDetails PropertyDetails { get; set; } = new CompanyDetails();
        private string ReservationNo { get; set; } = string.Empty;

        private GuestDetails? GuestDetails { get; set; } 

        private string RoomNo { get; set; } = string.Empty;
        public string PreviousCheckOutDate { get; set; } = string.Empty;
        public string ExtendDate { get; set; } = string.Empty;
        public string CheckoutFormat { get; set; } = string.Empty;
        public RoomExtendEmailNotification(DbContextSql context, CompanyDetails propertyDetails, string reservationNo, GuestDetails guestDetails, string roomNo, string previousCheckOutDate, string extendDate, string checkoutformat)
        {
            _context = context;
            PropertyDetails = propertyDetails;
            ReservationNo = reservationNo;
            GuestDetails = guestDetails;
            RoomNo = roomNo;
            PreviousCheckOutDate = previousCheckOutDate;
            ExtendDate = extendDate;
            CheckoutFormat = checkoutformat;
        }

        public async Task SendEmail()
        {
            string subject = $"Room Extend - {ReservationNo}";
            string htmlBody = @$"
                <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Room Extend Successful</title>
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">

    <table width=""100%"" style=""max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        <tr>
            <td style=""padding: 20px; text-align: center; background-color: #007bff; color: white; border-top-left-radius: 8px; border-top-right-radius: 8px;"">
                <h2>Room Extend Successful</h2>
            </td>
        </tr>

        <tr>
            <td style=""padding: 20px; color: #333333;"">
                <p>Dear <strong>{GuestDetails.GuestName}</strong>,</p>

                <p>We are pleased that your Room Extended sucessfully!</p>

                <table width=""100%"" style=""margin-top: 20px;"">
                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Reservation Number:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{ReservationNo}</td>
                    </tr>
                   
                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Room No:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{RoomNo}</td>
                    </tr>
 
                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>{(CheckoutFormat == Constants.Constants.SameDayFormat ? "No of Hours" : $@"Previous Check-Out Date")}</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{PreviousCheckOutDate}</td>
                    </tr>                  

                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>{(CheckoutFormat == Constants.Constants.SameDayFormat ? "Extended Hours" : $@"Extended Date")}</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{ExtendDate}</td>
                    </tr>

                    
                </table>

                <p style=""margin-top: 20px;"">If you have any questions or special requests, feel free to contact us.  
                <br>We look forward to welcoming you and ensuring you have a wonderful stay!</p>

                <p style=""margin-top: 30px;"">Thank you for choosing <strong>{PropertyDetails.CompanyName}</strong>!</p>
            </td>
        </tr>

        <tr>
            <td style=""padding: 20px; text-align: center; font-size: 12px; color: #888888; background-color: #f1f1f1; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px;"">
                {PropertyDetails.CompanyName} | {PropertyDetails.ContactNo1} | {PropertyDetails.CompanyAddress} <br/>
                
            </td>
        </tr>
    </table>

</body>
</html>
";

            await Notification.SendMail(_context, subject, htmlBody, PropertyDetails.PropertyId, GuestDetails.Email);
        }
    }
}
