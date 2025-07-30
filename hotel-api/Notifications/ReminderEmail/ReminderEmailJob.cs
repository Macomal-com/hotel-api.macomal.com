using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Repository.Models;
using Repository.ReportModels;
using RepositoryModels.Repository;
using System;
using System.Data;
using System.Net.Mail;
using System.Net;

namespace hotel_api.Notifications.ReminderEmail
{

    public class ReminderEmailDTO
    {
        public int Id { get; set; }
        public int DaysBefore { get; set; }
        public DateOnly DueDate { get; set; }
        public string DocumentPath { get; set; } = string.Empty;
        public string ReminderTime { get; set; } = string.Empty;
        public string ReminderType { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public string ReminderMail { get; set; } = string.Empty;
        public string UniqueCaNo { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Smtp { get; set; } = string.Empty;
        public bool SslTrue { get; set; }
        public string AppPassword { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public class ReminderEmailJob : IJob
    {
        private readonly DbContextSql _context;
      

        public ReminderEmailJob(DbContextSql context)
        {
            _context = context;
      
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Background process started" + DateTime.Now.ToString("dd-MM-yyyy hh:mm"));
            DateTime currentTime = DateTime.Now;
            try
            {
                DataTable table = new DataTable();
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("ReminderHistorySchedule", connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@currentTime", currentTime.ToString("hh:mm"));

                        await connection.OpenAsync();

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(table);
                        }

                       
                        
                    }
                }

                var reminderList = new List<ReminderEmailDTO>();

                foreach (DataRow row in table.Rows)
                {
                    var reminder = new ReminderEmailDTO
                    {
                        Id = row.Field<int>("Id"),
                        DaysBefore = row.Field<int>("DaysBefore"),
                        DueDate = DateOnly.FromDateTime(row.Field<DateTime>("DueDate")),
                        DocumentPath = row.Field<string>("DocumentPath") ?? string.Empty,
                        ReminderTime = row.Field<string>("ReminderTime") ?? string.Empty,
                        ReminderType = row.Field<string>("ReminderType") ?? string.Empty,
                        HolderName = row.Field<string>("HolderName") ?? string.Empty,
                        ReminderMail = row.Field<string>("ReminderMail") ?? string.Empty,
                        UniqueCaNo = row.Field<string>("UniqueCaNo") ?? string.Empty,
                        CompanyName = row.Field<string>("CompanyName") ?? string.Empty,
                        CompanyId = row.Field<int>("CompanyId"),
                        Email = row.Field<string>("Email") ?? string.Empty,
                        Password = row.Field<string>("Password") ?? string.Empty,
                        Port = row.Field<int>("Port"),
                        Smtp = row.Field<string>("Smtp") ?? string.Empty,
                        SslTrue = row.Field<bool>("SslTrue"),
                        AppPassword = row.Field<string>("AppPassword") ?? string.Empty,
                        UserName = row.Field<string>("UserName") ?? string.Empty
                    };

                    reminderList.Add(reminder);
                }

                Console.WriteLine("Reminder List", reminderList.Count );
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy hh:mm"));



                foreach (var item in reminderList)
                {
                    Console.WriteLine("For reminder" + item.ReminderType, item.ReminderTime);
                    Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy hh:mm"));
                            //send email
                            string subject = $"Reminder - {item.ReminderType}";

                            string htmlBody = @$"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Reminder</title>
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">

    <table width=""100%"" style=""max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        <tr>
            <td style=""padding: 20px; text-align: center; background-color: #007bff; color: white; border-top-left-radius: 8px; border-top-right-radius: 8px;"">
                <h2>Upcoming Reminder</h2>
            </td>
        </tr>

        <tr>
            <td style=""padding: 20px; color: #333333;"">
                <p>Dear <strong>{item.HolderName}</strong>,</p>

                <p>This is a gentle reminder regarding your upcoming task as per our records.</p>

                <table width=""100%"" style=""margin-top: 20px;"">
                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Reminder For:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.UniqueCaNo}</td>
                    </tr>

                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Due Date:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.DueDate.ToString("dd-MM-yyy")}</td>
                    </tr>

 <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Property Name:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.CompanyName}</td>
                    </tr>
                   
                </table>

                
            </td>
        </tr>

      
    </table>

</body>
</html>
";


                    var fromAddress = new MailAddress(item.Email, item.UserName);
                    var toAddress = new MailAddress(item.ReminderMail);
                    string fromPassword = item.AppPassword; // Important: App Password, not your regular password


                    var smtp = new SmtpClient
                    {
                        Host = item.Smtp,
                        Port = item.Port,
                        EnableSsl = item.SslTrue,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                        Timeout = 20000
                    };

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true,



                    })
                    {
                        

                        smtp.Send(message);
                    }


                   
                      
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           

            
        }
    }

}
