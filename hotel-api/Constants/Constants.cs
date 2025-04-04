namespace hotel_api.Constants
{
    public class Constants
    {
        public static string ErrorMessage = "Someting went wrong. Try Again";
        public static DateTime DefaultDate = new DateTime(1900, 01, 01);

        public static string SuperAdmin = "superadmin";

        public static string Standard = "Standard";
        public static string Weekend = "Weekend";
        public static string Custom = "Custom";
        public static string Hour = "Hour";
        public static int HighPrority = 1;
        public static int MediumPrority = 2;
        public static int LowPrority = 3;

        public static string SameDayFormat = "Same Day";
        public static string NightFormat = "Night";
        public static string Hour24Format = "24 Hours";

        public static string MultipleGst = "Multiple";
        public static string SingleGst = "Single";

        public static string WithGst = "With Gst";
        public static string WithoutGst = "Without Gst";

        public static string Inclusive = "Inclusive";
        public static string Exclusive = "Exclusive";

        public static string Reservation = "Reservation";

        //Payment format
        public static string ReservationWisePayment = "Reservation";

        //rooms status
        public static string Clean = "Clean";

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
    }
}
