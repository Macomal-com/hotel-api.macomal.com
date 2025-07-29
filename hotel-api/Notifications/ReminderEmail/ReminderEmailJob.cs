using Microsoft.EntityFrameworkCore;
using Quartz;
using RepositoryModels.Repository;
using System;

namespace hotel_api.Notifications.ReminderEmail
{
    public class ReminderEmailJob : IJob
    {
        private readonly DbContextSql _context;
      

        public ReminderEmailJob(DbContextSql context)
        {
            _context = context;
      
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //var now = DateTime.Now;
            //var today = now.Date;
            //var currentTime = now.ToString("HH:mm");

            //var reminders = await _context.ReminderHistoryMasters
            //    .Where(r => r.IsActive && !r.BillPaid)
            //    .Where(r => EF.Functions.DateDiffDay(today, r.DueDate) == r.DaysBefore)
            //    .Where(r => r.ReminderTime == currentTime)
            //    .ToListAsync();

            //foreach (var reminder in reminders)
            //{
            //    var toEmail = "recipient@example.com"; // TODO: Get user email
            //    var subject = $"Reminder: Bill Due on {reminder.DueDate:dd-MM-yyyy}";
            //    var body = $"Hello,<br><br>Your bill is due on <strong>{reminder.DueDate:dd-MM-yyyy}</strong>.<br><br>Regards,<br>Reminder System";

            //    //await _emailService.SendEmailAsync(toEmail, subject, body);

            //    Console.WriteLine($"📧 Email sent for ReminderId={reminder.ReminderId} at {currentTime}");
            //}
        }
    }

}
