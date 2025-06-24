using Repository.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;
using RepositoryModels.Repository;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace hotel_api.Notifications
{
    public class Notification
    {
        public async static Task<bool> SendMail(DbContextSql _context, string emailSubject, string htmlBody, int companyId, string sendAddress, byte[]? attachment = null)
        {
            try
            {
                EmailCredential? emaiCredential = await _context.EmailCredential.FirstOrDefaultAsync(x => x.CompanyId == companyId);

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
                    IsBodyHtml = true,
                

                   
                })
                {
                    if (attachment != null)
                    {
                        var pdfStream = new MemoryStream(attachment);
                        var pdf = new Attachment(pdfStream, "invoice.pdf", "application/pdf");
                        message.Attachments.Add(pdf);

                    }

                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    
        public static async Task<bool> SendWhatsAppMessage(DbContextSql _context, int companyId, object requestBody)
        {
            try
            {
                WhatsAppCredentials? whatsAppCredentials = await _context.WhatsAppCredentials.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.IsActive);

                if (whatsAppCredentials == null)
                {
                    return false;
                }
                var url = whatsAppCredentials.Url;
                var authKey = whatsAppCredentials.AuthKey;

                

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authKey}");

                var jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);

                var responseString = await response.Content.ReadAsStringAsync();
                if(response.StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine($"Status Code: {response.StatusCode}");
                    Console.WriteLine($"Response: {responseString}");
                    return true;
                }
                else
                {
                    return false;
                }
                    

            }
            catch(Exception ex)
            {
                return false;
            }
            
        }
    
    }
}

