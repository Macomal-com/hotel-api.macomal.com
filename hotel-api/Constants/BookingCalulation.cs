using Repository.Models;

namespace hotel_api.Constants
{
    public class BookingCalulation
    {
        //CALCULATE ALL ROOMS TOTAL AMOUNT
        public static (decimal RoomAmout, decimal TotalGst, decimal TotalAmount) ReservationRoomsTotal(List<BookingDetailDTO> bookings)
        {
            decimal roomAmount = 0;
            decimal gst = 0;
            decimal totalAmount = 0;
            foreach (var item in bookings)
            {
                if (item.NoOfRooms > 0)
                {
                    int rooms = item.NoOfRooms;

                    while (rooms > 0)
                    {
                        roomAmount = roomAmount + item.BookingAmount;
                        gst = gst + item.GstAmount;
                        totalAmount = totalAmount + item.TotalBookingAmount;
                        rooms--;
                    }
                }
                else
                {
                    roomAmount = roomAmount + item.BookingAmount;
                    gst = gst + item.GstAmount;
                    totalAmount = totalAmount + item.TotalBookingAmount;
                }


            }

            roomAmount = Calculation.RoundOffDecimal(roomAmount);
            gst = Calculation.RoundOffDecimal(gst);
            totalAmount = Calculation.RoundOffDecimal(totalAmount);

            return (roomAmount, gst, totalAmount);
        }



        public static decimal BookingTotalAmount(BookingDetail booking)
        {
            return (booking.BookingAmount + booking.GstAmount + booking.TotalServicesAmount + booking.EarlyCheckInCharges + booking.LateCheckOutCharges) - booking.CheckOutDiscoutAmount;
        }
    }
}
