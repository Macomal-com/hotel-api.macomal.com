using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.DTO;
using RepositoryModels.Repository;

namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController(DbContextSql context, IMapper mapper) : ControllerBase
    {
        private readonly DbContextSql _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet("GetServicesFormData")]
        public async Task<IActionResult> GetServicesFormData()
        {
            try
            {
                var response = new RoomServiceFormResponse();
                //kot number
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                string financialYear = (HttpContext.Request.Headers["FinancialYear"]).ToString();

                var getbookingno = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Type == Constants.Constants.DocumentKot && x.FinancialYear == financialYear);
                if (getbookingno == null || getbookingno.Suffix == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found.", data = getbookingno });
                }
                response.KotNumber = getbookingno.Prefix + getbookingno.Separator + getbookingno.Prefix1 + getbookingno.Separator + getbookingno.Prefix2 + getbookingno.Suffix + getbookingno.Number + getbookingno.LastNumber;

                //groups
                response.Groups = await _context.GroupMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync();
                if(response.Groups.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "Groups not found" });
                }
                //subgroup
                response.SubGroups = await _context.SubGroupMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync();
                if (response.SubGroups.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "Sub Groups not found" });
                }
                //services

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
            
        }
    }
}
