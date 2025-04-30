using Repository.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;
using RepositoryModels.Repository;
using Microsoft.EntityFrameworkCore;

namespace hotel_api.Notifications
{
    public class Notification
    {
        public async static Task<bool> SendMail(DbContextSql _context, string emailSubject, string htmlBody, int companyId, string sendAddress)
        {
            try
            {
                EmailCredential emaiCredential = await _context.EmailCredential.FirstOrDefaultAsync(x => x.CompanyId == companyId);

                if(emaiCredential == null)
                {
                    return false;
                }
                var fromAddress = new MailAddress(emaiCredential.Email, emaiCredential.UserName);
                var toAddress = new MailAddress(sendAddress);
                string fromPassword = emaiCredential.AppPassword; // Important: App Password, not your regular password
                

                var smtp = new SmtpClient
                {
                    Host = emaiCredential.Smtp,
                    Port = emaiCredential.Port,
                    EnableSsl = emaiCredential.SslTrue,
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

