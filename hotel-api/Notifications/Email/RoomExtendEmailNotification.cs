using Repository.Models;
using RepositoryModels.Repository;

namespace hotel_api.Notifications.Email
{
    public class RoomExtendEmailNotification
    {
        private readonly DbContextSql _context;


        private CompanyDetails PropertyDetails { get; set; } = new CompanyDetails();
        private string ReservationNo { get; set; } = string.Empty;

        public string RoomNo { get; set; }

        public string OriginalCheckOutDate { get; set; }
        public string ExtenededCheckOutDate { get; set; }
        private GuestDetails GuestDetails { get; set; }


        

        public RoomExtendEmailNotification(DbContextSql context, CompanyDetails propertyDetails, string reservationNo, string roomNo, string originalCheckOutDate, string extenededCheckOutDate, GuestDetails guestDetails)
        {
            _context = context;
            PropertyDetails = propertyDetails;
            ReservationNo = reservationNo;
            RoomNo = roomNo;
            OriginalCheckOutDate = originalCheckOutDate;
            ExtenededCheckOutDate = extenededCheckOutDate;
            GuestDetails = guestDetails;
        }

        public async Task SendEmail()
        {
            string subject = $"Room Extend - {ReservationNo}";
            string htmlBody = @$"
               <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Room Extend Notification</title>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
            font-family: Arial, sans-serif;
        }}
        .email-container {{
            max-width: 600px;
            margin: auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background-color: #ffc107;
            color: #333;
            padding: 20px;
            text-align: center;
        }}
        .content {{
            padding: 30px 20px;
            color: #333333;
            line-height: 1.6;
        }}
        .info-table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }}
        .info-table td {{
            padding: 10px;
            color: #555555;
            border-bottom: 1px solid #eeeeee;
        }}
        .info-table td:first-child {{
            font-weight: bold;
            width: 40%;
        }}
        .footer {{
            background-color: #f1f1f1;
            text-align: center;
            font-size: 12px;
            color: #888888;
            padding: 20px;
        }}
        @media only screen and (max-width: 600px) {{
            .content, .header, .footer {{
                padding: 15px;
            }}
        }}
    </style>
</head>
<body>

    <div class=""email-container"">
        <div class=""header"">
            <h2>Room Extend Notification</h2>
        </div>

        <div class=""content"">
            <p>Dear <strong>{GuestDetails.GuestName}</strong>,</p>

            <p>We would like to inform you that your room has been successfully extended. Please find the updated room details below:</p>

            <table class=""info-table"">
                <tr>
                    <td>Reservation Number:</td>
                    <td>{ReservationNo}</td>
                </tr>
               
                <tr>
                    <td>Room:</td>
                    <td>{RoomNo}</td>
                </tr>

 <tr>
                    <td>Original Check Out Date:</td>
                    <td>{OriginalCheckOutDate}</td>
                </tr>

<tr>
                    <td>Extended Check Out Date:</td>
                    <td>{ExtenededCheckOutDate}</td>
                </tr>
               
            </table>

            <p style=""margin-top: 25px;"">
                If you need any assistance during the transition or have further questions, please do not hesitate to contact our front desk.
            </p>

            <p style=""margin-top: 30px;"">Thank you for staying with <strong>{PropertyDetails.CompanyName}</strong>. We hope you enjoy your updated accommodation.</p>
        </div>

        <div class=""footer"">
            {PropertyDetails.CompanyName} | {PropertyDetails.ContactNo1} <br/>
            {PropertyDetails.CompanyAddress}
        </div>
    </div>

</body>
</html>

";

            await Notification.SendMail(_context, subject, htmlBody, PropertyDetails.PropertyId, GuestDetails.Email);
        }
    }
}
