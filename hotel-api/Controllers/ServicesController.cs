using AutoMapper;
using Azure;
using hotel_api.Constants;
using hotel_api.GeneralMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.DTO;
using Repository.Models;
using RepositoryModels.Repository;
using System.ComponentModel.Design;
using System.Data;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly DbContextSql _context;
        private readonly IMapper _mapper;
        private int companyId;
        private string financialYear = string.Empty;
        private int userId;
        public ServicesController(DbContextSql contextSql, IMapper map, IHttpContextAccessor httpContextAccessor)
        {
            _context = contextSql;
            _mapper = map;
            var headers = httpContextAccessor.HttpContext?.Request?.Headers;
            if (headers != null)
            {
                if (headers.TryGetValue("CompanyId", out var companyIdHeader) &&
           int.TryParse(companyIdHeader, out int comp))
                {
                    this.companyId = comp;
                }


                if (headers.TryGetValue("FinancialYear", out var financialYearHeader))
                {
                    this.financialYear = financialYearHeader.ToString();
                }

                if (headers.TryGetValue("UserId", out var userIdHeader) &&
                int.TryParse(companyIdHeader, out int id))
                {
                    this.userId = id;
                }
            }


        }

        [HttpGet("GetServicesFormData")]
        public async Task<IActionResult> GetServicesFormData()
        {
            try
            {
                var response = new RoomServiceFormResponse();
                //kot number

                string updatedKotNo = await DocumentHelper.GetDocumentNo(_context, Constants.Constants.DocumentKot, companyId, financialYear);

                if (updatedKotNo == null)
                {
                    return Ok(new { Code = 400, Message = "Document number not found." });
                }
                else
                {
                    response.KotNumber = updatedKotNo;
                }

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
                                  bd.CompanyId == companyId && rm.CompanyId == companyId && guest.CompanyId == companyId 
                                  
                                  select new
                                  {
                                      bd.BookingId,
                                      bd.ReservationNo,
                                      bd.RoomId,
                                      rm.RoomNo,
                                      guest.GuestName,
                                      bd.CheckInDateTime,
                                      bd.CheckOutDateTime,
                                      CheckInDate = bd.CheckInDate.ToString("yyyy-MM-dd"),
                                      CheckOutDate = bd.CheckOutDate.ToString("yyyy-MM-dd"),
                                      bd.CheckOutTime,
                                      bd.CheckInTime

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
                  

                    
                    

                }
                await _context.SaveChangesAsync();
                bool isUpdated = await CalculateTotalServiceAmount(advanceServices[0].BookingId);
                if(isUpdated == false)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = Constants.Constants.ErrorMessage });
                }


                string resultKot = await DocumentHelper.UpdateDocumentNo(_context, Constants.Constants.DocumentKot, companyId, financialYear);
                if (resultKot == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Error while updating document" });
                }

                
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
            List<AdvanceService> services  = await _context.AdvanceServices.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == bookingId).ToListAsync();

            decimal servicesAmount = services.Sum(x => x.ServicePrice * x.Quantity);
            decimal taxAmount = services.Sum(x => x.GstAmount);
            decimal totalServiceAmount = services.Sum(x => x.TotalAmount);

            var bookingDetail = await _context.BookingDetail.FirstOrDefaultAsync(x => x.BookingId == bookingId && x.IsActive == true && x.CompanyId == companyId);

            if(bookingDetail == null)
            {
                return false;
            }
            bookingDetail.ServicesAmount = servicesAmount;
            bookingDetail.ServicesTaxAmount = taxAmount;
            bookingDetail.TotalServicesAmount = totalServiceAmount;
           
            bookingDetail.TotalAmount = BookingCalulation.BookingTotalAmount(bookingDetail);

            _context.BookingDetail.Update(bookingDetail);
            await _context.SaveChangesAsync();

            return true;
        }
        [HttpGet("GetServiceList")]
        public async Task<IActionResult> GetServiceList()
        {
          
            try
            {
                var data = await (from service in _context.AdvanceServices
                                  join room in _context.RoomMaster on service.RoomId equals room.RoomId
                                  join roomType in _context.RoomCategoryMaster on room.RoomTypeId equals roomType.Id
                                  join gm in _context.GroupMaster on service.GroupId equals gm.Id
                                  join booking in _context.BookingDetail on service.BookingId equals booking.BookingId
                                  join guest in _context.GuestDetails on booking.GuestId equals guest.GuestId
                                  join subgm in _context.SubGroupMaster on service.SubGroupId equals subgm.SubGroupId
                                  where service.IsActive == true && gm.IsActive == true && subgm.IsActive == true &&
                                  service.CompanyId == companyId && gm.CompanyId == companyId && subgm.CompanyId == companyId
                                  select new
                                  {
                                      service.Id,
                                      service.ReservationNo,
                                      guest.GuestName,
                                      service.ServiceName,
                                      room.RoomNo,
                                      roomType.Type,
                                      guest.PhoneNumber,                                      
                                      service.KotNo,
                                      ServiceDate = service.ServiceDate.ToString("yyyy-MM-dd"),
                                      service.ServiceTime,
                                      service.TotalServicePrice,
                                      service.Quantity,
                                      service.TotalAmount,                                      
                                      gm.GroupName,
                                      subgm.SubGroupName,
                                      booking.Status,
                                      service.BookingId
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


        [HttpGet("GetServiceById/{serviceId}")]
        public async Task<IActionResult> GetServiceById(int serviceId)
        {
            try
            {
                if(serviceId == 0)
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }

                var services = await (
                    from roomservice in _context.AdvanceServices
                    join service in _context.ServicableMaster on roomservice.ServiceId equals service.ServiceId
                                           join groups in _context.GroupMaster on service.GroupId equals groups.Id
                                           join subgroups in _context.SubGroupMaster on service.SubGroupId equals subgroups.SubGroupId
                                           where 
                                           roomservice.IsActive == true && roomservice.Id == serviceId &&
                                           roomservice.CompanyId == companyId
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
                                               Quantity = roomservice.Quantity,
                                               DiscountAmount = roomservice.DiscountAmount,
                                               BookingId = roomservice.BookingId,
                                               Total = service.Amount * roomservice.Quantity,
                                               KotNo = roomservice.KotNo,
                                               ServiceDate = roomservice.ServiceDate,
                                               ServiceTime = roomservice.ServiceTime,
                                               Id = roomservice.Id
                                           }).FirstOrDefaultAsync();


                if (services == null)
                {
                    return Ok(new { Code = 400, Message = "Service not found" });
                }

                decimal netAmount = 0;
                    decimal gst = 0;
                    (netAmount, gst) = Constants.Calculation.CalculateGst(services.Amount, services.GstPercentage, services.TaxType);

                    services.ServicePrice = netAmount;
                    services.GstAmount = gst;
                    services.IgstAmount = gst;
                    (netAmount, gst) = Constants.Calculation.CalculateGst(services.Amount, services.CgstPercentage, services.TaxType);

                    services.CgstAmount = gst;

                    services.SgstAmount = gst;
                    if (services.TaxType == Constants.Constants.Inclusive)
                    {

                        services.InclusiveTotalAmount = services.Amount;
                    }
                    else
                    {
                        services.ExclusiveTotalAmount = services.Amount + services.GstAmount;
                    }


           


           
             

                var booking = await (from bd in _context.BookingDetail
                                  join rm in _context.RoomMaster on bd.RoomId equals rm.RoomId
                                  join guest in _context.GuestDetails on bd.GuestId equals guest.GuestId
                                  where bd.BookingId == services.BookingId && bd.IsActive == true && rm.IsActive == true && guest.IsActive == true &&
                                  bd.CompanyId == companyId && rm.CompanyId == companyId && guest.CompanyId == companyId 
                                  select new
                                  {
                                      bd.BookingId,
                                      bd.ReservationNo,
                                      bd.RoomId,
                                      rm.RoomNo,
                                      guest.GuestName,
                                      bd.CheckInDateTime,
                                      bd.CheckOutDateTime,
                                      CheckInDate = bd.CheckInDate.ToString("yyyy-MM-dd"),
                                      CheckOutDate = bd.CheckOutDate.ToString("yyyy-MM-dd"),
                                      bd.CheckOutTime,
                                      bd.CheckInTime

                                  }).FirstOrDefaultAsync();
                if(booking == null)
                {
                    return Ok(new { Code = 400, Message = "Booking not found" });
                }

                var result = new
                {
                    Service = services,
                    Booking = booking
                };

                return Ok(new { Code = 200, Message = "Service fetched successfully", data = result });
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("UpdateServices")]
        public async Task<IActionResult> UpdateServices([FromBody] List<AdvanceService> advanceServices)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                var currentTime = DateTime.Now;

                if (advanceServices.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }
                foreach (var service in advanceServices)
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



                    var advanceService = await _context.AdvanceServices.FirstOrDefaultAsync(x => x.IsActive == true && x.Id == service.Id && x.CompanyId == companyId);

                    if(advanceService == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, message = "Service not found" });
                    }

                    

                    advanceService.ServicePrice = service.ServicePrice;
                    advanceService.Quantity = service.Quantity;
                    advanceService.TaxType = service.TaxType;
                    advanceService.TotalAmount = service.TotalAmount;
                    advanceService.GSTPercentage = service.GSTPercentage;
                    advanceService.GstAmount = service.GstAmount;
                    advanceService.IgstAmount = service.IgstAmount;
                    advanceService.IGSTPercentage = service.IGSTPercentage;
                    advanceService.CGSTPercentage = service.CGSTPercentage;
                    advanceService.CgstAmount = service.CgstAmount;
                    advanceService.SgstAmount = service.SgstAmount;
                    advanceService.SGSTPercentage = service.SGSTPercentage;
                    advanceService.TotalServicePrice = service.TotalServicePrice;
                    advanceService.ServiceDate = service.ServiceDate;
                    advanceService.ServiceTime = service.ServiceTime;
                    advanceService.UpdatedDate = currentTime;

                    _context.AdvanceServices.Update(advanceService);
                }
                await _context.SaveChangesAsync();
                bool isUpdated = await CalculateTotalServiceAmount(advanceServices[0].BookingId);
                if (isUpdated == false)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = Constants.Constants.ErrorMessage });
                }


               


                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Service updated successfully" });


            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpDelete("DeleteService/{serviceId}")]
        public async Task<IActionResult> DeleteService(int serviceId, int bookingId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                var currentTime = DateTime.Now;

                if (serviceId == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }

                var advanceService = await _context.AdvanceServices.FirstOrDefaultAsync(x => x.IsActive == true && x.Id == serviceId && x.CompanyId == companyId);

                if(advanceService == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Service not found" });
                }

                advanceService.IsActive = false;
                advanceService.UpdatedDate = currentTime;

                await _context.SaveChangesAsync();
                bool isUpdated = await CalculateTotalServiceAmount(bookingId);
                if (isUpdated == false)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = Constants.Constants.ErrorMessage });
                }





                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Service deleted successfully" });


            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }
   
    
        
    
    }
}
