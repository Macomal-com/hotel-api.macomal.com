using Repository.Models;
using RepositoryModels.Repository;

namespace hotel_api.Notifications.Whatsapp
{
    public class CheckInWhatsAppNotification
    {
        private readonly DbContextSql _context;

        

        private CompanyDetails PropertyDetails = new CompanyDetails();
        private int CompanyId { get; set; }

        private List<BookingDetail> BookingDetail { get; set; } = new List<BookingDetail>();




        public CheckInWhatsAppNotification(DbContextSql context, CompanyDetails companyDetails, int companyId, List<BookingDetail> bookings)
        {
            _context = context;
            PropertyDetails = companyDetails;

            
            CompanyId = companyId;
            BookingDetail = bookings;
        }

        public async Task SendWhatsAppNotification()
        {
            foreach (var item in BookingDetail)
            {
               
            

            var requestBody = new
                {
                    countryCode = "+91",
                    phoneNumber = item.GuestDetails.PhoneNumber, // Replace with actual phone number
                    callbackData = "some text here",
                    type = "Template",
                    template = new
                    {
                        name = "checkedinall",
                        languageCode = "en_GB",
                        headerValues = new[] { PropertyDetails.CompanyName },
                        bodyValues = new[]
                        {

                             


                item.ReservationNo, item.CheckInDateTime.ToString("MMMM d, yyyy h:mm tt"), item.CheckOutTime, item.CheckOutDate.ToString("MMMM d, yyyy"), "RoomNo",item.NoOfNights.ToString(), item.TotalAmount.ToString(), item.TotalAmount.ToString(),"0", "0", "0"

                        }
                    }
                };

                await Notification.SendWhatsAppMessage(_context, CompanyId, requestBody);
            }

        }

    }
}
