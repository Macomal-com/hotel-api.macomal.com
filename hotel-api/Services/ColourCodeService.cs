using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System;

namespace hotel_api.Services
{
    public class ColourCodeService
    {
        private static readonly Random rand = new Random();

        private static string GenerateReadableRandomColor()
        {
            int r = rand.Next(40, 216);
            int g = rand.Next(40, 216);
            int b = rand.Next(40, 216);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static string GetUniqueColor(DbContextSql dbContext, int companyid)
        {
            string color;
            do
            {
                color = GenerateReadableRandomColor();
            }
            while (dbContext.RoomCategoryMaster.Any(b => b.IsActive == true &&  b.ColorCode == color && b.CompanyId == companyid));

            return color;
        }
    }
}
