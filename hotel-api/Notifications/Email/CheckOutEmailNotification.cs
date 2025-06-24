using Repository.Models;
using RepositoryModels.Repository;
using System.ComponentModel.Design;

namespace hotel_api.Notifications.Email
{
    public class CheckOutNotificationDTO
    {
        public string RoomNo { get; set; } = string.Empty;
        public int Pax { get; set; }
        public string CheckInDateTime { get; set; } = string.Empty;
        public string CheckOutDateTime { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestPhoneNo { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        
    }
    public class CheckOutEmailNotification
    {
        private readonly DbContextSql _context;
        private int CompanyId { get; set; }
        private  CompanyDetails CompanyDetails = new CompanyDetails();
        private List<CheckOutNotificationDTO> inNotificationDTOs = new List<CheckOutNotificationDTO>();

        private byte[]? attachment; 
        public CheckOutEmailNotification(DbContextSql context, List<CheckOutNotificationDTO> inNotificationDTOs, int companyId, CompanyDetails companyDetails, byte[] attachment)
        {
            _context = context;
            this.inNotificationDTOs = inNotificationDTOs;
            CompanyId = companyId;
            CompanyDetails = companyDetails;
            this.attachment = attachment;
        }


        public async Task SendEmail()
        {
            foreach (var item in inNotificationDTOs)
            {
                string subject = $"Check Out Successful - {item.ReservationNo}";
                string htmlBody = @$"
                <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Check Out Confirmation</title>
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">

    <table width=""100%"" style=""max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        <tr>
            <td style=""padding: 20px; text-align: center; background-color: #007bff; color: white; border-top-left-radius: 8px; border-top-right-radius: 8px;"">
                <h2>Check Out Confirmed</h2>
            </td>
        </tr>

        <tr>
            <td style=""padding: 20px; color: #333333;"">
                <p>Dear <strong>{item.GuestName}</strong>,</p>

                <p>We are pleased to inform you that your check-out has been successfully confirmed!</p>

                <table width=""100%"" style=""margin-top: 20px;"">
                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Reservation Number:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.ReservationNo}</td>
                    </tr>
<tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Room Type:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.RoomNo}</td>
                    </tr>

  <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Room Category:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.RoomType}</td>
                    </tr>

  <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Check In Date:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.CheckInDateTime}</td>
                    </tr>
                   
                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Check Out Date:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.CheckOutDateTime}</td>
                    </tr>

<tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Pax:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.Pax}</td>
                    </tr>
                </table>

                <p style=""margin-top: 20px;"">If you have any questions or special requests, feel free to contact us.  
                <br>We look forward to welcoming you and ensuring you have a wonderful stay!</p>

                <p style=""margin-top: 30px;"">Thank you for choosing <strong>{CompanyDetails.CompanyName}</strong>!</p>
            </td>
        </tr>

        <tr>
            <td style=""padding: 20px; text-align: center; font-size: 12px; color: #888888; background-color: #f1f1f1; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px;"">
                {CompanyDetails.CompanyName} | {CompanyDetails.ContactNo1} | {CompanyDetails.CompanyAddress} <br/>
                
            </td>
        </tr>
    </table>

</body>
</html>

                ";

                await Notifications.Notification.SendMail(_context, subject, htmlBody, CompanyId, item.GuestEmail, attachment);
            }

        }

    }
}
