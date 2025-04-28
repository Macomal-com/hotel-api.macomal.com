using Repository.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;

namespace hotel_api.Notifications
{
    public class Notification
    {
        public static bool SendMail(string emailSubject, string htmlBody)
        {
            try
            {
                var fromAddress = new MailAddress("enquiry@macoinfotech.com", "Himanshi Goel");
                var toAddress = new MailAddress("himanshi@macoinfotech.us", "Himanshi Test name");
                const string fromPassword = ""; // Important: App Password, not your regular password
                

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = emailSubject,
                    Body = htmlBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

