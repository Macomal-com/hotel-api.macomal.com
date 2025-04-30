namespace hotel_api.Notifications
{
    public class CancelBookingNotificationDTO
    {
        public string RoomNo { get; set; } = string.Empty;
        public int Pax { get; set; }
   
        public string CancelDate { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestPhoneNo { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;
    }
}
