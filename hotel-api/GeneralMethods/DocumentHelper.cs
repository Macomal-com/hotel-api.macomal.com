using Azure;
using Microsoft.EntityFrameworkCore;
using Repository.Models;
using RepositoryModels.Repository;
namespace hotel_api.GeneralMethods
{
    public class DocumentHelper
    {

        public static async Task<string> GetDocumentNo(DbContextSql _context,
            string bookingType,
            int companyId, string financialYear)
        {
            var getbookingno = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Type == bookingType && x.FinancialYear == financialYear && x.IsActive == true);

            if (getbookingno == null || getbookingno.Suffix == null)
            {
                return null;
            }
            return getbookingno.Prefix + getbookingno.Separator + getbookingno.Prefix1 + getbookingno.Separator + getbookingno.Prefix2 + getbookingno.Suffix + getbookingno.Number + getbookingno.LastNumber;
        }

        public static async Task<string> UpdateDocumentNo(
            DbContextSql context,
            string bookingType,
            int companyId,string financialYear)
        {
            try
            {
                var getbookingno = await context.DocumentMaster
                    .FirstOrDefaultAsync(x => x.Type == bookingType && x.CompanyId == companyId && x.IsActive && x.FinancialYear == financialYear);
                
                if (getbookingno == null)
                    return null;

                getbookingno.LastNumber += 1;
                context.DocumentMaster.Update(getbookingno);

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
