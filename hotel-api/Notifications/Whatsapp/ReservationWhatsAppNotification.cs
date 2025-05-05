using Repository.Models;
using RepositoryModels.Repository;

namespace hotel_api.Notifications.Whatsapp
{
    public class ReservationWhatsAppNotification
    {
        private readonly DbContextSql _context;
        private CompanyDetails PropertyDetails { get; set; } = new CompanyDetails();
      
        private GuestDetails GuestDetails = new GuestDetails();
        public int CompanyId { get; set; }

        public List<BookingDetail> BookingDetail { get; set; } = new List<BookingDetail>();




        public ReservationWhatsAppNotification(DbContextSql context, CompanyDetails companyDetails,  GuestDetails guestDetails, int companyId, List<BookingDetail> bookings)
        {
            _context = context;
            PropertyDetails = companyDetails;
            
            GuestDetails = guestDetails;
            CompanyId = companyId;
            BookingDetail = bookings;
        }

        public  async Task SendWhatsAppNotification()
        {
            foreach(var item in BookingDetail)
            {
                var requestBody = new
                {
                    countryCode = "+91",
                    phoneNumber = GuestDetails.PhoneNumber, // Replace with actual phone number
                    callbackData = "some text here",
                    type = "Template",
                    template = new
                    {
                        name = "checkedin",
                        languageCode = "en_US",
                        headerValues = new[] { GuestDetails.GuestName },
                        bodyValues = new[]
                        {
                            PropertyDetails.CompanyName, item.ReservationNo, item.CheckInDateTime.ToString("MMMM d, yyyy h:mm tt"), item.CheckOutTime, item.CheckOutDate.ToString("MMMM d, yyyy"), item.NoOfNights.ToString(), item.TotalAmount.ToString(), item.RoomTypeName, PropertyDetails.ContactNo1, PropertyDetails.Email, PropertyDetails.CompanyName
                    
                        }
                    }
                };

                await Notification.SendWhatsAppMessage(_context, CompanyId, requestBody);
            }
            
        }
    }
}
