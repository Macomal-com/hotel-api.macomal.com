using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.DTO;
using Repository.Models;
using RepositoryModels.Repository;
using System.ComponentModel.Design;
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

                var getKotNo = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Type == Constants.Constants.DocumentKot && x.FinancialYear == financialYear);
                if (getKotNo == null || getKotNo.Suffix == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found.", data = getKotNo });
                }
                response.KotNumber = getKotNo.Prefix + getKotNo.Separator + getKotNo.Prefix1 + getKotNo.Separator + getKotNo.Prefix2 + getKotNo.Suffix + getKotNo.Number + getKotNo.LastNumber;

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
                    decimal netAmount = 0;
                    decimal gst = 0;
                    (netAmount, gst) = Constants.Calculation.CalculateGst(item.Amount, item.GstPercentage, item.TaxType);

                    item.ServicePrice = netAmount;
                    item.GstAmount = gst;
                    item.IgstAmount = gst;
                    (netAmount, gst) = Constants.Calculation.CalculateGst(item.Amount, item.CgstPercentage, item.TaxType);

                    item.CgstAmount = gst;

                    item.SgstAmount = gst;
                    if (item.TaxType == Constants.Constants.Inclusive)
                    {
                        
                        item.InclusiveTotalAmount = item.Amount;
                    }
                    else
                    {
                        item.ExclusiveTotalAmount = item.Amount + item.GstAmount;
                    }
                    
                    
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
                                      bd.ReservationNo,
                                      bd.RoomId,
                                      rm.RoomNo,
                                      guest.GuestName,
                                      CheckInDate = bd.CheckInDate.ToString("yyyy-MM-dd"),
                                      CheckOutDate = bd.CheckOutDate.ToString("yyyy-MM-dd"),
                                      bd.CheckOutTime,

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

        [HttpPost("SaveServices")]
        public async Task<IActionResult> SaveServices([FromBody]List<AdvanceService> advanceServices)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try 
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();
                var currentTime = DateTime.Now;

                if(advanceServices.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }
                foreach(var service in advanceServices)
                {
                    var validator = new AdvanceServicesValidator();
                    var result = validator.Validate(service);
                    if (!result.IsValid)
                    {
                        var errors = result.Errors.Select(x => new
                        {
                            Error = x.ErrorMessage,
                            Field = x.PropertyName
                        }).ToList();
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 202, message = errors });
                    }

                    Constants.Constants.SetMastersDefault(service, companyId, userId, currentTime);

                    await _context.AdvanceServices.AddAsync(service);
                    await _context.SaveChangesAsync();

                }

                bool isUpdated = await CalculateTotalServiceAmount(advanceServices[0].BookingId);
                if(isUpdated == false)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = Constants.Constants.ErrorMessage });
                }

                var getKotNo = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.Type == Constants.Constants.DocumentKot && x.CompanyId == companyId && x.IsActive == true);
                if (getKotNo == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = Constants.Constants.ErrorMessage });
                }
                getKotNo.LastNumber = getKotNo.LastNumber + 1;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Services added successfully" });
                

            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }   

        private async Task<bool> CalculateTotalServiceAmount(int bookingId)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            decimal totalServiceAmount = await _context.AdvanceServices.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == bookingId).SumAsync(x => x.TotalAmount);

            var bookingDetail = await _context.BookingDetail.FirstOrDefaultAsync(x => x.BookingId == bookingId && x.IsActive == true && x.CompanyId == companyId);

            if(bookingDetail == null)
            {
                return false;
            }

            bookingDetail.ServicesAmount = totalServiceAmount;
            bookingDetail.TotalAmount = Constants.Calculation.BookingTotalAmount(bookingDetail);

            _context.BookingDetail.Update(bookingDetail);
            await _context.SaveChangesAsync();

            return true;
        }
        [HttpGet("GetServiceList")]
        public async Task<IActionResult> GetServiceList()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            
            try
            {
                var data = await (from service in _context.AdvanceServices
                                  join room in _context.RoomMaster on service.RoomId equals room.RoomId
                                  join gm in _context.GroupMaster on service.GroupId equals gm.Id
                                  join booking in _context.BookingDetail on service.BookingId equals booking.BookingId
                                  join guest in _context.GuestDetails on booking.GuestId equals guest.GuestId
                                  join subgm in _context.SubGroupMaster on service.SubGroupId equals subgm.SubGroupId
                                  where service.IsActive == true && gm.IsActive == true && subgm.IsActive == true &&
                                  service.CompanyId == companyId && gm.CompanyId == companyId && subgm.CompanyId == companyId &&
                                  service.UserId == userId && gm.UserId == userId && subgm.UserId == userId
                                  select new
                                  {
                                      service.ReservationNo,
                                      guest.GuestName,
                                      service.ServiceName,
                                      gm.GroupName,
                                      subgm.SubGroupName,
                                      room.RoomNo,


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
