﻿using Repository.Models;

namespace hotel_api.Constants
{
    public class Constants
    {
        public static string ErrorMessage = "Someting went wrong. Try Again";
        public static DateTime DefaultDate = new DateTime(1900, 01, 01);

        //user roles
        public static string SuperAdmin = "superadmin";

        //room rates type
        public static string Standard = "Standard";
        public static string Weekend = "Weekend";
        public static string Custom = "Custom";
        public static string Hour = "Hour";

        //room rate priority
        public static int HighPrority = 1;
        public static int MediumPrority = 2;
        public static int LowPrority = 3;

        //Reservation format
        public static string SameDayFormat = "Same Day";
        public static string NightFormat = "Night";
        public static string Hour24Format = "24 Hours";

        //Gst master type
        public static string MultipleGst = "Multiple";
        public static string SingleGst = "Single";

        public static string WithGst = "With Gst";
        public static string WithoutGst = "Without Gst";

        public static string Inclusive = "Inclusive";
        public static string Exclusive = "Exclusive";

        //gst masters
        public static string Reservation = "Reservation";
        public static string Agent = "Agent";

        //Payment format
        public static string ReservationWisePayment = "Reservation";
        public static string PartPayment = "PartPayment";
        public static string RoomWisePayment = "RoomPayment";

        //payment status
        public static string AgentPayment = "Agent";
        public static string AdvancePayment = "Advance";
        public static string ReceivedPayment = "Received";

        //rooms status
        public static string Clean = "Clean";
        public static string CheckIn = "CheckIn";
        public static string Confirmed = "Confirmed";
        public static string Rejected = "Rejected";
        public static string Pending = "Pending";
        public static string CheckOut = "CheckOut";
        public static string Dirty = "Dirty";

        //document type
        public static string DocumentReservation = "Reservation";
        public static string DocumentInvoice = "Invoice";
        public static string DocumentKot = "KOT";

        public static List<string> AllRoomStatus = new List<string> { CheckIn, Confirmed, Pending, CheckOut };

        //invoice format
        public static string ReservationInvoice = "ReservationInvoice";
        public static string RoomInvoice = "RoomInvoice";

        public static List<string> AllowedExtensions = new List<string> { ".png", ".jpg", "jpeg",".pdf" };
        public static string InvalidFileError = "Invalid File type. Only PNG, JPG, JPEG are allowed";
        
        public async static Task<string> AddFile(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return "";
            }
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads/ContractFiles");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public static void SetMastersDefault(ICommonProperties model, int companyid, int userId, DateTime currentDate)
        {
            model.CreatedDate = currentDate;
            model.UpdatedDate = currentDate;
            model.IsActive = true;
            model.CompanyId = companyid;
            model.UserId = userId;
        }

    }
}
