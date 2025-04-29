using Microsoft.EntityFrameworkCore;
using Repository.Models;
namespace hotel_api.GeneralMethods
{
    public class DocumentHelper
    {
        public static async Task<string> UpdateDocumentNo(
            DbContext context,
            string bookingType,
            int companyId)
        {
            try
            {
                var getbookingno = await context.Set<DocumentMaster>()
                    .FirstOrDefaultAsync(x => x.Type == bookingType && x.CompanyId == companyId && x.IsActive);
                
                if (getbookingno == null)
                    return null;

                getbookingno.LastNumber += 1;
                await context.SaveChangesAsync();
                return "success";
            }
            catch
            {
                return null;
            }
        }
    }

}
