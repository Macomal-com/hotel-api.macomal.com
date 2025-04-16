using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.DTO;
using Repository.Models;
using RepositoryModels.Repository;
using System.Text.RegularExpressions;

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
                if (response.Groups.Count == 0)
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
                response.Services = await (from service in _context.ServicableMaster
                                           join groups in _context.GroupMaster on service.GroupId equals groups.Id
                                           join subgroups in _context.SubGroupMaster on service.SubGroupId equals subgroups.SubGroupId
                                           where service.IsActive == true && service.CompanyId == companyId
                                           select new ServicesDTO
                                           {
                                               ServiceId = service.ServiceId,
                                               GroupId = service.GroupId,
                                               GroupName = groups.GroupName,
                                               SubGroupId = service.SubGroupId,
                                               SubGroupName = subgroups.SubGroupName,
                                               ServiceName = service.ServiceName,
                                               ServiceDescription = service.ServiceDescription,
                                               Amount = service.Amount,
                                               Discount = service.Discount,
                                               TaxType = service.TaxType,
                                               GstPercentage = groups.GST,
                                               //(TotalAmount, GstAmount) = Constants.Calculation.CalculateGst(service.Amount, groups.GST, service.TaxType),
                                               IgstPercentage = groups.IGST,
                                               //IgstAmount = service.IgstAmount,
                                               SgstPercentage = groups.SGST,
                                               //SgstAmount = service.SgstAmount,
                                               CgstPercentage = groups.CGST,
                                               //CgstAmount = service.CgstAmount,
                                               //TotalAmount = service.TotalAmount

                                           }).ToListAsync();

                foreach(var item in response.Services)
                {
                    (item.InclusiveTotalAmount, item.GstAmount) = Constants.Calculation.CalculateGst(item.Amount, item.GstPercentage, item.TaxType);

                    (item.InclusiveTotalAmount, item.IgstAmount) = Constants.Calculation.CalculateGst(item.Amount, item.IgstPercentage, item.TaxType);

                    (item.InclusiveTotalAmount, item.SgstAmount) = Constants.Calculation.CalculateGst(item.Amount, item.SgstPercentage, item.TaxType);

                    (item.InclusiveTotalAmount, item.CgstAmount) = Constants.Calculation.CalculateGst(item.Amount, item.CgstAmount, item.TaxType);

                   
                        item.InclusiveTotalAmount = item.Amount;

                    item.ExclusiveTotalAmount = item.Amount + item.GstAmount + item.IgstAmount + item.CgstAmount + item.SgstAmount;
                    
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", data = response });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }
        [HttpGet("GetDataFromBookingId")]
        public async Task<IActionResult> GetDataFromBookingId(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            if (id == 0)
            {
                
                return Ok(new { Code = 400, Message = "No Data found" });
            }
            try
            {
                var data = await (from bd in _context.BookingDetail
                                  join rm in _context.RoomMaster on bd.RoomId equals rm.RoomId
                                  join guest in _context.GuestDetails on bd.GuestId equals guest.GuestId
                                  where bd.BookingId == id && bd.IsActive == true && rm.IsActive == true && guest.IsActive == true &&
                                  bd.CompanyId == companyId && rm.CompanyId == companyId && guest.CompanyId == companyId &&
                                  bd.UserId == userId && rm.UserId == userId && guest.UserId == userId
                                  select new
                                  {
                                      bd.BookingId,
                                      bd.RoomId,
                                      rm.RoomNo,
                                      guest.GuestName,
                                      CheckInDate = bd.CheckInDate.ToString("yyyy-MM-dd"),
                                      CheckOutDate = bd.CheckOutDate.ToString("yyyy-MM-dd"),
                                  }).ToListAsync();
                if (data.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "Data Not Found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }
    }
}
