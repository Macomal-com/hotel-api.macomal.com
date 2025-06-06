using AutoMapper;
using Azure;
using Azure.Core;
using hotel_api.Constants;
using hotel_api.GeneralMethods;
using hotel_api.Notifications;
using hotel_api.Notifications.Email;
using hotel_api.Notifications.Whatsapp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Repository.DTO;
using Repository.Models;
using Repository.RequestDTO;
using RepositoryModels.Repository;
using System;
using System.ComponentModel.Design;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly DbContextSql _context;
        private readonly IMapper _mapper ;
        private int companyId;
        private string financialYear = string.Empty;
        private int userId;

        public BookingController(DbContextSql contextSql, IMapper map,IHttpContextAccessor httpContextAccessor)
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

        //GUEST APIS
        [HttpGet("GetGuestDetails")]
        public async Task<IActionResult> GetGuestDetails()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);


                var data = await _context.GuestDetails.Where(bm => bm.IsActive && bm.CompanyId == companyId).Select(x => new GuestDetails
                {
                    GuestId = x.GuestId,
                    GuestName = x.GuestName,
                    Nationality = x.Nationality,
                    StateName = x.StateName,
                    Address = x.Address,
                    City = x.City,
                    PhoneNumber = x.PhoneNumber,
                    Email = x.Email,
                    GuestImage = x.GuestImage,
                    Other1 = x.Other1,
                    Other2 = x.Other2,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate,
                    UserId = x.UserId,
                    CompanyId = x.CompanyId,
                    CityId = x.CityId,
                    StateId = x.StateId,
                    CountryId = x.CountryId,
                    Gender = x.Gender,
                    IdType = x.IdType,
                    IdNumber = x.IdNumber,
                    GuestNamePhone = x.GuestName + " : " + x.PhoneNumber
                }).ToListAsync();

                

                if (data.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("CheckRoomAvailaibility")]
        public async Task<IActionResult> CheckRoomAvailaibility(DateOnly checkInDate, string checkInTime, DateOnly checkOutDate, string checkOutTime, string pageName = "", int roomTypeId = 0)
        {
            try
            {
               
                if (checkInDate == null || checkOutDate == null || checkInDate == DateOnly.MinValue || checkOutDate == DateOnly.MinValue || checkInDate == Constants.Constants.DefaultDate || checkOutDate == Constants.Constants.DefaultDate)
                {
                    var result = await (from room in _context.RoomMaster
                                        join category in _context.RoomCategoryMaster on room.RoomTypeId equals category.Id
                                        where room.IsActive == true && room.CompanyId == companyId
                                        select new
                                        {
                                            RoomId = room.RoomId,
                                            RoomNo = room.RoomNo,
                                            RoomDesc = room.Description,
                                            CategoryId = room.RoomTypeId,
                                            RoomCategory = category.Type,
                                            RoomStatus = "Clean"
                                        }).ToListAsync();
                    return Ok(new { Code = 200, message = "Room retrieved successfully.", data = result, AvailableRooms = result.Count });
                }
                else
                {
                    DataSet dataSet = await GetRoomAvailability(checkInDate, checkInTime, checkOutDate, checkOutTime, pageName, roomTypeId);
                    if (dataSet == null)
                    {
                        return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                    }
                    var rows = new List<Dictionary<string, object>>();
                    var AvailableRooms = new Dictionary<string, object>();
                    var dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            dict[col.ColumnName] = row[col];
                        }
                        rows.Add(dict);
                    }

                    var dataTable2 = dataSet.Tables[1];
                    foreach (DataRow row in dataTable2.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dataTable2.Columns)
                        {
                            dict[col.ColumnName] = row[col];
                        }
                        AvailableRooms = dict;
                    }
                    return Ok(new { Code = 200, message = "Room charges retrieved successfully.", data = rows, AvailableRooms = AvailableRooms });
                }
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        

        [HttpGet("GetReservationFormData")]
        public async Task<IActionResult> GetReservationFormData()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                string financialYear = (HttpContext.Request.Headers["FinancialYear"]).ToString();

                var getbookingno = await DocumentHelper.GetDocumentNo(_context, Constants.Constants.DocumentReservation, companyId, financialYear);   

                if (getbookingno == null)
                {
                    return Ok(new { Code = 400, Message = "Document number not found.", data = getbookingno });
                }
                var bookingno = getbookingno;

                var agentDetails = await _context.AgentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId).Select(x => new
                {
                    AgentId = x.AgentId,
                    AgentName = x.AgentName,
                    AgentType = x.AgentType,
                    Commission = x.Commission,
                    Tcs = x.Tcs,
                    Tds = x.Tds,
                    GstNo = x.GstNo,
                    GstPercentage = x.GstPercentage,
                    GstType = x.GstType
                }).ToListAsync();

                var roomCategories = await _context.RoomCategoryMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).Select(x => new
                {
                    Id = x.Id,
                    Type = x.Type,
                    Description = x.Description,
                    MinPax = x.MinPax,
                    MaxPax = x.MaxPax,
                    DefaultPax = x.DefaultPax
                }).ToListAsync();

                var paymentModes = await _context.PaymentMode.Where(x => x.IsActive == true && x.CompanyId == companyId)
                    .Select(x => new
                    {
                        PaymentId = x.PaymentId,
                        PaymentModeName = x.PaymentModeName,
                        TransactionCharges = x.TransactionCharges,
                        TransactionType = x.TransactionType
                    }).ToListAsync();

                var hours = await _context.HourMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync();

                return Ok(new { Code = 200, Message = "Data get successfully", bookingno = bookingno, agentDetails = agentDetails, roomCategories = roomCategories, paymentModes = paymentModes, hours = hours });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("CheckReservationNoExists")]
        public async Task<IActionResult> CheckReservationNoExists(string reservationNo = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reservationNo))
                {
                    return Ok(new { Code = 400, Message = "Reservation No not found" });
                }
                var isExists = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.ReservationNo == reservationNo && x.CompanyId == companyId);
                if (isExists == null)
                {
                    return Ok(new { Code = 200, Message = "Reservation no not exists" });
                }
                else
                {
                    var getbookingno = await DocumentHelper.GetDocumentNo(_context, Constants.Constants.DocumentReservation, companyId, financialYear);
                    return Ok(new { Code = 409, Message = "Reservation no already exists" , data  = getbookingno });
                }
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("ReservationRoomRate")]
        public async Task<IActionResult> ReservationRoomRate(int roomTypeId, DateOnly checkInDate, DateOnly checkOutDate, int noOfRooms, int noOfNights, string gstType, int noOfHours = 0, string checkInTime = "", string checkOutTime = "", string discountType = "", decimal discount = 0, string checkOutFormat = "", string calculateRoomRates = "")
        {
            try
            {
                var roomRateResponse = new RoomRateResponse();
                //find property details
                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if (property == null)
                {
                    return Ok(new { Code = 400, Message = "Property not found" });
                }

               
                          
                var (code, message, response) = await CalculateRoomRateAsync(companyId, roomTypeId, checkInDate, checkOutDate,  noOfRooms, noOfNights, gstType, noOfHours, checkInTime, checkOutTime, discountType, discount, checkOutFormat == "" ? property.CheckOutFormat : checkOutFormat, calculateRoomRates == "" ? property.CalculateRoomRates : calculateRoomRates, property);
                return Ok(new { Code = code, Message = message, Data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

       
        [HttpGet("CalculateAgentCommision")]
        public async Task<IActionResult> CalculateAgentCommision(int agentId, decimal bookingAmount, decimal totalAmountWithGst)
        {
            try
            {
                var agentCommissionResponse = new AgentCommissionResponse();
                var (code, message, response) = await CalculateAgentCommisionAsync(agentId, bookingAmount, totalAmountWithGst);
                return Ok(new { Code = code, Message = message, Data = response });
               

            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("SaveReservation")]
        public async Task<IActionResult> SaveReservation([FromBody] ReservationRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var currentDate = DateTime.Now;
            try
            {
                if (request == null || request.GuestDetailsDTO == null || request.BookingDetailsDTO == null || request.BookingDetailsDTO.Count == 0 || request.PaymentDetailsDTO == null || request.ReservationDetailsDTO == null || request.AgentPaymentDetailsDTO == null)
                {
                    return Ok(new { code = 400, message = "Invalid data.", data = new object { } });
                }
                if (string.IsNullOrWhiteSpace(request.GuestDetailsDTO.GuestName) || string.IsNullOrWhiteSpace(request.GuestDetailsDTO.PhoneNumber))
                {
                    return Ok(new { code = 400, message = "Guest name and phone number is required.", data = new object { } });

                }

                //CHECK RESERVATION NO ALREADY USED
                var updatedBookingNo = await DocumentHelper.GetDocumentNo(_context, Constants.Constants.DocumentReservation, companyId, financialYear);

                if(updatedBookingNo != request.ReservationDetailsDTO.ReservationNo)
                {

                }


                //CHECK IF CHECKIN ROOMS SHOULD BE ASSIGNED
                if (request.ReservationDetailsDTO.IsCheckIn == true)
                {
                    foreach (var item in request.BookingDetailsDTO)
                    {
                        bool allRoomIdsValid = item.AssignedRooms.All(room => room.RoomId > 0);
                        if(allRoomIdsValid == false)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = "All selected rooms must be assigned before check-in" });
                        }
                    }
                }
                

                //CHECK ROOM ALREADY BOOKING OR NOT
                foreach (var item in request.BookingDetailsDTO)
                {
                    bool allRoomIdsAreZero = item.AssignedRooms.All(r => r.RoomId == 0);
                    if (allRoomIdsAreZero == true)
                    {
                        DataSet dataSet = await GetRoomAvailability(item.CheckInDate, item.CheckInTime, item.CheckOutDate, item.CheckOutTime, "cleanroom", item.RoomTypeId, 0);
                        if (dataSet == null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                        }
                        else
                        {
                            //Get available rooms on this category
                            int AvailableRooms = 0;
                            var dataTable2 = dataSet.Tables[1];
                            foreach (DataRow row in dataTable2.Rows)
                            {

                                foreach (DataColumn col in dataTable2.Columns)
                                {
                                    AvailableRooms = Convert.ToInt32(row[col]);
                                }

                            }

                            if (AvailableRooms < item.NoOfRooms)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 400, Message = "Required No of rooms are not available for " + item.RoomCategoryName });
                            }
                        }
                    }
                    else
                    {
                        foreach (var room in item.AssignedRooms)
                        {
                            //room is assigned
                            DataSet dataSet = await GetRoomAvailability(item.CheckInDate, item.CheckInTime, item.CheckOutDate, item.CheckOutTime, "checkbyroomid", item.RoomTypeId, room.RoomId);
                            if (dataSet == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                            }
                            //Check room is available or not
                            var rows = new List<Dictionary<string, object>>();

                            var dataTable = dataSet.Tables[0];
                            foreach (DataRow row in dataTable.Rows)
                            {
                                var dict = new Dictionary<string, object>();
                                foreach (DataColumn col in dataTable.Columns)
                                {
                                    dict[col.ColumnName] = row[col];
                                }
                                rows.Add(dict);
                            }
                            if (rows.Count == 0)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 400, Message = "Room not found" });
                            }
                            else
                            {
                                if (rows[0]["roomStatus"].ToString() != Constants.Constants.Clean)
                                {
                                    await transaction.RollbackAsync();
                                    return Ok(new { code = 400, message = "Room " + rows[0]["RoomNo"] + " is already reserved with Reservation No " + rows[0]["ReservationNo"], data = new object { } });
                                }
                            }


                        }
                    }


                }

               

                //Insert Guest Details
                var guest = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == request.GuestDetailsDTO.GuestId);

                if (guest == null)
                {
                    guest = _mapper.Map<GuestDetails>(request.GuestDetailsDTO);
                    Constants.Constants.SetMastersDefault(guest, companyId, userId, currentDate);
                   
                    await _context.GuestDetails.AddAsync(guest);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _mapper.Map(request.GuestDetailsDTO, guest);
                  
                    guest.UpdatedDate = currentDate;
                    _context.GuestDetails.Update(guest);
                    await _context.SaveChangesAsync();
                }

               
                

                //save booking
                int roomCount = 1;
                
                List<BookingDetail> bookings = new List<BookingDetail>();
                foreach (var item in request.BookingDetailsDTO)
                {
                    foreach (var room in item.AssignedRooms)
                    {
                        var bookingDetails = _mapper.Map<BookingDetail>(item);
                        Constants.Constants.SetMastersDefault(bookingDetails, companyId, userId, currentDate);


                        bookingDetails.CheckInDateTime = DateTimeMethod.ConvertToDateTime(bookingDetails.CheckInDate, bookingDetails.CheckInTime);
                        bookingDetails.CheckOutDateTime = DateTimeMethod.ConvertToDateTime(bookingDetails.CheckOutDate, bookingDetails.CheckOutTime);
                        bookingDetails.ReservationDate = bookingDetails.CheckInDate;
                        bookingDetails.ReservationTime = bookingDetails.CheckInTime;
                        bookingDetails.ReservationDateTime = bookingDetails.CheckInDateTime;
                        bookingDetails.InitialCheckOutDate = bookingDetails.CheckOutDate;
                        bookingDetails.InitialCheckOutTime = bookingDetails.CheckOutTime;
                        bookingDetails.InitialCheckOutDateTime = bookingDetails.CheckOutDateTime;
                        bookingDetails.Status = request.ReservationDetailsDTO.IsCheckIn == true ? Constants.Constants.CheckIn : bookingDetails.Status;
                        bookingDetails.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                        bookingDetails.BookingDate = currentDate;
                        bookingDetails.PrimaryGuestId = guest.GuestId;
                        bookingDetails.GuestId = guest.GuestId;
                        bookingDetails.RoomId = room.RoomId;
                        bookingDetails.RoomCount = request.BookingDetailsDTO.Count == 1 && item.AssignedRooms.Count == 1 ? 0 : roomCount;
                        bookingDetails.BookingSource = request.ReservationDetailsDTO.BookingSource;
                        bookingDetails.TotalAmount = BookingCalulation.BookingTotalAmount(bookingDetails);
                        bookingDetails.TotalAmountWithOutDiscount = bookingDetails.TotalAmount;
                        roomCount++;

                        await _context.BookingDetail.AddAsync(bookingDetails);
                        await _context.SaveChangesAsync();

                        bookings.Add(bookingDetails);

                        //room availability
                        RoomAvailability roomAvailability = new RoomAvailability();
                        Constants.Constants.SetMastersDefault(roomAvailability, companyId, userId, currentDate);
                        roomAvailability.CheckInDate = bookingDetails.CheckInDate;
                        roomAvailability.CheckOutDate = bookingDetails.CheckOutDate;
                        roomAvailability.CheckInTime = bookingDetails.CheckInTime;
                        roomAvailability.CheckOutTime = bookingDetails.CheckOutTime;
                        roomAvailability.CheckInDateTime = bookingDetails.CheckInDateTime;
                        roomAvailability.CheckOutDateTime = bookingDetails.CheckOutDateTime;
                        roomAvailability.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                        roomAvailability.BookingId = bookingDetails.BookingId;
                        roomAvailability.RoomId = room.RoomId;
                        roomAvailability.RoomStatus = bookingDetails.Status;
                        roomAvailability.RoomTypeId = bookingDetails.RoomTypeId;
                        await _context.RoomAvailability.AddAsync(roomAvailability);
                        

                        //room rates
                        foreach (var rates in room.roomRates)
                        {
                            rates.BookingId = bookingDetails.BookingId;
                            rates.RoomId = room.RoomId;
                            
                            rates.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                            Constants.Constants.SetMastersDefault(rates, companyId, userId, currentDate);

                            await _context.BookedRoomRates.AddAsync(rates);

                        }


                    }


                }

                decimal totalTransactionAmout = 0;
                //paid to agent
                if (request.AgentPaymentDetailsDTO.PaymentAmount > 0)
                {
                    var paymentDetails = _mapper.Map<PaymentDetails>(request.AgentPaymentDetailsDTO);
                    Constants.Constants.SetMastersDefault(paymentDetails, companyId, userId, currentDate);
                    paymentDetails.BookingId = 0;
                    paymentDetails.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                    paymentDetails.PaymentLeft = request.PaymentDetailsDTO.PaymentAmount;
                    await _context.PaymentDetails.AddAsync(paymentDetails);
                    
                }

                //advance payment
                if (request.PaymentDetailsDTO.PaymentAmount > 0)
                {
                    var paymentDetails = _mapper.Map<PaymentDetails>(request.PaymentDetailsDTO);
                    Constants.Constants.SetMastersDefault(paymentDetails, companyId, userId, currentDate);
                    paymentDetails.BookingId = 0;
                    if(paymentDetails.TransactionCharges > 0)
                    {
                        if (paymentDetails.TransactionType == Constants.Constants.DeductionByAmount)
                        {
                            paymentDetails.TransactionAmount = paymentDetails.TransactionCharges;
                        }
                        else
                        {
                            paymentDetails.TransactionAmount = Constants.Calculation.CalculatePercentage(paymentDetails.PaymentAmount, paymentDetails.TransactionCharges);
                        }
                        totalTransactionAmout = paymentDetails.TransactionAmount;
                    }
                    paymentDetails.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                    paymentDetails.PaymentLeft = request.PaymentDetailsDTO.PaymentAmount - paymentDetails.TransactionAmount;
                    await _context.PaymentDetails.AddAsync(paymentDetails);
                    
                }

                //save reservation details
                var reservationDetails = _mapper.Map<ReservationDetails>(request.ReservationDetailsDTO);
                reservationDetails.PrimaryGuestId = guest.GuestId;
                Constants.Constants.SetMastersDefault(reservationDetails, companyId, userId, currentDate);

                //(reservationDetails.TotalRoomPayment, reservationDetails.TotalGst, reservationDetails.TotalAmount) = BookingCalulation.ReservationRoomsTotal(request.BookingDetailsDTO);

                //agent service charge
                if (reservationDetails.AgentId > 0)
                {
                    var gstPercentage = await GetGstPercetage(Constants.Constants.Agent);
                    if (gstPercentage == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 500, Message = "Gst percentage not found for agent" });
                    }
                    else
                    {
                        if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                        {
                            var gstRangeMaster = GetApplicableGstRange(gstPercentage.ranges, (Calculation.RoundOffDecimal(reservationDetails.AgentServiceCharge)));
                            if (gstRangeMaster == null)
                            {
                                reservationDetails.AgentServiceGstPercentage = gstPercentage.TaxPercentage;
                            }
                            else
                            {
                                reservationDetails.AgentServiceGstPercentage = gstRangeMaster.TaxPercentage;
                            }
                        }
                        else
                        {
                            reservationDetails.AgentServiceGstPercentage = gstPercentage.TaxPercentage;
                        }

                        (reservationDetails.AgentServiceCharge, reservationDetails.AgentServiceGstAmount) = Calculation.CalculateGst(Calculation.RoundOffDecimal(reservationDetails.AgentServiceCharge), reservationDetails.AgentServiceGstPercentage, reservationDetails.AgentServiceGstType);

                        reservationDetails.AgentTotalServiceCharge = Calculation.RoundOffDecimal(reservationDetails.AgentServiceCharge + reservationDetails.AgentServiceGstAmount);
                    }
                }
                reservationDetails.RoomsCount = roomCount - 1;
                reservationDetails.TransactionAmount = totalTransactionAmout;
                await _context.ReservationDetails.AddAsync(reservationDetails);

                var response = await DocumentHelper.UpdateDocumentNo(_context,Constants.Constants.Reservation,companyId, financialYear);
                if (response == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if (property == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }

                //send email
                if (property.IsEmailNotification && property.ReservationNotification)
                {
                    ReservationEmailNotification emailNotification = new ReservationEmailNotification(_context, property, request.ReservationDetailsDTO.ReservationNo, roomCount - 1, guest, companyId);
                    await emailNotification.SendEmail();
                }

                //if (property.IsWhatsappNotification && property.ReservationNotification)
                //{
                //    ReservationWhatsAppNotification whatsAppNotification = new ReservationWhatsAppNotification(_context, property, guest, companyId, bookings);
                //    await whatsAppNotification.SendWhatsAppNotification();
                //}


                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Reservation created successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("ApproveReservation")]
        public async Task<IActionResult> ApproveReservation(string status, string idType, int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (idType == Constants.Constants.Booking)
                {
                    var bookings = await _context.BookingDetail.FirstOrDefaultAsync(x => x.BookingId == id && x.CompanyId == companyId && x.IsActive);
                    if (bookings == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 404, Message = "Data Not Found" });
                    }
                    var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.BookingId == bookings.BookingId && x.CompanyId == companyId && x.IsActive);
                    if (roomAvailability == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 404, Message = "Data Not Found" });
                    }

                    if (status == Constants.Constants.Confirmed)
                    {
                        bookings.Status = Constants.Constants.Confirmed;
                        roomAvailability.RoomStatus = Constants.Constants.Confirmed;
                    }
                    else if (status == Constants.Constants.Rejected)
                    {
                        bookings.IsActive = false;
                        bookings.Status = Constants.Constants.Rejected;

                        _context.RoomAvailability.Remove(roomAvailability);

                        var bookedRoomRate = await _context.BookedRoomRates
                            .Where(x => x.BookingId == bookings.BookingId && x.CompanyId == companyId && x.IsActive)
                            .ToListAsync();
                        if (bookedRoomRate == null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 404, Message = "Data Not Found" });
                        }
                        _context.BookedRoomRates.RemoveRange(bookedRoomRate);

                        var paymentDetails = await _context.PaymentDetails
                            .Where(x => x.BookingId == bookings.BookingId && x.CompanyId == companyId && x.IsActive)
                            .ToListAsync();
                        if (paymentDetails == null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 404, Message = "Data Not Found" });
                        }
                        foreach (var pd in paymentDetails)
                        {
                            pd.IsActive = false;
                            _context.PaymentDetails.Update(pd);
                        }

                    }
                }
                else
                {
                    var reservation = await _context.ReservationDetails.FirstOrDefaultAsync(d=> d.ReservationId == id && d.CompanyId == companyId && d.IsActive);
                    if (reservation == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 404, Message = "Data Not Found" });
                    }

                   

                    var booking = await _context.BookingDetail
                        .Where(x => x.ReservationNo == reservation.ReservationNo && x.CompanyId == companyId && x.IsActive)
                        .ToListAsync();
                    if (booking.Count == 0)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 404, Message = "Data Not Found" });
                    }
                    if (status == Constants.Constants.Confirmed)
                    {
                        foreach (var item in booking)
                        {
                            item.Status = Constants.Constants.Confirmed;

                            var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.BookingId == item.BookingId && x.CompanyId == companyId && x.IsActive);
                            if (roomAvailability == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 404, Message = "Data Not Found" });
                            }
                            roomAvailability.RoomStatus = Constants.Constants.Confirmed;
                        }
                    }
                    else if (status == Constants.Constants.Rejected)
                    {
                        reservation.IsActive = false;

                        foreach (var item in booking)
                        {
                            item.Status = Constants.Constants.Rejected;
                            item.IsActive = false;

                            var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync( x => x.BookingId == item.BookingId && x.CompanyId == companyId && x.IsActive);
                            if (roomAvailability == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 404, Message = "Data Not Found" });
                            }

                            _context.RoomAvailability.Remove(roomAvailability);

                            var bookedRoomRate = await _context.BookedRoomRates
                                .Where(x => x.BookingId == item.BookingId && x.CompanyId == companyId && x.IsActive)
                                .ToListAsync();
                            if (bookedRoomRate == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 404, Message = "Data Not Found" });
                            }
                            _context.BookedRoomRates.RemoveRange(bookedRoomRate);

                            var paymentDetails = await _context.PaymentDetails
                                .Where(x => x.BookingId == item.BookingId && x.CompanyId == companyId && x.IsActive)
                                .ToListAsync();
                            if (paymentDetails == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 404, Message = "Data Not Found" });
                            }
                            foreach (var pd in paymentDetails)
                            {
                                pd.IsActive = false;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Code = 200, Message = $"{idType} {status} Successfully" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }



        [HttpGet("GetGuestList")]
        public async Task<IActionResult> GetGuestList(string status = "", string pageName = "")
        {
            try
            {
                var statusList = new List<string> { Constants.Constants.Pending, Constants.Constants.Confirmed, Constants.Constants.CheckIn };
                if (pageName == "addPayment")
                {
                    var bookings = await (from booking in _context.BookingDetail
                                          join guest in _context.GuestDetails on booking.PrimaryGuestId equals guest.GuestId
                                          join rguest in _context.GuestDetails on booking.GuestId equals rguest.GuestId into roomguest
                                          from bookingguest in roomguest.DefaultIfEmpty()
                                          join r in _context.RoomMaster on new { booking.RoomId, CompanyId = companyId }
                                            equals new { RoomId = r.RoomId, r.CompanyId } into rooms
                                          from room in rooms.DefaultIfEmpty()
                                          where booking.CompanyId == companyId && booking.IsActive == true && guest.CompanyId == companyId

                                          && statusList.Contains(booking.Status)
                                          select new
                                          {
                                              ReservationNo = booking.ReservationNo,
                                              BookingId = booking.BookingId,
                                              RoomGuestId = bookingguest != null ? bookingguest.GuestId : 0,
                                              GuestId = booking.PrimaryGuestId,
                                              PrimaryGuestName = guest.GuestName,
                                              GuestName = bookingguest != null ? bookingguest.GuestName : guest.GuestName,
                                              RoomId = booking.RoomId,
                                              RoomNo = room != null ? room.RoomNo : "",
                                              ReservationName = room != null
                                            ? $"{room.RoomNo} : {booking.ReservationNo}" +
                                              (booking.RoomCount > 0 ? $"-{booking.RoomCount}" : "") +
                                              $" : {(bookingguest != null ? bookingguest.GuestName : guest.GuestName)}"
                                            : $"{booking.ReservationNo}" +
                                              (booking.RoomCount > 0 ? $"-{booking.RoomCount}" : "") +
                                              $" : {(bookingguest != null ? bookingguest.GuestName : guest.GuestName)}"
                                          }).ToListAsync();
                    return Ok(new { Code = 200, Message = "Data fetch successfully", data = bookings });
                }

                else if (pageName == "cancelBooking")
                {
                    var bookings = await (from booking in _context.BookingDetail
                                          join guest in _context.GuestDetails on booking.PrimaryGuestId equals guest.GuestId
                                          join rguest in _context.GuestDetails on booking.GuestId equals rguest.GuestId into roomguest
                                          from bookingguest in roomguest.DefaultIfEmpty()
                                          join r in _context.RoomMaster on new { booking.RoomId, CompanyId = companyId }
                                            equals new { RoomId = r.RoomId, r.CompanyId } into rooms
                                          from room in rooms.DefaultIfEmpty()
                                          where booking.CompanyId == companyId && booking.IsActive == true && guest.CompanyId == companyId

                                          && statusList.Contains(booking.Status) && booking.TotalServicesAmount == 0
                                          select new
                                          {
                                              ReservationNo = booking.ReservationNo,
                                              BookingId = booking.BookingId,
                                              RoomGuestId = bookingguest != null ? bookingguest.GuestId : 0,
                                              GuestId = booking.PrimaryGuestId,
                                              PrimaryGuestName = guest.GuestName,
                                              GuestName = bookingguest != null ? bookingguest.GuestName : guest.GuestName,
                                              RoomId = booking.RoomId,
                                              RoomNo = room != null ? room.RoomNo : "",
                                              ReservationName = room != null
                                            ? $"{room.RoomNo} : {booking.ReservationNo}" +
                                              (booking.RoomCount > 0 ? $"-{booking.RoomCount}" : "") +
                                              $" : {(bookingguest != null ? bookingguest.GuestName : guest.GuestName)}"
                                            : $"{booking.ReservationNo}" +
                                              (booking.RoomCount > 0 ? $"-{booking.RoomCount}" : "") +
                                              $" : {(bookingguest != null ? bookingguest.GuestName : guest.GuestName)}"
                                          }).ToListAsync();
                    return Ok(new { Code = 200, Message = "Data fetch successfully", data = bookings });
                }


                else
                {
                    if (string.IsNullOrEmpty(status))
                    {
                        return Ok(new { Code = 200, Message = "Data fetch successfully", data = new List<object>() });
                    }
                    var bookings = await (from booking in _context.BookingDetail
                                          join guest in _context.GuestDetails on booking.PrimaryGuestId equals guest.GuestId
                                          join rguest in _context.GuestDetails on booking.GuestId equals rguest.GuestId into roomguest
                                          from bookingguest in roomguest.DefaultIfEmpty()
                                          join r in _context.RoomMaster on new { booking.RoomId, CompanyId = companyId }
                                            equals new { RoomId = r.RoomId, r.CompanyId } into rooms
                                          from room in rooms.DefaultIfEmpty()
                                          where booking.CompanyId == companyId && booking.IsActive == true && guest.CompanyId == companyId

                                          && booking.Status == status
                                          select new
                                          {
                                              ReservationNo = booking.ReservationNo,
                                              BookingId = booking.BookingId,
                                              RoomGuestId = bookingguest != null ? bookingguest.GuestId : 0,
                                              GuestId = booking.PrimaryGuestId,
                                              PrimaryGuestName = guest.GuestName,
                                              GuestName = bookingguest != null ? bookingguest.GuestName : guest.GuestName,
                                              RoomId = booking.RoomId,
                                              RoomNo = room != null ? room.RoomNo : "",
                                              ReservationName = room != null
                                            ? $"{room.RoomNo} : {booking.ReservationNo}" +
                                              (booking.RoomCount > 0 ? $"-{booking.RoomCount}" : "") +
                                              $" : {(bookingguest != null ? bookingguest.GuestName : guest.GuestName)}"
                                            : $"{booking.ReservationNo}" +
                                              (booking.RoomCount > 0 ? $"-{booking.RoomCount}" : "") +
                                              $" : {(bookingguest != null ? bookingguest.GuestName : guest.GuestName)}"
                                          }).ToListAsync();

                    return Ok(new { Code = 200, Message = "Data fetch successfully", data = bookings });
                }

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetBookingsOnCheckIn")]
        public async Task<IActionResult> GetBookingsOnCheckIn(string reservationNo, int guestId)
        {
            try
            {
                List<string> statusList = new List<string> { Constants.Constants.Pending, Constants.Constants.Confirmed, Constants.Constants.CheckIn };
                var checkInResponse = new CheckInResponse();
                if (string.IsNullOrWhiteSpace(reservationNo) || guestId == 0)
                {
                    return Ok(new { Code = 500, Message = "Invalid data" });
                }
                
                //Get reservation details
                checkInResponse.ReservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);
                if (checkInResponse.ReservationDetails == null)
                {
                    return Ok(new { Code = 400, Message = $"No details for {reservationNo} reservation" });
                }

                checkInResponse.GuestDetails = await _context.GuestDetails.Where(x => x.CompanyId == companyId && x.IsActive && x.GuestId == guestId).FirstOrDefaultAsync();
                if (checkInResponse.GuestDetails == null)
                {
                    return Ok(new { Code = 400, Message = $"No details for {reservationNo} reservation" });
                }

                var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo).ToListAsync();

                
                checkInResponse.IsSingleRoom = await _context.BookingDetail.CountAsync(x => x.ReservationNo == reservationNo && x.CompanyId == companyId && x.IsActive == true) == 1 ? true : false;

                checkInResponse.BookingDetails = await (
                                        from booking in _context.BookingDetail
                                        join room in _context.RoomMaster
                                            on new { RoomId = booking.RoomId, CompanyId = companyId }
                                            equals new { RoomId = room.RoomId, CompanyId = room.CompanyId } into rooms
                                        from bookrooms in rooms.DefaultIfEmpty()
                                        join category in _context.RoomCategoryMaster
                                            on new { RoomTypeId = booking.RoomTypeId, CompanyId = companyId }
                                            equals new { RoomTypeId = category.Id, CompanyId = category.CompanyId }

                                        where booking.IsActive == true
                                            && booking.CompanyId == companyId
                                            && booking.ReservationNo == reservationNo && statusList.Contains(booking.Status)
                                        select new BookingDetail
                                        {
                                            BookingId = booking.BookingId,
                                            GuestId = booking.GuestId,
                                            RoomId = booking.RoomId,
                                            RoomTypeId = booking.RoomTypeId,
                                            CheckInDate = booking.CheckInDate,
                                            CheckInTime = booking.CheckInTime,
                                            CheckOutDate = booking.CheckOutDate,
                                            CheckOutTime = booking.CheckOutTime,
                                            CheckInDateTime = booking.CheckInDateTime,
                                            CheckOutDateTime = booking.CheckOutDateTime,
                                            CheckoutFormat = booking.CheckoutFormat,
                                            NoOfNights = booking.NoOfNights,
                                            NoOfHours = booking.NoOfHours,
                                            RoomCount = booking.RoomCount,
                                            Pax = booking.Pax,
                                            Status = booking.Status,
                                            Remarks = booking.Remarks,
                                            ReservationNo = booking.ReservationNo,
                                            BookingDate = booking.BookingDate,
                                            CreatedDate = booking.CreatedDate,
                                            UpdatedDate = booking.UpdatedDate,
                                            IsActive = booking.IsActive,
                                            PrimaryGuestId = booking.PrimaryGuestId,
                                            InitialBalanceAmount = booking.InitialBalanceAmount,
                                            BalanceAmount = booking.BalanceAmount,
                                            AdvanceAmount = booking.AdvanceAmount,
                                            ReceivedAmount = booking.ReceivedAmount,
                                            AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                            RefundAmount = booking.RefundAmount,
                                            InvoiceDate = booking.InvoiceDate,
                                            InvoiceNo = booking.InvoiceNo,
                                            UserId = booking.UserId,
                                            GstType = booking.GstType,
                                            CompanyId = booking.CompanyId,
                                            BookingAmount = booking.BookingAmount,
                                            GstAmount = booking.GstAmount,
                                            TotalBookingAmount = booking.TotalBookingAmount,
                                            BookingSource = booking.BookingSource,
                                            ReservationDate = booking.ReservationDate,
                                            ReservationTime = booking.ReservationTime,
                                            ReservationDateTime = booking.ReservationDateTime,
                                            InitialCheckOutDate = booking.InitialCheckOutDate,
                                            InitialCheckOutTime = booking.InitialCheckOutTime,
                                            InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                            ServicesAmount = booking.ServicesAmount,
                                            TotalAmount = booking.TotalAmount,
                                            TotalAmountWithOutDiscount = booking.TotalAmountWithOutDiscount,
                                            AgentAdvanceAmount = booking.AgentAdvanceAmount,
                                            InvoiceName = booking.InvoiceName,
                                            BillTo = booking.BillTo,
                                            TotalServicesAmount = booking.TotalServicesAmount,
                                            ServicesTaxAmount = booking.ServicesTaxAmount,
                                            CancelAmount = booking.CancelAmount,
                                            CancelDate = booking.CancelDate,
                                            CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                            IsEarlyCheckIn = booking.IsEarlyCheckIn,
                                            EarlyCheckInPolicyName = booking.EarlyCheckInPolicyName,
                                            EarlyCheckInDeductionBy = booking.EarlyCheckInDeductionBy,
                                            EarlyCheckInApplicableOn = booking.EarlyCheckInApplicableOn,
                                            EarlyCheckInFromHour = booking.EarlyCheckInFromHour,
                                            EarlyCheckInToHour = booking.EarlyCheckInToHour,
                                            EarlyCheckInCharges = booking.EarlyCheckInCharges,
                                            IsLateCheckOut = booking.IsLateCheckOut,
                                            LateCheckOutPolicyName = booking.LateCheckOutPolicyName,
                                            LateCheckOutDeductionBy = booking.LateCheckOutDeductionBy,
                                            LateCheckOutApplicableOn = booking.LateCheckOutApplicableOn,
                                            LateCheckOutFromHour = booking.LateCheckOutFromHour,
                                            LateCheckOutToHour = booking.LateCheckOutToHour,
                                            LateCheckOutCharges = booking.LateCheckOutCharges,
                                            DiscountType = booking.DiscountType,
                                            DiscountPercentage = booking.DiscountPercentage,
                                            DiscountAmount = booking.DiscountAmount,
                                            DiscountTotalAmount = booking.DiscountTotalAmount,
                                            BookingAmountWithoutDiscount = booking.BookingAmountWithoutDiscount,
                                            CalculateRoomRates = booking.CalculateRoomRates,
                                            IsSelectedValue = false,
                                            // NotMapped fields
                                            RoomTypeName = category.Type == null ? "" : category.Type,
                                            RoomNo = bookrooms.RoomNo == null ? "" : bookrooms.RoomNo
                                        } // project the entity to map later
                                    ).ToListAsync();


                foreach (var item in checkInResponse.BookingDetails)
                {
                    item.BookedRoomRates = roomRates.Where(x => x.BookingId == item.BookingId).ToList();
                    item.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == item.GuestId) ?? new GuestDetails();
                }



                //payment details
                checkInResponse.PaymentDetails = await (from x in _context.PaymentDetails
                                                        join room in _context.RoomMaster on x.RoomId equals room.RoomId into roomT
                                                        from rm in roomT.DefaultIfEmpty()
                                                        where x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo
                                                        orderby x.IsReceived
                                                        select new PaymentDetails
                                                        {
                                                            PaymentId = x.PaymentId,
                                                            BookingId = x.BookingId,
                                                            ReservationNo = x.ReservationNo,
                                                            PaymentDate = x.PaymentDate,
                                                            PaymentMethod = x.PaymentMethod,
                                                            TransactionId = x.TransactionId,
                                                            PaymentStatus = x.PaymentStatus,
                                                            PaymentType = x.PaymentType,
                                                            BankName = x.BankName,
                                                            PaymentReferenceNo = x.PaymentReferenceNo,
                                                            PaidBy = x.PaidBy,
                                                            Remarks = x.Remarks,
                                                            Other1 = x.Other1,
                                                            Other2 = x.Other2,
                                                            IsActive = x.IsActive,
                                                            IsReceived = x.IsReceived,
                                                            RoomId = x.RoomId,
                                                            UserId = x.UserId,
                                                            PaymentFormat = x.PaymentFormat,
                                                            RefundAmount = x.RefundAmount,
                                                            PaymentAmount = x.PaymentAmount,
                                                            PaymentLeft = x.PaymentLeft,
                                                            CreatedDate = x.CreatedDate,
                                                            UpdatedDate = x.UpdatedDate,
                                                            CompanyId = x.CompanyId,
                                                            RoomNo = rm != null ? rm.RoomNo : "",
                                                            TransactionAmount = x.TransactionAmount,
                                                            TransactionType = x.TransactionType,
                                                            TransactionCharges = x.TransactionCharges,
                                                            IsEditable = x.PaymentAmount != x.PaymentLeft ? false : true
                                                        }).ToListAsync();



                //payment summary
                var paymentSummary = CalculateCheckInSummary(checkInResponse.ReservationDetails, checkInResponse.BookingDetails, checkInResponse.PaymentDetails);
               

                checkInResponse.PaymentSummary = paymentSummary;



                return Ok(new { Code = 200, Message = "Data fetched successfully", data = checkInResponse });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("UpdateRoomDetail")]
        public async Task<IActionResult> UpdateRoomDetail([FromBody] List<BookingDetail> bookingList, string reservationNo)
        {

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                if (bookingList.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "No data found" });
                }

                var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);
                if (reservationDetails == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 500, Message = "Invalid reservation" });
                }

                foreach (var item in bookingList)
                {
                    //guest details
                    if (item.GuestDetails.GuestId == 0)
                    {
                        if(item.GuestDetails.GuestName != "")
                        {
                            Constants.Constants.SetMastersDefault(item.GuestDetails, companyId, userId, currentDate);
                            await _context.GuestDetails.AddAsync(item.GuestDetails);
                            await _context.SaveChangesAsync();
                        }
                       
                    }
                    else
                    {
                        var guestdetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.GuestId == item.GuestDetails.GuestId);
                        if (guestdetails == null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = "No guest found" });
                        }
                        else
                        {
                            guestdetails.GuestName = item.GuestDetails.GuestName;
                            guestdetails.Nationality = item.GuestDetails.Nationality;
                            guestdetails.CountryId = item.GuestDetails.CountryId;
                            guestdetails.IdType = item.GuestDetails.IdType;
                            guestdetails.IdNumber = item.GuestDetails.IdNumber;
                            guestdetails.PhoneNumber = item.GuestDetails.PhoneNumber;
                            guestdetails.Email = item.GuestDetails.Email;
                            guestdetails.Gender = item.GuestDetails.Gender;
                            _context.GuestDetails.Update(guestdetails);
                            await _context.SaveChangesAsync();
                        }
                    }

                    //  booked room rate
                    var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();

                    if (roomRates.Count > 0)
                    {
                        _context.BookedRoomRates.RemoveRange(roomRates);
                        await _context.SaveChangesAsync();
                    }



                    // create new room
                    if (item.BookingId == 0)
                    {
                        var bookingDetails = _mapper.Map<BookingDetail>(item);
                        Constants.Constants.SetMastersDefault(bookingDetails, companyId, userId, currentDate);

                        bookingDetails.CheckInDateTime = DateTime.ParseExact((bookingDetails.CheckInDate.ToString("yyyy-MM-dd")) + " " + bookingDetails.CheckInTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        bookingDetails.CheckOutDateTime = DateTime.ParseExact((bookingDetails.CheckOutDate.ToString("yyyy-MM-dd")) + " " + bookingDetails.CheckOutTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        bookingDetails.ReservationDate = bookingDetails.CheckInDate;
                        bookingDetails.ReservationTime = bookingDetails.CheckInTime;
                        bookingDetails.ReservationDateTime = bookingDetails.CheckInDateTime;
                        bookingDetails.InitialCheckOutDate = bookingDetails.CheckOutDate;
                        bookingDetails.InitialCheckOutTime = bookingDetails.CheckOutTime;
                        bookingDetails.InitialCheckOutDateTime = bookingDetails.CheckOutDateTime;
                        bookingDetails.BookingDate = currentDate;
                        bookingDetails.PrimaryGuestId = reservationDetails.PrimaryGuestId;
                        bookingDetails.GuestId = item.GuestDetails.GuestId;
                        bookingDetails.BookingSource = reservationDetails.BookingSource;
                        bookingDetails.ReservationNo = reservationDetails.ReservationNo;
                        
                        bookingDetails.TotalAmount = BookingCalulation.BookingTotalAmount(bookingDetails);
                        bookingDetails.TotalAmountWithOutDiscount = bookingDetails.TotalAmount;
                        await _context.BookingDetail.AddAsync(bookingDetails);
                        await _context.SaveChangesAsync();

                        RoomAvailability roomAvailability = new RoomAvailability();
                        Constants.Constants.SetMastersDefault(roomAvailability, companyId, userId, currentDate);
                        roomAvailability.CheckInDate = bookingDetails.ReservationDate;
                        roomAvailability.CheckOutDate = bookingDetails.CheckOutDate;
                        roomAvailability.CheckInTime = bookingDetails.ReservationTime;
                        roomAvailability.CheckOutTime = bookingDetails.CheckOutTime;
                        roomAvailability.CheckInDateTime = bookingDetails.ReservationDateTime;
                        roomAvailability.CheckOutDateTime = bookingDetails.CheckOutDateTime;
                        roomAvailability.ReservationNo = bookingDetails.ReservationNo;
                        roomAvailability.BookingId = bookingDetails.BookingId;
                        roomAvailability.RoomId = bookingDetails.RoomId;
                        roomAvailability.RoomStatus = bookingDetails.Status;
                        roomAvailability.RoomTypeId = bookingDetails.RoomTypeId;
                        await _context.RoomAvailability.AddAsync(roomAvailability);
                        await _context.SaveChangesAsync();

                        foreach (var rates in item.BookedRoomRates)
                        {
                            rates.BookingId = bookingDetails.BookingId;
                            rates.RoomId = bookingDetails.RoomId;
                            rates.ReservationNo = reservationNo;
                            Constants.Constants.SetMastersDefault(rates, companyId, userId, currentDate);

                            await _context.BookedRoomRates.AddAsync(rates);
                            

                        }
                        await _context.SaveChangesAsync();


                        reservationDetails.RoomsCount = reservationDetails.RoomsCount + 1;

                        
                    }
                    else
                    {
                        var bookingDetails = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId);
                        if (bookingDetails == null)
                        {
                            return Ok(new { Code = 400, Message = "Booking not found" });
                        }

                        bookingDetails.ReservationDate = item.ReservationDate;
                        bookingDetails.ReservationTime = item.ReservationTime;
                        bookingDetails.ReservationDateTime = DateTime.ParseExact((bookingDetails.ReservationDate.ToString("yyyy-MM-dd")) + " " + bookingDetails.ReservationTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        bookingDetails.CheckInDate = item.CheckInDate;
                        bookingDetails.CheckInTime = item.CheckInTime;
                        bookingDetails.CheckInDateTime = DateTime.ParseExact((bookingDetails.CheckInDate.ToString("yyyy-MM-dd")) + " " + bookingDetails.CheckInTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        bookingDetails.CheckOutDate = item.CheckOutDate;
                        bookingDetails.CheckOutTime = item.CheckOutTime;
                        bookingDetails.CheckOutDateTime = DateTime.ParseExact((bookingDetails.CheckOutDate.ToString("yyyy-MM-dd")) + " " + bookingDetails.CheckOutTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        bookingDetails.InitialCheckOutDate = bookingDetails.CheckOutDate;
                        bookingDetails.InitialCheckOutTime = bookingDetails.CheckOutTime;
                        bookingDetails.InitialCheckOutDateTime = bookingDetails.CheckOutDateTime;
                        bookingDetails.GuestId = item.GuestDetails.GuestId;
                        bookingDetails.RoomTypeId = item.RoomTypeId;
                        bookingDetails.Pax = item.Pax;
                        bookingDetails.NoOfHours = item.NoOfHours;
                        bookingDetails.NoOfNights = item.NoOfNights;
                        bookingDetails.GstType = item.GstType;
                        bookingDetails.RoomId = item.RoomId;

                        bookingDetails.BookingAmountWithoutDiscount = item.BookingAmountWithoutDiscount;
                        bookingDetails.BookingAmount = item.BookingAmount;
                        bookingDetails.GstAmount = item.GstAmount;
                        bookingDetails.TotalBookingAmount = item.TotalBookingAmount;

                        bookingDetails.IsEarlyCheckIn = item.IsEarlyCheckIn;
                        bookingDetails.EarlyCheckInPolicyName = item.EarlyCheckInPolicyName;
                        bookingDetails.EarlyCheckInDeductionBy = item.EarlyCheckInDeductionBy;
                        bookingDetails.EarlyCheckInApplicableOn = item.EarlyCheckInApplicableOn;
                        bookingDetails.EarlyCheckInFromHour = item.EarlyCheckInFromHour;
                        bookingDetails.EarlyCheckInToHour = item.EarlyCheckInToHour;
                        bookingDetails.EarlyCheckInCharges = item.EarlyCheckInCharges;

                        bookingDetails.IsLateCheckOut = item.IsLateCheckOut;
                        bookingDetails.LateCheckOutPolicyName = item.LateCheckOutPolicyName;
                        bookingDetails.LateCheckOutApplicableOn = item.LateCheckOutApplicableOn;
                        bookingDetails.LateCheckOutFromHour = item.LateCheckOutFromHour;
                        bookingDetails.LateCheckOutToHour = item.LateCheckOutToHour;
                        bookingDetails.LateCheckOutCharges = item.LateCheckOutCharges;

                        bookingDetails.DiscountType = item.DiscountType;
                        bookingDetails.DiscountPercentage = item.DiscountPercentage;
                        bookingDetails.DiscountAmount = item.DiscountAmount;
                        bookingDetails.DiscountTotalAmount = item.DiscountTotalAmount;

                        bookingDetails.TotalAmount = BookingCalulation.BookingTotalAmount(bookingDetails);
                        bookingDetails.TotalAmountWithOutDiscount = bookingDetails.TotalAmount;
                        bookingDetails.UpdatedDate = currentDate;
                        
                        
                        _context.BookingDetail.Update(bookingDetails);
                        await _context.SaveChangesAsync();

                        // room avaialability
                        var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId);
                        if (roomAvailability == null)
                        {
                            return Ok(new { Code = 400, Message = "Room availability not found" });
                        }
                        else
                        {
                            roomAvailability.CheckInDate = bookingDetails.ReservationDate;
                            roomAvailability.CheckOutDate = bookingDetails.CheckOutDate;
                            roomAvailability.CheckInTime = bookingDetails.ReservationTime;
                            roomAvailability.CheckOutTime = bookingDetails.CheckOutTime;
                            roomAvailability.CheckInDateTime = bookingDetails.ReservationDateTime;
                            roomAvailability.CheckOutDateTime = bookingDetails.CheckOutDateTime;
                            roomAvailability.RoomStatus = bookingDetails.Status;
                            roomAvailability.RoomTypeId = bookingDetails.RoomTypeId;
                            roomAvailability.UpdatedDate = currentDate;
                            roomAvailability.RoomId = bookingDetails.RoomId;
                            _context.RoomAvailability.Update(roomAvailability);
                            
                        }

                        foreach (var rates in item.BookedRoomRates)
                        {
                            rates.Id = 0;
                            rates.BookingId = bookingDetails.BookingId;
                            rates.RoomId = bookingDetails.RoomId;
                            rates.ReservationNo = reservationNo;
                            Constants.Constants.SetMastersDefault(rates, companyId, userId, currentDate);

                            await _context.BookedRoomRates.AddAsync(rates);
                            

                        }
                        var payments = await _context.PaymentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId && x.PaymentFormat == Constants.Constants.RoomWisePayment).ToListAsync();

                        foreach(var pay in payments)
                        {
                            pay.RoomId = item.RoomId;
                            pay.UpdatedDate = currentDate;
                            _context.PaymentDetails.Update(pay);
                        }
                    }




                }

                List<BookingDetail> details = await _context.BookingDetail.Where(x => x.IsActive && x.CompanyId == companyId && x.ReservationNo == reservationNo).ToListAsync();
                if (details.Count > 1)
                {
                    int roomCount = 1;
                    foreach (var item in details)
                    {
                        item.RoomCount = roomCount;
                        roomCount++;
                        _context.BookingDetail.Update(item);
                        
                    }
                }



                List<BookingDetailDTO> allBookings = details
                                                    .Select(x => _mapper.Map<BookingDetailDTO>(x))
                                                    .ToList();
                decimal bookingAmount = 0;

                decimal bookingAmountWithGst = 0;

                (bookingAmount, decimal gstAmount, bookingAmountWithGst) = BookingCalulation.ReservationRoomsTotal(allBookings);

                if(reservationDetails.AgentId > 0)
                {
                    var (code, message, agentCommisionResponse) = await CalculateAgentCommisionAsync(reservationDetails.AgentId, bookingAmount, bookingAmountWithGst);
                    if(code != 200 && agentCommisionResponse!=null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = message });
                    }
                    else
                    {
                        reservationDetails.AgentGstType = agentCommisionResponse.AgentGstType;
                        reservationDetails.CommissionPercentage = agentCommisionResponse.AgentCommissionPercentage;
                        reservationDetails.CommissionAmount = agentCommisionResponse.AgentCommisionAmount;
                        reservationDetails.Tcs = agentCommisionResponse.TcsPercentage;
                        reservationDetails.TcsAmount = agentCommisionResponse.TcsAmount;
                        reservationDetails.Tds = agentCommisionResponse.TdsPercentage;
                        reservationDetails.TdsAmount = agentCommisionResponse.TdsAmount;
                    }
                }
                

                reservationDetails.UpdatedDate = currentDate;
                _context.ReservationDetails.Update(reservationDetails);

                await _context.SaveChangesAsync();

                

                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Room Updated successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }




        [HttpPost("UpdatePaymentDetail")]
        public async Task<IActionResult> UpdatePaymentDetail([FromBody] PaymentDetails paymentDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reservationdetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.ReservationNo == paymentDetails.ReservationNo);
                if(reservationdetails == null)
                {
                    return Ok(new { Code = 400, Message = "Reservation details not found" });
                }

                decimal transactionCharges = reservationdetails.TransactionAmount;
                var currentDate = DateTime.Now;
                if (paymentDetails.PaymentId > 0)
                {
                    var payment = await _context.PaymentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId && x.PaymentId == paymentDetails.PaymentId).FirstOrDefaultAsync();
                    if (payment == null)
                    {
                        return Ok(new { Code = 400, Message = "Invalid data" });
                    }
                    else
                    {
                        if (paymentDetails.IsActive == false)
                        {
                            payment.IsActive = paymentDetails.IsActive;

                        }
                        else
                        {
                            payment.PaymentDate = paymentDetails.PaymentDate;
                            payment.PaymentMethod = paymentDetails.PaymentMethod;
                            payment.TransactionId = paymentDetails.TransactionId;
                            payment.PaymentStatus = paymentDetails.PaymentStatus;
                            payment.PaymentType = paymentDetails.PaymentType;
                            payment.BankName = paymentDetails.BankName;
                            payment.PaymentReferenceNo = paymentDetails.PaymentReferenceNo;
                            payment.PaidBy = paymentDetails.PaidBy;
                            payment.RoomId = paymentDetails.RoomId;
                            payment.PaymentFormat = paymentDetails.PaymentFormat;
                            payment.RefundAmount = paymentDetails.RefundAmount;
                            payment.PaymentAmount = paymentDetails.PaymentAmount;
                            payment.TransactionType = paymentDetails.TransactionType;
                           

                            transactionCharges = transactionCharges - payment.TransactionAmount;

                            if (paymentDetails.TransactionCharges > 0)
                            {
                                if (paymentDetails.TransactionType == Constants.Constants.DeductionByAmount)
                                {
                                    payment.TransactionAmount = paymentDetails.TransactionCharges;
                                }
                                else
                                {
                                    payment.TransactionAmount = Constants.Calculation.CalculatePercentage(paymentDetails.PaymentAmount, paymentDetails.TransactionCharges);
                                }
                                
                            }
                            else
                            {
                                payment.TransactionAmount = 0;
                            }
                                payment.PaymentLeft = paymentDetails.PaymentAmount - payment.TransactionAmount;
                            transactionCharges = transactionCharges + payment.TransactionAmount;
                            payment.UpdatedDate = currentDate;
                        }


                        _context.PaymentDetails.Update(payment);
                        
                    }
                }
                else
                {

                    
                    if (paymentDetails.TransactionCharges > 0)
                    {
                        if (paymentDetails.TransactionType == Constants.Constants.DeductionByAmount)
                        {
                            paymentDetails.TransactionAmount = paymentDetails.TransactionCharges;
                        }
                        else
                        {
                            paymentDetails.TransactionAmount = Constants.Calculation.CalculatePercentage(paymentDetails.PaymentAmount, paymentDetails.TransactionCharges);
                        }
                       
                    }
                    paymentDetails.PaymentLeft = paymentDetails.PaymentAmount - paymentDetails.TransactionAmount;
                    transactionCharges = transactionCharges + paymentDetails.TransactionAmount;
                    Constants.Constants.SetMastersDefault(paymentDetails, companyId, userId, currentDate);
                    await _context.PaymentDetails.AddAsync(paymentDetails);

                    reservationdetails.TransactionAmount = transactionCharges;
                    _context.ReservationDetails.Update(reservationdetails);

                    




                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Payment Updated successfully" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("UpdateRoomsCheckIn")]
        public async Task<IActionResult> UpdateRoomsCheckIn([FromBody] RoomCheckInDTO roomCheckInDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            { 
                var currentDate = DateTime.Now;
                List<CheckInNotificationDTO> notificationDTOs = new List<CheckInNotificationDTO>();
                List<BookingDetail> bookings = new List<BookingDetail>();
                if (roomCheckInDTO.rooms.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "No rooms found for checkin" });
                }

                

                foreach (var item in roomCheckInDTO.rooms)
                {
                    CheckInNotificationDTO inNotificationDTO = new CheckInNotificationDTO();
                    var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive && x.CompanyId == companyId && x.BookingId == item);
                    if (booking == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                    }
                    else
                    {
                        //update guest details
                        if (roomCheckInDTO.IsSingleRoom)
                        {
                            int Code;
                            string Message;
                            int Result;
                            (Code, Message, Result) = await AddUpdateGuest(roomCheckInDTO.GuestDetails, companyId, userId, currentDate);
                            if (Code == 400)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 400, Message = Message });
                            }
                            booking.GuestId = Result;
                            booking.PrimaryGuestId = Result;

                            var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.ReservationNo == roomCheckInDTO.ReservationNo && x.CompanyId == companyId && x.IsActive == true);
                            if(reservationDetails == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 400, Message = "Reservation details not found" });
                            }
                            reservationDetails.PrimaryGuestId = Result;
                            _context.ReservationDetails.Update(reservationDetails);
                        }
                        
                        //add notifictaion only for checkin
                        if (roomCheckInDTO.IsSelectedValue)
                        {
                            inNotificationDTO.RoomNo = await _context.RoomMaster
                                                     .Where(x => x.RoomId == booking.RoomId)
                                                     .Select(x => x.RoomNo)
                                                     .FirstOrDefaultAsync() ?? "";
                            var guestDetails = await _context.GuestDetails
                                                    .Where(x => x.GuestId == booking.GuestId && x.CompanyId == companyId)

                                                    .FirstOrDefaultAsync();
                            inNotificationDTO.GuestName = guestDetails.GuestName ?? "";
                            inNotificationDTO.GuestPhoneNo = guestDetails.PhoneNumber ?? "";
                            inNotificationDTO.GuestEmail = guestDetails.Email ?? "";
                            inNotificationDTO.Pax = booking.Pax;

                            inNotificationDTO.CheckInDateTime = booking.CheckInDateTime.ToString("f");
                            inNotificationDTO.CheckOutDateTime = booking.CheckOutDateTime.ToString("f");
                            inNotificationDTO.RoomType = await _context.RoomCategoryMaster.Where(x => x.Id == booking.RoomTypeId).Select(x => x.Type).FirstOrDefaultAsync() ?? "";
                            inNotificationDTO.ReservationNo = booking.ReservationNo;
                            notificationDTOs.Add(inNotificationDTO);
                        }

                        


                        booking.Status = roomCheckInDTO.IsSelectedValue ? Constants.Constants.CheckIn : booking.Status;
                        booking.UpdatedDate = currentDate;
                        _context.BookingDetail.Update(booking);


                        booking.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == booking.GuestId && x.IsActive == true && x.CompanyId == companyId);
                        bookings.Add(booking);

                        //update room availablility only for checkin
                        if (roomCheckInDTO.IsSelectedValue)
                        {
                            var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId && x.RoomStatus == Constants.Constants.Confirmed);
                            if (roomAvailability == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 400, Message = "Room availability not found" });
                            }

                            roomAvailability.RoomStatus = Constants.Constants.CheckIn;
                            roomAvailability.UpdatedDate = currentDate;
                            _context.RoomAvailability.Update(roomAvailability);
                        }
                      

                    }

                }

                //add notification only for checkin
                if (roomCheckInDTO.IsSelectedValue)
                {
                    var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                    if (property == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                    }


                    if (property.IsEmailNotification)
                    {
                        CheckInEmailNotification inEmailNotification = new CheckInEmailNotification(_context, notificationDTOs, companyId, property);

                        await inEmailNotification.SendEmail();
                    }

                    //if (property.IsWhatsappNotification)
                    //{
                    //    CheckInWhatsAppNotification inWhatsAppNotification = new CheckInWhatsAppNotification(_context, property, companyId, bookings);


                    //    await inWhatsAppNotification.SendWhatsAppNotification();
                    //}
                }



                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = roomCheckInDTO.IsSelectedValue ? "Rooms Check-In successfully": "Booking updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }



       

        [HttpPost("UpdateRoomShiftExtend")]
        public async Task<IActionResult> UpdateRoomShiftExtend([FromBody] ShiftExtentRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            DateTime currentDate = DateTime.Now;
            try
            {
                var validator = new ShiftExtentRequestValidator();
                var result = validator.Validate(request);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 202, Message = errors });
                }

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if(property == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 202, Message = "Property not found" });
                }

                //room shift
                if (request.Type == "Shift")
                {
                    var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == request.BookingId);
                    if (booking == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Booking not found" });
                    }

                    var newBooking = new BookingDetail
                    {
                        GuestId = booking.GuestId,
                        RoomId = booking.RoomId,
                        RoomTypeId = booking.RoomTypeId,
                        CheckInDate = booking.CheckInDate,
                        CheckInTime = booking.CheckInTime,
                        CheckOutDate = booking.CheckOutDate,
                        CheckOutTime = booking.CheckOutTime,
                        CheckInDateTime = booking.CheckInDateTime,
                        CheckOutDateTime = booking.CheckOutDateTime,
                        CheckoutFormat = booking.CheckoutFormat,
                        NoOfNights = booking.NoOfNights,
                        NoOfHours = booking.NoOfHours,
                        GstType = booking.GstType,
                        RoomCount = booking.RoomCount,
                        Pax = booking.Pax,
                        Status = booking.Status,
                        Remarks = booking.Remarks,
                        ReservationNo = booking.ReservationNo,
                        BookingDate = booking.BookingDate,
                        CreatedDate = currentDate, // new booking timestamp
                        UpdatedDate = currentDate,
                        IsActive = true,
                        BookingSource = booking.BookingSource,
                        PrimaryGuestId = booking.PrimaryGuestId,
                        InitialBalanceAmount = booking.InitialBalanceAmount,
                        BalanceAmount = booking.BalanceAmount,
                        AdvanceAmount = booking.AdvanceAmount,
                        ReceivedAmount = booking.ReceivedAmount,
                        AdvanceReceiptNo = booking.AdvanceReceiptNo,
                        RefundAmount = booking.RefundAmount,
                        InvoiceDate = booking.InvoiceDate,
                        InvoiceNo = booking.InvoiceNo,
                        UserId = userId, // assume you have userId available

                        CompanyId = booking.CompanyId,
                        BookingAmount = booking.BookingAmount,
                        GstAmount = booking.GstAmount,
                        TotalBookingAmount = booking.TotalBookingAmount,

                        ReservationDate = booking.ReservationDate,
                        ReservationTime = booking.ReservationTime,
                        ReservationDateTime = booking.ReservationDateTime,
                        InitialCheckOutDate = booking.InitialCheckOutDate,
                        InitialCheckOutTime = booking.InitialCheckOutTime,
                        InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                        ServicesAmount = booking.ServicesAmount,
                        TotalAmount = booking.TotalAmount,
                        TotalAmountWithOutDiscount = booking.TotalAmountWithOutDiscount,
                        AgentAdvanceAmount = booking.AgentAdvanceAmount,
                        InvoiceName = booking.InvoiceName,
                        BillTo = booking.BillTo,
                        TotalServicesAmount = booking.TotalServicesAmount,
                        ServicesTaxAmount = booking.ServicesTaxAmount,
                        CancelAmount = booking.CancelAmount,
                        CancelDate = booking.CancelDate,
                        CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                        IsEarlyCheckIn = booking.IsEarlyCheckIn,
                        EarlyCheckInPolicyName = booking.EarlyCheckInPolicyName,
                        EarlyCheckInApplicableOn = booking.EarlyCheckInApplicableOn,
                        EarlyCheckInFromHour = booking.EarlyCheckInFromHour,
                        EarlyCheckInToHour = booking.EarlyCheckInToHour,
                        EarlyCheckInCharges = booking.EarlyCheckInCharges,
                        IsLateCheckOut = booking.IsLateCheckOut,
                        LateCheckOutPolicyName = booking.LateCheckOutPolicyName,
                        LateCheckOutDeductionBy = booking.LateCheckOutDeductionBy,
                        LateCheckOutApplicableOn = booking.LateCheckOutApplicableOn,
                        LateCheckOutFromHour = booking.LateCheckOutFromHour,
                        LateCheckOutToHour = booking.LateCheckOutToHour,
                        LateCheckOutCharges = booking.LateCheckOutCharges,
                        DiscountType = booking.DiscountType,
                        DiscountPercentage = booking.DiscountPercentage,
                        DiscountAmount = booking.DiscountAmount,
                        BookingAmountWithoutDiscount = booking.BookingAmountWithoutDiscount,
                        DiscountTotalAmount = booking.DiscountTotalAmount,
                        CalculateRoomRates = booking.CalculateRoomRates,
                        TransactionCharges = booking.TransactionCharges,
                        AgentServiceCharge = booking.AgentServiceCharge,
                        ResidualAmount = booking.ResidualAmount,
                        CheckOutDiscountType = booking.CheckOutDiscountType,
                        CheckOutDiscountPercentage = booking.CheckOutDiscountPercentage,
                        CheckOutDiscoutAmount = booking.CheckOutDiscoutAmount,

                    };

                    if (booking.CheckoutFormat == Constants.Constants.SameDayFormat)
                    {
                        string isRoomAvailable = await CheckRoomAvailable(booking.ReservationDate, booking.ReservationTime, booking.CheckOutDate, booking.CheckOutTime, request.ShiftRoomTypeId, request.ShiftRoomId);

                        if (isRoomAvailable != "success")
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = isRoomAvailable });
                        }
                    }
                    else
                    {
                        //check new room available or not
                        string isRoomAvailable = await CheckRoomAvailable(request.ShiftDate, Constants.Constants.DayStartTime, booking.CheckOutDate, booking.CheckOutTime, request.ShiftRoomTypeId, request.ShiftRoomId);

                        if (isRoomAvailable != "success")
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = isRoomAvailable });
                        }

                    }






                    //update new booking object
                    newBooking.RoomId = request.ShiftRoomId;
                    newBooking.RoomTypeId = request.ShiftRoomTypeId;

                    await _context.BookingDetail.AddAsync(newBooking);
                    await _context.SaveChangesAsync();

                    //update booking object
                    booking.IsActive = false;
                    booking.Status = Constants.Constants.Shift;
                    booking.UpdatedDate = currentDate;
                    _context.BookingDetail.Update(booking);

                    if(newBooking.CheckoutFormat == Constants.Constants.SameDayFormat)
                    {
                        var (code, message, response) = await CalculateRoomRateAsync(companyId, request.ShiftRoomTypeId, booking.ReservationDate, booking.CheckOutDate, 1, 0, booking.GstType, booking.NoOfHours, booking.ReservationTime, booking.CheckOutTime, booking.DiscountType, booking.DiscountType == Constants.Constants.DeductionByPercentage ? booking.DiscountPercentage : booking.DiscountAmount, booking.CheckoutFormat, booking.CalculateRoomRates, property);
                        if (code != 200)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = message });
                        }

                        var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToListAsync();

                        _context.BookedRoomRates.RemoveRange(roomRates);

                        foreach (var item in response.BookedRoomRates)
                        {
                            item.BookingId = newBooking.BookingId;
                            item.RoomId = newBooking.RoomId;
                            item.ReservationNo = newBooking.ReservationNo;
                            Constants.Constants.SetMastersDefault(item, companyId, userId, currentDate);
                            await _context.BookedRoomRates.AddAsync(item);

                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        //calculate no of nights
                        int nights = Constants.Calculation.FindNightsAndHours(request.ShiftDate, Constants.Constants.DayStartTime, booking.CheckOutDate, booking.CheckOutTime, booking.CheckoutFormat);

                        var (code, message, response) = await CalculateRoomRateAsync(companyId, request.ShiftRoomTypeId, request.ShiftDate, booking.CheckOutDate, 1, nights, booking.GstType, booking.NoOfHours, Constants.Constants.DayStartTime, booking.CheckOutTime, booking.DiscountType, booking.DiscountType == Constants.Constants.DeductionByPercentage ? booking.DiscountPercentage : booking.DiscountAmount, booking.CheckoutFormat, booking.CalculateRoomRates, property);

                        //calculate room rates
                        //var (code, message, response) = await CalculateRoomRateAsync(companyId, request.ShiftRoomTypeId, request.ShiftDate, booking.CheckOutDate, booking.CheckoutFormat, 1, nights, booking.GstType, 0);
                        if (code != 200)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = message });
                        }

                        var bookedRoomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == request.BookingId).OrderBy(x => x.BookingDate).ToListAsync();
                        if (bookedRoomRates.Count == 0)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = "No room rates found" });
                        }
                        foreach (var item in bookedRoomRates)
                        {
                            if (item.BookingDate >= request.ShiftDate)
                            {
                                _context.BookedRoomRates.Remove(item);

                            }
                            else
                            {
                                item.BookingId = newBooking.BookingId;
                                item.UpdatedDate = currentDate;
                                _context.BookedRoomRates.Update(item);

                            }
                        }
                        foreach (var item in response.BookedRoomRates)
                        {
                            item.BookingId = newBooking.BookingId;
                            item.RoomId = newBooking.RoomId;
                            item.ReservationNo = newBooking.ReservationNo;
                            Constants.Constants.SetMastersDefault(item, companyId, userId, currentDate);
                            await _context.BookedRoomRates.AddAsync(item);

                        }
                        await _context.SaveChangesAsync();

                       

                    }


                    var newbookedRoomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == newBooking.BookingId).OrderBy(x => x.BookingDate).ToListAsync();

                    (newBooking.BookingAmount, newBooking.GstAmount, newBooking.TotalBookingAmount) = CalculateTotalBookedRoomRate(newbookedRoomRates);

                    newBooking.TotalAmount = BookingCalulation.BookingTotalAmount(newBooking);
                    newBooking.TotalAmountWithOutDiscount = newBooking.TotalAmount;
                    _context.BookingDetail.Update(newBooking);



                    //roomavailability
                    var roomAvailaibility = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId);
                    if (roomAvailaibility == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "No room availability found" });
                    }

                    if(newBooking.CheckoutFormat == Constants.Constants.SameDayFormat)
                    {
                        
                        roomAvailaibility.RoomStatus = Constants.Constants.Dirty;
                        roomAvailaibility.UpdatedDate = currentDate;
                        _context.RoomAvailability.Update(roomAvailaibility);

                    }
                    else
                    {
                        roomAvailaibility.CheckOutDate = DateTimeMethod.GetADayBefore(request.ShiftDate);
                        roomAvailaibility.CheckOutTime = Constants.Constants.DayEndTime;
                        roomAvailaibility.CheckOutDateTime = DateTimeMethod.ConvertToDateTime(roomAvailaibility.CheckOutDate, roomAvailaibility.CheckOutTime);
                        roomAvailaibility.RoomStatus = Constants.Constants.Dirty;
                        roomAvailaibility.UpdatedDate = currentDate;
                        _context.RoomAvailability.Update(roomAvailaibility);

                    }

                    if (booking.CheckoutFormat == Constants.Constants.SameDayFormat)
                    {
                        //create new room availability
                        var newRoomAvailability = new RoomAvailability();
                        newRoomAvailability.CheckInDate = newBooking.ReservationDate;
                        newRoomAvailability.CheckInTime = newBooking.ReservationTime;
                        newRoomAvailability.CheckInDateTime = newBooking.ReservationDateTime;
                        newRoomAvailability.CheckOutDate = newBooking.CheckOutDate;
                        newRoomAvailability.CheckOutTime = newBooking.CheckOutTime;
                        newRoomAvailability.CheckOutDateTime = newBooking.CheckOutDateTime;
                        newRoomAvailability.ReservationNo = booking.ReservationNo;
                        newRoomAvailability.BookingId = newBooking.BookingId;
                        newRoomAvailability.RoomId = newBooking.RoomId;
                        newRoomAvailability.RoomStatus = newBooking.Status;
                        newRoomAvailability.RoomTypeId = newBooking.RoomTypeId;
                        Constants.Constants.SetMastersDefault(newRoomAvailability, companyId, userId, currentDate);
                        await _context.RoomAvailability.AddAsync(newRoomAvailability);
                    }
                    else
                    {
                        //create new room availability
                        var newRoomAvailability = new RoomAvailability();
                        newRoomAvailability.CheckInDate = request.ShiftDate;
                        newRoomAvailability.CheckInTime = Constants.Constants.DayStartTime;
                        newRoomAvailability.CheckInDateTime = DateTimeMethod.ConvertToDateTime(newRoomAvailability.CheckInDate, newRoomAvailability.CheckInTime);
                        newRoomAvailability.CheckOutDate = booking.CheckOutDate;
                        newRoomAvailability.CheckOutTime = booking.CheckOutTime;
                        newRoomAvailability.CheckOutDateTime = DateTimeMethod.ConvertToDateTime(newRoomAvailability.CheckOutDate, newRoomAvailability.CheckOutTime);
                        newRoomAvailability.ReservationNo = request.ReservationNo;
                        newRoomAvailability.BookingId = newBooking.BookingId;
                        newRoomAvailability.RoomId = newBooking.RoomId;
                        newRoomAvailability.RoomStatus = newBooking.Status;
                        newRoomAvailability.RoomTypeId = newBooking.RoomTypeId;
                        Constants.Constants.SetMastersDefault(newRoomAvailability, companyId, userId, currentDate);
                        await _context.RoomAvailability.AddAsync(newRoomAvailability);
                    }

                       


                    //advance services
                    var advanceServices = await _context.AdvanceServices.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == request.BookingId).ToListAsync();

                    if (advanceServices.Count > 0)
                    {
                        if(newBooking.CheckoutFormat == Constants.Constants.SameDayFormat)
                        {
                            foreach (var service in advanceServices)
                            {
                                service.RoomId = newBooking.RoomId;
                                    service.BookingId = newBooking.BookingId;
                                
                                service.UpdatedDate = currentDate;
                                _context.AdvanceServices.Update(service);

                            }
                        }
                        else
                        {
                            foreach (var service in advanceServices)
                            {
                                if (service.ServiceDate >= request.ShiftDate)
                                {
                                    service.BookingId = newBooking.BookingId;
                                    service.RoomId = newBooking.RoomId;
                                }
                                else
                                {
                                    service.BookingId = newBooking.BookingId;
                                }
                                service.UpdatedDate = currentDate;
                                _context.AdvanceServices.Update(service);

                            }
                        }
                            
                    }


                    //payment details
                    var payments = await _context.PaymentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId && x.RoomId == booking.RoomId).ToListAsync();
                    foreach (var pay in payments)
                    {
                        pay.BookingId = newBooking.BookingId;
                        pay.RoomId = newBooking.RoomId;
                        pay.UpdatedDate = currentDate;
                        _context.PaymentDetails.Update(pay);

                    }

                    //reservation details
                    var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == request.ReservationNo);

                    if (reservationDetails == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Reservation not found" });
                    }

                    List<BookingDetailDTO> allBookings = await _context.BookingDetail.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == request.ReservationNo).Select(x => _mapper.Map<BookingDetailDTO>(x)).ToListAsync();

                    decimal bookingAmount = 0;
                    
                    decimal bookingAmountWithGst = 0;

                    (bookingAmount, decimal gstAmount , bookingAmountWithGst) = BookingCalulation.ReservationRoomsTotal(allBookings);

                    

                    if (reservationDetails.AgentId > 0)
                    {
                        var (code1, message1, agentCommisionResponse) = await CalculateAgentCommisionAsync(reservationDetails.AgentId, bookingAmount, bookingAmountWithGst);
                        if (code1 != 200 && agentCommisionResponse != null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = message1 });
                        }
                        else
                        {
                            reservationDetails.AgentGstType = agentCommisionResponse.AgentGstType;
                            reservationDetails.CommissionPercentage = agentCommisionResponse.AgentCommissionPercentage;
                            reservationDetails.CommissionAmount = agentCommisionResponse.AgentCommisionAmount;
                            reservationDetails.Tcs = agentCommisionResponse.TcsPercentage;
                            reservationDetails.TcsAmount = agentCommisionResponse.TcsAmount;
                            reservationDetails.Tds = agentCommisionResponse.TdsPercentage;
                            reservationDetails.TdsAmount = agentCommisionResponse.TdsAmount;
                        }
                    }

                    reservationDetails.UpdatedDate = currentDate;
                    _context.ReservationDetails.Update(reservationDetails);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        Code = 200,
                        Message =
                        "Room Shift successfully"
                    });


                }

                else
                {
                    var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == request.BookingId);
                    if (booking == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Booking not found" });
                    }

                    //check new room available or not
                    DateOnly tempDate = DateOnly.FromDateTime(DateTime.Now);
                    string tempTime = "";
                    (tempDate, tempTime) = DateTimeMethod.GetAMinuteAfter(booking.CheckOutDate, booking.CheckOutTime);
                    if (booking.CheckoutFormat == Constants.Constants.SameDayFormat)
                    {
                        //calculate checkout date
                        (booking.CheckOutDate, booking.CheckOutTime) = Constants.DateTimeMethod.CalculateCheckoutDateTimeOnHour(booking.ReservationDateTime, request.ExtendHour);

                        string isRoomAvailable = await CheckRoomAvailable(tempDate, tempTime, booking.CheckOutDate, booking.CheckOutTime, booking.RoomTypeId, booking.RoomId);

                        if (isRoomAvailable != "success")
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = isRoomAvailable });
                        }

                        var (code, message, response) = await CalculateRoomRateAsync(companyId, booking.RoomTypeId, booking.ReservationDate, booking.CheckOutDate, 1, 0, booking.GstType, request.ExtendHour, booking.ReservationTime, booking.CheckOutTime, booking.DiscountType, booking.DiscountType == Constants.Constants.DeductionByPercentage ? booking.DiscountPercentage : booking.DiscountAmount, booking.CheckoutFormat, booking.CalculateRoomRates, property);

                        if (code != 200)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = message });
                        }

                        var roomRates = await _context.BookedRoomRates.Where(x => x.BookingId == booking.BookingId && x.IsActive == true && x.CompanyId == companyId).ToListAsync();

                        _context.BookedRoomRates.RemoveRange(roomRates);
                       

                        foreach (var item in response.BookedRoomRates)
                        {
                            item.BookingId = booking.BookingId;
                            item.RoomId = booking.RoomId;
                            item.ReservationNo = booking.ReservationNo;
                            Constants.Constants.SetMastersDefault(item, companyId, userId, currentDate);
                            await _context.BookedRoomRates.AddAsync(item);
                        }
                        await _context.SaveChangesAsync();

                        booking.NoOfHours = request.ExtendHour;
                        booking.InitialCheckOutDate = booking.CheckOutDate;
                        booking.InitialCheckOutTime = booking.CheckOutTime;
                        booking.CheckOutDateTime = DateTimeMethod.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);
                        booking.InitialCheckOutDateTime = booking.CheckOutDateTime;

                    }
                    else
                    {
                        string isRoomAvailable = await CheckRoomAvailable(tempDate, tempTime, request.ExtendedDate, booking.CheckOutTime, booking.RoomTypeId, booking.RoomId);

                        if (isRoomAvailable != "success")
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = isRoomAvailable });
                        }

                        //calculate nights
                        int nights = Constants.Calculation.FindNightsAndHours(booking.CheckOutDate, booking.CheckOutTime, request.ExtendedDate, booking.CheckOutTime, booking.CheckoutFormat);

                        var (code, message, response) = await CalculateRoomRateAsync(companyId, booking.RoomTypeId, booking.CheckOutDate, request.ExtendedDate, 1, nights, booking.GstType, 0, booking.CheckOutTime, booking.CheckOutTime, booking.DiscountType, booking.DiscountType == Constants.Constants.DeductionByPercentage ? booking.DiscountPercentage :  booking.DiscountAmount, booking.CheckoutFormat, booking.CalculateRoomRates, property);

                        //var (code, message, response) = await CalculateRoomRateAsync(companyId, booking.RoomTypeId, booking.CheckOutDate, request.ExtendedDate, booking.CheckoutFormat, 1, nights, booking.GstType, 0);
                        if (code != 200)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = message });
                        }

                        foreach (var item in response.BookedRoomRates)
                        {
                            item.BookingId = booking.BookingId;
                            item.RoomId = booking.RoomId;
                            item.ReservationNo = booking.ReservationNo;
                            Constants.Constants.SetMastersDefault(item, companyId, userId, currentDate);
                            await _context.BookedRoomRates.AddAsync(item);
                        }
                        await _context.SaveChangesAsync();
                        
                        booking.CheckOutDate = request.ExtendedDate;
                        booking.InitialCheckOutDate = request.ExtendedDate;

                        booking.CheckOutDateTime = DateTimeMethod.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);
                        booking.InitialCheckOutDateTime = booking.CheckOutDateTime;
                        booking.NoOfNights = Constants.Calculation.FindNightsAndHours(booking.ReservationDate, booking.ReservationTime, booking.CheckOutDate, booking.CheckOutTime, booking.CheckoutFormat);
                    }
                    

                    

                    

                    var newbookedRoomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).OrderBy(x => x.BookingDate).ToListAsync();

                    (booking.BookingAmount, booking.GstAmount, booking.TotalBookingAmount) = CalculateTotalBookedRoomRate(newbookedRoomRates);
                    booking.TotalAmount = BookingCalulation.BookingTotalAmount(booking);
                    booking.TotalAmountWithOutDiscount = booking.TotalAmount;
                   
                    booking.UpdatedDate = currentDate;
                    _context.BookingDetail.Update(booking);
                    await _context.SaveChangesAsync();

                    //room availability
                    var roomAvailaibility = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId);
                    if (roomAvailaibility == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "No room availability found" });
                    }
                    roomAvailaibility.CheckOutTime = booking.CheckOutTime;
                    roomAvailaibility.CheckOutDate = booking.CheckOutDate;
                    roomAvailaibility.CheckOutDateTime = booking.CheckOutDateTime;
                    roomAvailaibility.UpdatedDate = currentDate;
                    _context.RoomAvailability.Update(roomAvailaibility);

                    //reservation detail
                    var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == request.ReservationNo);

                    if (reservationDetails == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Reservation not found" });
                    }
                    List<BookingDetailDTO> allBookings = await _context.BookingDetail.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == request.ReservationNo).Select(x => _mapper.Map<BookingDetailDTO>(x)).ToListAsync();

                    (reservationDetails.TotalRoomPayment, reservationDetails.TotalGst, reservationDetails.TotalAmount) = BookingCalulation.ReservationRoomsTotal(allBookings);

                    if (reservationDetails.AgentId > 0)
                    {
                        var (code1, message1, agentCommisionResponse) = await CalculateAgentCommisionAsync(reservationDetails.AgentId, reservationDetails.TotalRoomPayment, reservationDetails.TotalAmount);
                        if (code1 != 200 && agentCommisionResponse != null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = message1 });
                        }
                        else
                        {
                            reservationDetails.AgentGstType = agentCommisionResponse.AgentGstType;
                            reservationDetails.CommissionPercentage = agentCommisionResponse.AgentCommissionPercentage;
                            reservationDetails.CommissionAmount = agentCommisionResponse.AgentCommisionAmount;
                            reservationDetails.Tcs = agentCommisionResponse.TcsPercentage;
                            reservationDetails.TcsAmount = agentCommisionResponse.TcsAmount;
                            reservationDetails.Tds = agentCommisionResponse.TdsPercentage;
                            reservationDetails.TdsAmount = agentCommisionResponse.TdsAmount;
                        }
                    }

                    reservationDetails.UpdatedDate = currentDate;
                    _context.ReservationDetails.Update(reservationDetails);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        Code = 200,
                        Message =
                        "Room Extended successfully"
                    });



                }



            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetPayments")]
        public async Task<IActionResult> GetPayments(int roomId, string reservationNo, string filter, int guestId)
        {
            try
            {
                var response = new AddPaymentResponse();
                response.ReservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);


                if (response.ReservationDetails == null)
                {
                    return Ok(new { Code = 500, Message = "Reservation details not found" });
                }

                var statusList = new List<string> { Constants.Constants.Pending, Constants.Constants.Confirmed, Constants.Constants.CheckIn };
                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.GuestId == guestId);

                response.BookingDetails = await (from booking in _context.BookingDetail
                                                 join room in _context.RoomMaster on booking.RoomId equals room.RoomId into roomLeft
                                                 from rooms in roomLeft.DefaultIfEmpty()
                                                 join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                                 where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == reservationNo && statusList.Contains(booking.Status)
                                                 select new BookingDetail
                                                 {
                                                     BookingId = booking.BookingId,
                                                     GuestId = booking.GuestId,
                                                     RoomId = booking.RoomId,
                                                     RoomTypeId = booking.RoomTypeId,
                                                     CheckInDate = booking.CheckInDate,
                                                     CheckInTime = booking.CheckInTime,
                                                     CheckOutDate = booking.CheckOutDate,
                                                     CheckOutTime = booking.CheckOutTime,
                                                     CheckInDateTime = booking.CheckInDateTime,
                                                     CheckOutDateTime = booking.CheckOutDateTime,
                                                     CheckoutFormat = booking.CheckoutFormat,
                                                     NoOfNights = booking.NoOfNights,
                                                     NoOfHours = booking.NoOfHours,
                                                    
                                                     RoomCount = booking.RoomCount,
                                                     Pax = booking.Pax,
                                                     Status = booking.Status,
                                                     Remarks = booking.Remarks,
                                                     ReservationNo = booking.ReservationNo,
                                                     BookingDate = booking.BookingDate,
                                                     CreatedDate = booking.CreatedDate,
                                                     UpdatedDate = booking.UpdatedDate,
                                                     IsActive = booking.IsActive,
                                                     PrimaryGuestId = booking.PrimaryGuestId,
                                                     InitialBalanceAmount = booking.InitialBalanceAmount,
                                                     BalanceAmount = booking.BalanceAmount,
                                                     AdvanceAmount = booking.AdvanceAmount,
                                                     ReceivedAmount = booking.ReceivedAmount,
                                                     AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                                     RefundAmount = booking.RefundAmount,

                                                     UserId = booking.UserId,
                                                     GstType = booking.GstType,
                                                     CompanyId = booking.CompanyId,
                                                     BookingAmount = booking.BookingAmount,
                                                     GstAmount = booking.GstAmount,
                                                     TotalBookingAmount = booking.TotalBookingAmount,
                                                     BookingSource = booking.BookingSource,
                                                     ReservationDate = booking.ReservationDate,
                                                     ReservationTime = booking.ReservationTime,
                                                     ReservationDateTime = booking.ReservationDateTime,
                                                     RoomTypeName = roomType.Type,
                                                     RoomNo = rooms.RoomNo,
                                                     InitialCheckOutDate = booking.InitialCheckOutDate,
                                                     InitialCheckOutTime = booking.InitialCheckOutTime,
                                                     InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                                     BillTo = "",

                                                     TotalAmount = booking.TotalAmount,
                                                     TotalAmountWithOutDiscount = booking.TotalAmountWithOutDiscount,
                                                     BookedRoomRates = _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList()
                                                 }).ToListAsync();

                response.PaymentDetails = await (from payment in _context.PaymentDetails
                                                 join room in _context.RoomMaster on payment.RoomId equals room.RoomId into roomleft
                                                 from rooms in roomleft.DefaultIfEmpty()
                                                 where payment.IsActive == true && payment.CompanyId == companyId && payment.ReservationNo == reservationNo
                                                 select new PaymentDetails
                                                 {
                                                     PaymentId = payment.PaymentId,
                                                     BookingId = payment.BookingId,
                                                     ReservationNo = payment.ReservationNo,
                                                     PaymentDate = payment.PaymentDate,
                                                     PaymentMethod = payment.PaymentMethod,
                                                     TransactionId = payment.TransactionId,
                                                     PaymentStatus = payment.PaymentStatus,
                                                     PaymentType = payment.PaymentType,
                                                     BankName = payment.BankName,
                                                     PaymentReferenceNo = payment.PaymentReferenceNo,
                                                     PaidBy = payment.PaidBy,
                                                     Remarks = payment.Remarks,
                                                     Other1 = payment.Other1,
                                                     Other2 = payment.Other2,
                                                     IsActive = payment.IsActive,
                                                     IsReceived = payment.IsReceived,
                                                     RoomId = payment.RoomId,
                                                     RoomNo = rooms.RoomNo,
                                                     UserId = payment.UserId,
                                                     PaymentFormat = payment.PaymentFormat,
                                                     RefundAmount = payment.RefundAmount,
                                                     PaymentAmount = payment.PaymentAmount,
                                                     
                                                     CreatedDate = payment.CreatedDate,
                                                     UpdatedDate = payment.UpdatedDate,
                                                     CompanyId = payment.CompanyId,
                                                     PaymentLeft = payment.PaymentLeft
                                                 }).ToListAsync();

                if (response.BookingDetails.Count == 0)
                {
                    return Ok(new { Code = 500, Message = "Bookings not found" });
                }
                if (filter == Constants.Constants.ReservationWisePayment)
                {
                    response.PaymentSummary = await CalculateSummary(response.ReservationDetails, response.BookingDetails);
                }
                else
                {
                    var result = response.BookingDetails.Where(b => b.RoomId == roomId).ToList();
                    response.PaymentSummary = await CalculateSummary(response.ReservationDetails, result);
                }

                return Ok(new { Code = 200, Message = "Data fetched", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchPaymentDetails/{id}")]
        public async Task<IActionResult> PatchServicableMaster(int id, [FromBody] JsonPatchDocument<PaymentDetails> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var payment = await _context.PaymentDetails.FindAsync(id);

            if (payment == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(payment, ModelState);
            payment.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Payment updated successfully" });
        }

        [HttpGet("GetBookingsOnCheckOut")]
        public async Task<IActionResult> GetBookingsOnCheckOut(string reservationNo, int guestId)
        {
            try
            {
                var response = new CheckOutResponse();
                response.ReservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                
                if(property == null)
                {
                    return Ok(new { Code = 500, Message = "Property details not found" });
                }
                response.PropertyDetails = property;
                if (response.ReservationDetails == null)
                {
                    return Ok(new { Code = 500, Message = "Reservation details not found" });
                }

                string updatedInvoiceNo = await DocumentHelper.GetDocumentNo(_context, Constants.Constants.DocumentInvoice, companyId, financialYear);

                if (updatedInvoiceNo == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found."});
                }
                else
                {
                    response.InvoiceNo = updatedInvoiceNo;
                }


                response.CheckOutDiscountType = Constants.Constants.DeductionByPercentage;
                //Get primary guest details
                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.GuestId == guestId);


                var allBookingsCount = await _context.BookingDetail.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo && x.Status != Constants.Constants.CheckOut).CountAsync();

                response.BookingDetails = await (from booking in _context.BookingDetail
                                                 join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                                 join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                                 join guest in _context.GuestDetails
                                                 on booking.GuestId equals guest.GuestId
                                                 where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == reservationNo && booking.Status == Constants.Constants.CheckIn orderby booking.TotalAmount
                                                 select new BookingDetail
                                                 {
                                                     BookingId = booking.BookingId,
                                                     GuestId = booking.GuestId,
                                                     RoomId = booking.RoomId,
                                                     RoomTypeId = booking.RoomTypeId,
                                                     CheckInDate = booking.CheckInDate,
                                                     CheckInTime = booking.CheckInTime,
                                                     CheckOutDate = booking.CheckOutDate,
                                                     CheckOutTime = booking.CheckOutTime,
                                                     CheckInDateTime = booking.CheckInDateTime,
                                                     CheckOutDateTime = booking.CheckOutDateTime,
                                                     CheckoutFormat = booking.CheckoutFormat,
                                                     NoOfNights = booking.NoOfNights,
                                                     NoOfHours = booking.NoOfHours,
                                                     RoomCount = booking.RoomCount,
                                                     Pax = booking.Pax,
                                                     Status = booking.Status,
                                                     Remarks = booking.Remarks,
                                                     ReservationNo = booking.ReservationNo,
                                                     BookingDate = booking.BookingDate,
                                                     CreatedDate = booking.CreatedDate,
                                                     UpdatedDate = booking.UpdatedDate,
                                                     IsActive = booking.IsActive,
                                                     PrimaryGuestId = booking.PrimaryGuestId,
                                                     InitialBalanceAmount = booking.InitialBalanceAmount,
                                                     BalanceAmount = booking.BalanceAmount,
                                                     AdvanceAmount = booking.AdvanceAmount,
                                                     ReceivedAmount = booking.ReceivedAmount,
                                                     AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                                     RefundAmount = booking.RefundAmount,
                                                     InvoiceDate = response.InvoiceDate,
                                                     InvoiceNo = response.InvoiceNo,
                                                     UserId = booking.UserId,
                                                     GstType = booking.GstType,
                                                     CompanyId = booking.CompanyId,
                                                     BookingAmount = booking.BookingAmount,
                                                     GstAmount = booking.GstAmount,
                                                     TotalBookingAmount = booking.TotalBookingAmount,
                                                     BookingSource = booking.BookingSource,
                                                     ReservationDate = booking.ReservationDate,
                                                     ReservationTime = booking.ReservationTime,
                                                     ReservationDateTime = booking.ReservationDateTime,
                                                     InitialCheckOutDate = booking.InitialCheckOutDate,
                                                     InitialCheckOutTime = booking.InitialCheckOutTime,
                                                     InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                                     ServicesAmount = booking.ServicesAmount,
                                                     TotalAmount = booking.TotalAmount,
                                                     TotalAmountWithOutDiscount = booking.TotalAmountWithOutDiscount,
                                                   
                                                     AgentAdvanceAmount = booking.AgentAdvanceAmount,
                                                     InvoiceName = booking.InvoiceName,
                                                     BillTo = booking.BillTo,
                                                     TotalServicesAmount = booking.TotalServicesAmount,
                                                     ServicesTaxAmount = booking.ServicesTaxAmount,
                                                     CancelAmount = booking.CancelAmount,
                                                     CancelDate = booking.CancelDate,
                                                     CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                                     IsEarlyCheckIn = booking.IsEarlyCheckIn,
                                                     EarlyCheckInPolicyName = booking.EarlyCheckInPolicyName,
                                                     EarlyCheckInDeductionBy = booking.EarlyCheckInDeductionBy,
                                                     EarlyCheckInApplicableOn = booking.EarlyCheckInApplicableOn,
                                                     EarlyCheckInFromHour = booking.EarlyCheckInFromHour,
                                                     EarlyCheckInToHour = booking.EarlyCheckInToHour,
                                                     EarlyCheckInCharges = booking.EarlyCheckInCharges,
                                                     IsLateCheckOut = booking.IsLateCheckOut,
                                                     LateCheckOutPolicyName = booking.LateCheckOutPolicyName,
                                                     LateCheckOutDeductionBy = booking.LateCheckOutDeductionBy,
                                                     LateCheckOutApplicableOn = booking.LateCheckOutApplicableOn,
                                                     LateCheckOutFromHour = booking.LateCheckOutFromHour,
                                                     LateCheckOutToHour = booking.LateCheckOutToHour,
                                                     LateCheckOutCharges = booking.LateCheckOutCharges,
                                                     DiscountType = booking.DiscountType,
                                                     DiscountPercentage = booking.DiscountPercentage,
                                                     DiscountAmount = booking.DiscountAmount,
                                                     DiscountTotalAmount = booking.DiscountTotalAmount,
                                                     BookingAmountWithoutDiscount = booking.BookingAmountWithoutDiscount,
                                                     CalculateRoomRates = booking.CalculateRoomRates,
                                                     TransactionCharges = booking.TransactionCharges,
                                                     IsSelectedValue = true,
                                                     CheckOutDiscountType = Constants.Constants.DeductionByPercentage,
                                                     CheckOutDiscountPercentage = 0,
                                                     CheckOutDiscoutAmount = 0,
                                                     // NotMapped fields
                                                     RoomTypeName = roomType.Type,
                                                     RoomNo = rooms.RoomNo ,
                                                     BookedRoomRates = (from rates in _context.BookedRoomRates
                                                                        join type in _context.RoomCategoryMaster on rates.RoomTypeId equals type.Id 
                                                                        where rates.IsActive == true && rates.CompanyId == companyId && rates.BookingId == booking.BookingId
                                                                        select new BookedRoomRate
                                                                        {
                                                                            Id = rates.Id,
                                                                            BookingId = rates.BookingId,
                                                                            RoomId = rates.RoomId,
                                                                            ReservationNo = rates.ReservationNo,
                                                                            RoomRate = rates.RoomRate,
                                                                            GstPercentage = rates.GstPercentage,
                                                                            GstAmount = rates.GstAmount,
                                                                            TotalRoomRate = rates.TotalRoomRate,
                                                                            GstType = rates.GstType,
                                                                            BookingDate = rates.BookingDate,
                                                                            CreatedDate = rates.CreatedDate,
                                                                            UpdatedDate = rates.UpdatedDate,
                                                                            UserId = rates.UserId,
                                                                            CompanyId = rates.CompanyId,
                                                                            IsActive = rates.IsActive,
                                                                            CGST = rates.CGST,
                                                                            CGSTAmount = rates.CGSTAmount,
                                                                            SGST = rates.SGST,
                                                                            SGSTAmount = rates.SGSTAmount,
                                                                            DiscountType = rates.DiscountType,
                                                                            DiscountPercentage = rates.DiscountPercentage,
                                                                            DiscountAmount = rates.DiscountAmount,
                                                                            RoomRateWithoutDiscount = rates.RoomRateWithoutDiscount,
                                                                            RoomTypeId = rates.RoomTypeId,
                                                                            RoomTypeName = type.Type
                                                                        }).ToList(),
                                                     
                                                     GuestDetails = guest,
                                                     AdvanceServices = _context.AdvanceServices.Where(x => x.IsActive == true && x.BookingId == booking.BookingId).ToList(),
                                                    
                                                 }).ToListAsync();

                bool isAllRoomCheckOut = allBookingsCount == response.BookingDetails.Count;
                if (response.BookingDetails.Count == 0)
                {
                    return Ok(new { Code = 500, Message = "Bookings not found" });
                }

                response.PaymentDetails = await (from pay in _context.PaymentDetails
                                                 join room in _context.RoomMaster on pay.RoomId equals room.RoomId into rooms
                                                 from roommaster in rooms.DefaultIfEmpty()
                                                 where pay.IsActive == true && pay.IsReceived == false && pay.PaymentLeft > 0 && pay.CompanyId == companyId && pay.ReservationNo == reservationNo
                                                 select new PaymentDetails
                                                 {
                                                     PaymentId = pay.PaymentId,
                                                     BookingId = pay.BookingId,
                                                     ReservationNo = pay.ReservationNo,
                                                     PaymentDate = pay.PaymentDate,
                                                     PaymentMethod = pay.PaymentMethod,
                                                     TransactionId = pay.TransactionId,
                                                     PaymentStatus = pay.PaymentStatus,
                                                     PaymentType = pay.PaymentType,
                                                     BankName = pay.BankName,
                                                     PaymentReferenceNo = pay.PaymentReferenceNo,
                                                     PaidBy = pay.PaidBy,
                                                     Remarks = pay.Remarks,
                                                     Other1 = pay.Other1,
                                                     Other2 = pay.Other2,
                                                     IsActive = pay.IsActive,
                                                     IsReceived = pay.IsReceived,
                                                     RoomId = pay.RoomId,
                                                     UserId = pay.UserId,
                                                     PaymentFormat = pay.PaymentFormat,
                                                     RefundAmount = pay.RefundAmount,
                                                     PaymentAmount = pay.PaymentAmount,
                                                     PaymentLeft = pay.PaymentLeft,
                                                     CreatedDate = pay.CreatedDate,
                                                     UpdatedDate = pay.UpdatedDate,
                                                     CompanyId = pay.CompanyId,
                                                     RoomNo = roommaster != null ? roommaster.RoomNo : "",
                                                     TransactionAmount = pay.TransactionAmount,
                                                     TransactionType = pay.TransactionType,
                                                     TransactionCharges = pay.TransactionCharges,
                                                     IsEditable = pay.PaymentAmount != pay.PaymentLeft ? false : true
                                                 }).ToListAsync();

               

                //Set agent service charge
                SetAgentServiceCharge(response.BookingDetails, response.ReservationDetails.AgentTotalServiceCharge, response.ReservationDetails.RoomsCount);


                response.PaymentSummary = await CalculateSummaryForCheckOut(response.ReservationDetails, response.BookingDetails, response.PaymentDetails, isAllRoomCheckOut);

                CalculateInvoice(response.BookingDetails, response.PaymentDetails, Constants.Constants.CheckOut);

                foreach (var item in response.BookingDetails)
                {
                    item.BalanceAmount = CalculateCheckOutBalanceBooking(item);
                    
                }

                if(allBookingsCount == response.BookingDetails.Count)
                {
                    SetRefundAmount(response.PaymentDetails, response.BookingDetails);
                }
                else
                {
                    SetResidualAmount(response.PaymentDetails, response.BookingDetails);
                }


                foreach (var item in response.BookingDetails)
                {
                    
                    response.PaymentSummary.BalanceAmount += item.BalanceAmount;
                    response.PaymentSummary.RefundAmount += item.RefundAmount;
                    response.PaymentSummary.ResidualAmount += item.ResidualAmount;
                }


                var pdfBytes = GenerateInvoicePdf(response, "").ToArray();
                return File(pdfBytes, "application/pdf", "invoice.pdf");

                

                //return Ok(new { Code = 200, Message = "Data fetch successfully", data = response });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("generate")]
        public IActionResult GeneratePdf()
        {
            // Create a memory stream to store the PDF
            using var ms = new MemoryStream();

            // Initialize PDF writer and document
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add content to the document
            document.Add(new Paragraph("Hello from iText 7 in .NET Core Web API!"));

            // Close the document
            document.Close();

            // Return the PDF as a FileStreamResult
            var pdfBytes = ms.ToArray();
            return File(pdfBytes, "application/pdf", "example.pdf");
        }


        [HttpPost("CalculateRoomRateOnCheckOut")]
        public async Task<IActionResult> CalculateRoomRateOnCheckOut([FromBody] CalculateRoomRateRequest request)
        {
            try
            {
                
                var response = new CheckOutResponse();
                if (request.Bookings.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "Invalid data", data = response });
                }

                ICollection<int> keys = request.Bookings.Keys;

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if (property == null)
                {
                    return Ok(new { Code = 400, Message = "Property not found", data = response });
                }


                // Or convert to a list if needed
                List<int> bookingIdList = request.Bookings.Keys.ToList();


                int allBookingsCount = await _context.BookingDetail.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == request.ReservationDetails.ReservationNo && x.Status != Constants.Constants.CheckOut).CountAsync();

                bool isAllRoomCheckOut = allBookingsCount == bookingIdList.Count ? true : false;

                List<BookingDetail> bookings = await (from booking in _context.BookingDetail
                                                            join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                                            join guest in _context.GuestDetails
                                                            on booking.GuestId equals guest.GuestId
                                                            where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == request.ReservationDetails.ReservationNo && bookingIdList.Contains(booking.BookingId) orderby booking.TotalAmount
                                                            select new BookingDetail
                                                            {
                                                                BookingId = booking.BookingId,
                                                                GuestId = booking.GuestId,
                                                                RoomId = booking.RoomId,
                                                                RoomTypeId = booking.RoomTypeId,
                                                                CheckInDate = booking.CheckInDate,
                                                                CheckInTime = booking.CheckInTime,
                                                                CheckOutDate = booking.CheckOutDate,
                                                                CheckOutTime = booking.CheckOutTime,
                                                                CheckInDateTime = booking.CheckInDateTime,
                                                                CheckOutDateTime = booking.CheckOutDateTime,
                                                                CheckoutFormat = booking.CheckoutFormat,
                                                                NoOfNights = booking.NoOfNights,
                                                                NoOfHours = booking.NoOfHours,
                                                                RoomCount = booking.RoomCount,
                                                                Pax = booking.Pax,
                                                                Status = booking.Status,
                                                                Remarks = booking.Remarks,
                                                                ReservationNo = booking.ReservationNo,
                                                                BookingDate = booking.BookingDate,
                                                                CreatedDate = booking.CreatedDate,
                                                                UpdatedDate = booking.UpdatedDate,
                                                                IsActive = booking.IsActive,
                                                                PrimaryGuestId = booking.PrimaryGuestId,
                                                                InitialBalanceAmount = booking.InitialBalanceAmount,
                                                                BalanceAmount = booking.BalanceAmount,
                                                                AdvanceAmount = booking.AdvanceAmount,
                                                                ReceivedAmount = booking.ReceivedAmount,
                                                                AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                                                RefundAmount = booking.RefundAmount,
                                                                InvoiceDate = request.InvoiceDate,
                                                                InvoiceNo = request.InvoiceNo,
                                                                UserId = booking.UserId,
                                                                GstType = booking.GstType,
                                                                CompanyId = booking.CompanyId,
                                                                BookingAmount = booking.BookingAmount,
                                                                GstAmount = booking.GstAmount,
                                                                TotalBookingAmount = booking.TotalBookingAmount,
                                                                BookingSource = booking.BookingSource,
                                                                ReservationDate = booking.ReservationDate,
                                                                ReservationTime = booking.ReservationTime,
                                                                ReservationDateTime = booking.ReservationDateTime,
                                                                InitialCheckOutDate = booking.InitialCheckOutDate,
                                                                InitialCheckOutTime = booking.InitialCheckOutTime,
                                                                InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                                                ServicesAmount = booking.ServicesAmount,
                                                                TotalAmount = booking.TotalAmount,
                                                                TotalAmountWithOutDiscount = booking.TotalAmount,
                                                                AgentAdvanceAmount = booking.AgentAdvanceAmount,
                                                                InvoiceName = booking.InvoiceName,
                                                                BillTo = booking.BillTo,
                                                                TotalServicesAmount = booking.TotalServicesAmount,
                                                                ServicesTaxAmount = booking.ServicesTaxAmount,
                                                                CancelAmount = booking.CancelAmount,
                                                                CancelDate = booking.CancelDate,
                                                                CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                                                IsEarlyCheckIn = booking.IsEarlyCheckIn,
                                                                EarlyCheckInPolicyName = booking.EarlyCheckInPolicyName,
                                                                EarlyCheckInDeductionBy = booking.EarlyCheckInDeductionBy,
                                                                EarlyCheckInApplicableOn = booking.EarlyCheckInApplicableOn,
                                                                EarlyCheckInFromHour = booking.EarlyCheckInFromHour,
                                                                EarlyCheckInToHour = booking.EarlyCheckInToHour,
                                                                EarlyCheckInCharges = booking.EarlyCheckInCharges,
                                                                IsLateCheckOut = booking.IsLateCheckOut,
                                                                LateCheckOutPolicyName = booking.LateCheckOutPolicyName,
                                                                LateCheckOutDeductionBy = booking.LateCheckOutDeductionBy,
                                                                LateCheckOutApplicableOn = booking.LateCheckOutApplicableOn,
                                                                LateCheckOutFromHour = booking.LateCheckOutFromHour,
                                                                LateCheckOutToHour = booking.LateCheckOutToHour,
                                                                LateCheckOutCharges = booking.LateCheckOutCharges,
                                                                DiscountType = booking.DiscountType,
                                                                DiscountPercentage = booking.DiscountPercentage,
                                                                DiscountAmount = booking.DiscountAmount,
                                                                DiscountTotalAmount = booking.DiscountTotalAmount,
                                                                BookingAmountWithoutDiscount = booking.BookingAmountWithoutDiscount,
                                                                CalculateRoomRates = booking.CalculateRoomRates,
                                                                TransactionCharges = booking.TransactionCharges,
                                                                IsSelectedValue = true,
                                                                CheckOutDiscountType = Constants.Constants.DeductionByPercentage,
                                                                CheckOutDiscountPercentage = 0,
                                                                CheckOutDiscoutAmount = 0,
                                                                // NotMapped fields
                                                                RoomTypeName = roomType.Type,
                                                                RoomNo = rooms.RoomNo,
                                                                BookedRoomRates = (from rates in _context.BookedRoomRates
                                                                                   join type in _context.RoomCategoryMaster on rates.RoomTypeId equals type.Id
                                                                                   where rates.IsActive == true && rates.CompanyId == companyId && rates.BookingId == booking.BookingId
                                                                                   select new BookedRoomRate
                                                                                   {
                                                                                       Id = rates.Id,
                                                                                       BookingId = rates.BookingId,
                                                                                       RoomId = rates.RoomId,
                                                                                       ReservationNo = rates.ReservationNo,
                                                                                       RoomRate = rates.RoomRate,
                                                                                       GstPercentage = rates.GstPercentage,
                                                                                       GstAmount = rates.GstAmount,
                                                                                       TotalRoomRate = rates.TotalRoomRate,
                                                                                       GstType = rates.GstType,
                                                                                       BookingDate = rates.BookingDate,
                                                                                       CreatedDate = rates.CreatedDate,
                                                                                       UpdatedDate = rates.UpdatedDate,
                                                                                       UserId = rates.UserId,
                                                                                       CompanyId = rates.CompanyId,
                                                                                       IsActive = rates.IsActive,
                                                                                       CGST = rates.CGST,
                                                                                       CGSTAmount = rates.CGSTAmount,
                                                                                       SGST = rates.SGST,
                                                                                       SGSTAmount = rates.SGSTAmount,
                                                                                       DiscountType = rates.DiscountType,
                                                                                       DiscountPercentage = rates.DiscountPercentage,
                                                                                       DiscountAmount = rates.DiscountAmount,
                                                                                       RoomRateWithoutDiscount = rates.RoomRateWithoutDiscount,
                                                                                       RoomTypeId = rates.RoomTypeId,
                                                                                       RoomTypeName = type.Type
                                                                                   }).ToList(),
                                                                GuestDetails = guest,
                                                                AdvanceServices = _context.AdvanceServices.Where(x => x.IsActive == true && x.BookingId == booking.BookingId).ToList(),
                                                            }).ToListAsync();





                //var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && bookingIdList.Contains(x.BookingId)).ToListAsync();
                response.PaymentDetails = await (from pay in _context.PaymentDetails
                                                 join room in _context.RoomMaster on pay.RoomId equals room.RoomId into rooms
                                                 from roommaster in rooms.DefaultIfEmpty()
                                                 where pay.IsActive == true && pay.IsReceived == false && pay.PaymentLeft > 0 && pay.CompanyId == companyId && pay.ReservationNo == request.ReservationDetails.ReservationNo
                                                 select new PaymentDetails
                                                 {
                                                     PaymentId = pay.PaymentId,
                                                     BookingId = pay.BookingId,
                                                     ReservationNo = pay.ReservationNo,
                                                     PaymentDate = pay.PaymentDate,
                                                     PaymentMethod = pay.PaymentMethod,
                                                     TransactionId = pay.TransactionId,
                                                     PaymentStatus = pay.PaymentStatus,
                                                     PaymentType = pay.PaymentType,
                                                     BankName = pay.BankName,
                                                     PaymentReferenceNo = pay.PaymentReferenceNo,
                                                     PaidBy = pay.PaidBy,
                                                     Remarks = pay.Remarks,
                                                     Other1 = pay.Other1,
                                                     Other2 = pay.Other2,
                                                     IsActive = pay.IsActive,
                                                     IsReceived = pay.IsReceived,
                                                     RoomId = pay.RoomId,
                                                     UserId = pay.UserId,
                                                     PaymentFormat = pay.PaymentFormat,
                                                     RefundAmount = pay.RefundAmount,
                                                     PaymentAmount = pay.PaymentAmount,
                                                     PaymentLeft = pay.PaymentLeft,
                                                     CreatedDate = pay.CreatedDate,
                                                     UpdatedDate = pay.UpdatedDate,
                                                     CompanyId = pay.CompanyId,
                                                     RoomNo = roommaster != null ? roommaster.RoomNo : "",
                                                     TransactionAmount = pay.TransactionAmount,
                                                     TransactionType = pay.TransactionType,
                                                     TransactionCharges = pay.TransactionCharges,
                                                     IsEditable = pay.PaymentAmount != pay.PaymentLeft ? false : true
                                                 }).ToListAsync();


                List<Decimal> equallyDivide = new List<Decimal>();
                if (request.CheckOutDiscountType == Constants.Constants.DeductionByAmount)
                {
                   equallyDivide = EquallyDivideAmount(request.CheckOutDiscount, bookings.Count);
                }

                foreach (var (item, index) in bookings.Select((item, index) => (item, index)))
                {
                    if (!(request.Bookings.TryGetValue(item.BookingId, out DateTime value)))
                    {

                        return Ok(new { Code = 400, Message = "Invalid data" });
                    }
                    //set checkout date
                    (item.CheckOutDate, item.CheckOutTime) = DateTimeMethod.GetDateTime(value);

                    item.CheckOutDateTime = DateTimeMethod.ConvertToDateTime(item.CheckOutDate, item.CheckOutTime);

                    if(item.CheckoutFormat == Constants.Constants.SameDayFormat)
                    {
                        item.NoOfHours = Constants.Calculation.CalculateHour(item.ReservationDate, item.ReservationTime, item.CheckOutDate, item.CheckOutTime);

                        //find rates for same day
                        var (code, message, ratesResponse) = await CalculateRoomRateAsync(companyId, item.RoomTypeId, item.ReservationDate, item.CheckOutDate, 1, item.NoOfNights, item.GstType, item.NoOfHours, item.ReservationTime, item.CheckOutTime, item.DiscountType, item.DiscountType == Constants.Constants.DeductionByPercentage ? item.DiscountPercentage : equallyDivide[index], item.CheckoutFormat, item.CalculateRoomRates, property);
                        if(code == 200)
                        {
                            item.BookingAmount = ratesResponse.BookingAmount;
                            item.GstAmount = ratesResponse.GstAmount;
                            item.TotalBookingAmount = ratesResponse.TotalBookingAmount;
                            item.BookedRoomRates = ratesResponse.BookedRoomRates;
                        }
                        else
                        {
                            return Ok(new { Code = code, Message = message });
                        }
                    }
                    else
                    {
                        item.NoOfNights = Constants.Calculation.FindNightsAndHours(item.ReservationDate, item.ReservationTime, item.CheckOutDate, item.CheckOutTime, item.CheckoutFormat);
                        item.BookingAmount = 0;
                        item.GstAmount = 0;
                        item.TotalBookingAmount = 0;
                        item.TotalAmount = 0;


                        var eachRoomRate = item.BookedRoomRates.Where(x => x.BookingId == item.BookingId && (item.NoOfNights == 1
                        ? x.BookingDate == item.CheckInDate : x.BookingDate < item.CheckOutDate)).OrderBy(x => x.BookingDate).ToList();



                        item.BookedRoomRates = eachRoomRate;

                        foreach (var rate in eachRoomRate)
                        {
                            item.BookingAmount = item.BookingAmount + rate.RoomRate;
                            item.GstAmount = item.GstAmount + rate.GstAmount;
                            item.TotalBookingAmount = item.TotalBookingAmount + rate.TotalRoomRate;

                        }

                        // calculate late checkout

                        //check late check out
                        if (property.IsDefaultCheckOutTimeApplicable && property.IsLateCheckOutPolicyEnable && item.CheckoutFormat == Constants.Constants.NightFormat)
                        {


                            int differenceHours = DateTimeMethod.FindLateCheckOutHourDifference(property.CheckOutTime, item.CheckOutTime);

                            var extraPolicy = await _context.ExtraPolicies.Where(x => x.IsActive == true && x.Status == Constants.Constants.LATECHECKOUT).ToListAsync();
                            if (extraPolicy.Count == 0)
                            {
                                return Ok(new { Code = 400, Message = "Late check out policies not found", data = response });
                            }

                            if (differenceHours > 0)
                            {
                                var applicablePolicy = extraPolicy.FirstOrDefault(x => x.FromHour <= differenceHours && x.ToHour > differenceHours);
                                if (applicablePolicy == null)
                                {
                                    return Ok(new { Code = 400, Message = "Suitable late checkout policy not found", data = response });
                                }
                                else
                                {
                                    item.IsLateCheckOut = true;
                                    item.LateCheckOutPolicyName = applicablePolicy.PolicyName;
                                    item.LateCheckOutDeductionBy = applicablePolicy.DeductionBy;
                                    item.LateCheckOutApplicableOn = applicablePolicy.ChargesApplicableOn;
                                    item.LateCheckOutFromHour = applicablePolicy.FromHour;
                                    item.LateCheckOutToHour = applicablePolicy.ToHour;
                                    if (applicablePolicy.DeductionBy == Constants.Constants.DeductionByAmount)
                                    {

                                        item.LateCheckOutCharges = applicablePolicy.Amount;
                                    }
                                    else
                                    {
                                        if (applicablePolicy.ChargesApplicableOn == Constants.Constants.ChargesOnTotalAmount)
                                        {
                                            item.LateCheckOutCharges = Constants.Calculation.CalculatePercentage(item.TotalBookingAmount, applicablePolicy.Amount);

                                        }
                                        else
                                        {
                                            item.LateCheckOutCharges = Constants.Calculation.CalculatePercentage(item.BookingAmount, applicablePolicy.Amount);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                item.IsLateCheckOut = false;
                                item.LateCheckOutPolicyName = "";
                                item.LateCheckOutDeductionBy = "";
                                item.LateCheckOutApplicableOn = "";
                                item.LateCheckOutFromHour = 0;
                                item.LateCheckOutToHour = 0;
                                item.LateCheckOutCharges = 0;
                            }
                        }
                    }


                        

                    item.TotalAmountWithOutDiscount = BookingCalulation.BookingTotalAmount(item);

                    if(request.CheckOutDiscount > 0)
                    {
                        if(request.CheckOutDiscountType == Constants.Constants.DeductionByPercentage)
                        {
                            item.CheckOutDiscountType = request.CheckOutDiscountType;
                            item.CheckOutDiscountPercentage = request.CheckOutDiscount;
                            item.CheckOutDiscoutAmount = Constants.Calculation.CalculatePercentage(item.TotalAmountWithOutDiscount, item.CheckOutDiscountPercentage);
                        }
                        else
                        {
                            item.CheckOutDiscountType = request.CheckOutDiscountType;
                            
                            item.CheckOutDiscoutAmount = equallyDivide[index];
                        }
                    }
                    

                    item.TotalAmount = BookingCalulation.BookingTotalAmount(item);
                }

                PaymentCheckOutSummary paymentSummary = await CalculateSummaryForCheckOut(request.ReservationDetails, bookings, response.PaymentDetails, isAllRoomCheckOut);
                paymentSummary.CheckOutDiscountType = request.CheckOutDiscountType;
                paymentSummary.CheckOutDiscountPercentage = request.CheckOutDiscountType == Constants.Constants.DeductionByPercentage ? request.CheckOutDiscount : 0;
                response.BookingDetails = bookings;
                response.PaymentSummary = paymentSummary;




                CalculateInvoice(response.BookingDetails, response.PaymentDetails, Constants.Constants.CheckOut);

               


                foreach (var item in response.BookingDetails)
                {
                    item.BalanceAmount = CalculateCheckOutBalanceBooking(item);
                }

                
                //calculate refund               

                if (isAllRoomCheckOut)
                {
                    SetRefundAmount(response.PaymentDetails, response.BookingDetails);
                }
                else
                {
                    SetResidualAmount(response.PaymentDetails, response.BookingDetails);

                   
                }


                foreach (var item in response.BookingDetails)
                {

                    response.PaymentSummary.BalanceAmount += item.BalanceAmount;
                    response.PaymentSummary.RefundAmount += item.RefundAmount;
                    response.PaymentSummary.ResidualAmount += item.ResidualAmount;
                }

                return Ok(new { Code = 200, Message = "Rates fetched successfully", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        //Set agent service charge
        private void SetAgentServiceCharge(List<BookingDetail> bookings,decimal agentcharge, int roomCounts)
        {
            if(roomCounts > 0)
            {
                List<decimal> equallyDivideArr = EquallyDivideAmount(agentcharge, roomCounts);
                foreach (var item in bookings)
                {
                    item.AgentServiceCharge = equallyDivideArr[0];
                }
            }
           
        }
        
        
        //INVOICE CALCULATION
        private void CalculateInvoice(List<BookingDetail> bookings, List<PaymentDetails> payments, string status)
        {
            //set room payment if room wise payment
            foreach (var booking in bookings)
            {
                foreach (var pay in payments)
                {
                    if (pay.RoomId == booking.RoomId && pay.BookingId == booking.BookingId)
                    {
                        
                        decimal balance = status == Constants.Constants.CheckOut ?  CalculateCheckOutBalanceBooking(booking) : CalculateBalanceCancelAmount(booking);
                        //amount left in room
                        if (balance > 0)
                        {
                            // payment left is less than balance
                            if(balance >= pay.PaymentLeft)
                            {
                                booking.ReceivedAmount = booking.ReceivedAmount + pay.PaymentLeft;

                                pay.InvoiceHistories.Add(CreateInvoiceHistory(booking, pay, pay.PaymentLeft));

                                pay.RefundAmount = 0;
                                pay.PaymentLeft = 0;
                                pay.IsReceived = true;

                            }
                            //payment left is greater than balance
                            else
                            {
                                booking.ReceivedAmount = booking.ReceivedAmount + balance;
                                booking.RefundAmount = booking.RefundAmount + (pay.PaymentLeft - balance);

                                pay.RefundAmount = pay.PaymentLeft - balance;
                                pay.IsReceived = true;
                                pay.PaymentLeft = 0;
                                pay.InvoiceHistories.Add(CreateInvoiceHistory(booking, pay, balance));
                            }
                         
                            
                        }
                        else
                        {
                            pay.RefundAmount = pay.PaymentLeft;
                            pay.PaymentLeft = 0;
                            pay.IsReceived = true;
                            booking.RefundAmount = booking.RefundAmount +  pay.RefundAmount;
                            pay.InvoiceHistories.Add(CreateInvoiceHistory(booking, pay, 0));
                        }

                    }
                }
            }

            //calculate advance
            decimal agentAdvance = 0;
            decimal advanceAmount = 0;
            decimal receivedAmount = 0;
            (agentAdvance, advanceAmount, receivedAmount) = CalculatePayment(payments, bookings);

            //agent advance allocation
            if (agentAdvance > 0)
            {
                int roomCounts = status == Constants.Constants.CheckOut ?  GetBalanceRoomCount(bookings) : GetBalanceCancelCount(bookings)
;
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(agentAdvance, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = status == Constants.Constants.CheckOut ?  CalculateCheckOutBalanceBooking(bookings[i]) : CalculateBalanceCancelAmount(bookings[i]);
                    if (balance > 0)
                    {
                        decimal currentBalance = 0;
                        if (balance >= equallyDivideArr[divideArrIndex])
                        {
                            currentBalance = equallyDivideArr[divideArrIndex];
                        }
                        else
                        {
                            currentBalance = balance;
                            if (divideArrIndex < equallyDivideArr.Count - 1)
                            {
                                equallyDivideArr[divideArrIndex + 1] += (equallyDivideArr[divideArrIndex] - balance);

                            }
                        }

                        bookings[i].AgentAdvanceAmount += currentBalance;

                        //assign payment
                        while (paymentIndex != payments.Count && currentBalance != 0)
                        {
                            if (!IsAgentPaymentLeft(payments, paymentIndex))
                            {
                                paymentIndex++;
                            }
                            else
                            {
                                //payment     
                                if (payments[paymentIndex].PaymentLeft >= currentBalance)
                                {
                                    payments[paymentIndex].PaymentLeft -= currentBalance;
                                    bookings[i].AgentAdvanceAmount += currentBalance;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], currentBalance));
                                    currentBalance = 0;
                                }
                                else
                                {
                                    bookings[i].AgentAdvanceAmount += payments[paymentIndex].PaymentLeft;
                                    currentBalance = currentBalance - payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].PaymentLeft = 0;
                                    payments[paymentIndex].IsReceived = true;
                                    
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], payments[paymentIndex].PaymentLeft));
                                }
                                paymentIndex++;
                            }

                        }
                        divideArrIndex++;
                    }


                }

            }

            //advance allocation
            if (advanceAmount > 0)
            {
                int roomCounts = status == Constants.Constants.CheckOut ?  GetBalanceRoomCount(bookings) : GetBalanceCancelCount(bookings);
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(advanceAmount, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = status == Constants.Constants.CheckOut ? CalculateCheckOutBalanceBooking(bookings[i]) : CalculateBalanceCancelAmount(bookings[i]);
                    if (balance > 0)
                    {
                        decimal currentBalance = 0;
                        if (balance >= equallyDivideArr[divideArrIndex])
                        {
                            currentBalance = equallyDivideArr[divideArrIndex];
                        }
                        else
                        {
                            currentBalance = balance;
                            if (divideArrIndex < equallyDivideArr.Count - 1)
                            {
                                equallyDivideArr[divideArrIndex + 1] += (equallyDivideArr[divideArrIndex] - balance);

                            }
                        }



                        //assign payment
                        while (paymentIndex != payments.Count && currentBalance != 0)
                        {
                            if (!IsAdvancePaymentLeft(payments, paymentIndex))
                            {
                                paymentIndex++;
                            }
                            else
                            {
                                //payment

                                if (payments[paymentIndex].PaymentLeft >= currentBalance)
                                {
                                    payments[paymentIndex].PaymentLeft -= currentBalance;
                                    bookings[i].AdvanceAmount += currentBalance;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], currentBalance));
                                    currentBalance = 0;
                                }
                                else
                                {
                                    bookings[i].AdvanceAmount += payments[paymentIndex].PaymentLeft;
                                    currentBalance = currentBalance - payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].PaymentLeft = 0;
                                    payments[paymentIndex].IsReceived = true;
                                    
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], payments[paymentIndex].PaymentLeft));
                                }

                                paymentIndex++;
                            }
                        }
                        divideArrIndex++;
                    }


                }

            }

            //receive amount
            if (receivedAmount > 0)
            {
                int roomCounts = status == Constants.Constants.CheckOut ?  GetBalanceRoomCount(bookings) : GetBalanceCancelCount(bookings);
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(receivedAmount, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = status == Constants.Constants.CheckOut ? CalculateCheckOutBalanceBooking(bookings[i]) : CalculateBalanceCancelAmount(bookings[i]);
                    if (balance > 0)
                    {
                        decimal currentBalance = 0;
                        if (balance >= equallyDivideArr[divideArrIndex])
                        {
                            currentBalance = equallyDivideArr[divideArrIndex];
                        }
                        else
                        {
                            currentBalance = balance;
                            if (divideArrIndex < equallyDivideArr.Count - 1)
                            {
                                equallyDivideArr[divideArrIndex + 1] += (equallyDivideArr[divideArrIndex] - balance);

                            }
                        }



                        //assign payment
                        while (paymentIndex != payments.Count && currentBalance != 0)
                        {
                            if (!IsReceivedPaymentLeft(payments, paymentIndex))
                            {
                                paymentIndex++;
                            }
                            else
                            {
                                //payment     
                                if (payments[paymentIndex].PaymentLeft > currentBalance)
                                {
                                    payments[paymentIndex].PaymentLeft -= currentBalance;
                                    bookings[i].ReceivedAmount += currentBalance;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], currentBalance));
                                    currentBalance = 0;
                                }
                                else
                                {
                                    bookings[i].ReceivedAmount += payments[paymentIndex].PaymentLeft;
                                    currentBalance = currentBalance - payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].PaymentLeft = 0;
                                    payments[paymentIndex].IsReceived = true;
                                    
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], payments[paymentIndex].PaymentLeft));
                                }
                                paymentIndex++;
                            }
                        }
                        divideArrIndex++;
                    }


                }

            }


        }

        private decimal CalculateCheckOutBalanceBooking(BookingDetail booking)
        {
            return (booking.TotalAmount - (booking.AgentAdvanceAmount + booking.AdvanceAmount + booking.ReceivedAmount));
        }

        private int GetBalanceRoomCount(List<BookingDetail> bookings)
        {
            int count = 0;
            foreach (var item in bookings)
            {
                decimal balance = CalculateCheckOutBalanceBooking(item);
                if (balance > 0)
                {
                    count++;
                }
            }
            return count;
        }

        [HttpPost("UpdateRoomsCheckOut")]
        public async Task<IActionResult> UpdateRoomsCheckOut([FromBody] CheckOutResponse request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            List<CheckOutNotificationDTO> notificationDTOs = new List<CheckOutNotificationDTO>();
            try
            {
                
                string financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();
                var currentTime = DateTime.Now;

                if (request.BookingDetails.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "No rooms selected for checkout" });
                }
                if (string.IsNullOrWhiteSpace(request.ReservationDetails.ReservationNo))
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Reservation No not found" });
                }
                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if (property == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Property not found" });
                }

                foreach (var item in request.BookingDetails)
                {
                    //if (!IsTodayCheckOutDate(item.CheckOutDate))
                    //{
                    //    await transaction.RollbackAsync();
                    //    return Ok(new { Code = 400, Message = "Check Out Date is not equal to today's date" });
                    //}

                    if(item.CheckoutFormat == Constants.Constants.SameDayFormat)
                    {
                        var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();

                        _context.BookedRoomRates.RemoveRange(roomRates);

                        foreach(var rate in item.BookedRoomRates)
                        {
                            rate.BookingId = item.BookingId;
                            rate.RoomId = item.RoomId;
                            rate.ReservationNo = item.ReservationNo;
                            Constants.Constants.SetMastersDefault(rate, companyId, userId, currentTime);
                            await _context.BookedRoomRates.AddAsync(rate);
                        }
                    }
                    else
                    {
                        var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();

                        int noOfnights = Constants.Calculation.FindNightsAndHours(item.ReservationDate, item.ReservationTime, item.CheckOutDate, item.CheckOutTime, item.CheckoutFormat);

                        foreach (var rate in roomRates)
                        {
                            if(noOfnights == 1)
                            {
                                
                            }

                            if (noOfnights == 1 ? rate.BookingDate == item.CheckInDate : rate.BookingDate < item.CheckOutDate)
                            {
                            }
                            else
                            {
                                rate.IsActive = false;
                                rate.UpdatedDate = currentTime;
                                _context.BookedRoomRates.Update(rate);

                            }


                        }
                    }

                    //item.TotalAmount = Constants.Calculation.BookingTotalAmount(item);
                }


                foreach (var item in request.BookingDetails)
                {
                    CheckOutNotificationDTO inNotificationDTO = new CheckOutNotificationDTO();

                    var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId && x.Status == Constants.Constants.CheckIn);
                    if (booking == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Booking not found" });
                    }


                    booking.CheckOutDate = item.CheckOutDate;
                    booking.CheckOutTime = item.CheckOutTime;
                    booking.CheckOutDateTime = DateTimeMethod.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);

                    booking.AgentServiceCharge = item.AgentServiceCharge;

                    booking.IsLateCheckOut = item.IsLateCheckOut;
                    booking.LateCheckOutDeductionBy = item.LateCheckOutDeductionBy;
                    booking.LateCheckOutApplicableOn = item.LateCheckOutApplicableOn;
                    booking.LateCheckOutFromHour = item.LateCheckOutFromHour;
                    booking.LateCheckOutToHour = item.LateCheckOutToHour;
                    booking.LateCheckOutPolicyName = item.LateCheckOutPolicyName;
                    booking.LateCheckOutCharges = item.LateCheckOutCharges;
                    //booking.NoOfHours = Constants.Calculation.CalculateHour(booking.ReservationDate, booking.ReservationTime, booking.CheckOutDate, booking.CheckOutTime);
                    booking.NoOfNights = Constants.Calculation.FindNightsAndHours(booking.ReservationDate, booking.ReservationTime, booking.CheckOutDate, booking.CheckOutTime, booking.CheckoutFormat);
                    booking.Status = Constants.Constants.CheckOut;
                    booking.UpdatedDate = currentTime;
                    booking.InitialBalanceAmount = item.BalanceAmount;
                    booking.BalanceAmount = item.BalanceAmount;
                    booking.RefundAmount = item.RefundAmount;
                    booking.ResidualAmount = item.ResidualAmount;
                    booking.AdvanceAmount = item.AdvanceAmount;
                    booking.AgentAdvanceAmount = item.AgentAdvanceAmount;
                    booking.ReceivedAmount = item.ReceivedAmount;
                    booking.InvoiceNo = request.InvoiceNo;
                    booking.InvoiceDate = request.InvoiceDate;
                    booking.InvoiceName = request.InvoiceName;
                    booking.BookingAmount = item.BookingAmount;
                    booking.GstAmount = item.GstAmount;
                    booking.TotalBookingAmount = item.TotalBookingAmount;
                    booking.TotalAmount = item.TotalAmount;
                    booking.TotalAmountWithOutDiscount = item.TotalAmountWithOutDiscount;
                    booking.BillTo = item.BillTo;
                    booking.CheckOutDiscountType = request.CheckOutDiscount > 0 ?  item.CheckOutDiscountType : "";
                    booking.CheckOutDiscountPercentage = item.CheckOutDiscountPercentage;
                    booking.CheckOutDiscoutAmount = item.CheckOutDiscoutAmount;
                    booking.CheckOutInvoiceFormat = property.CheckOutInvoice;
                    _context.BookingDetail.Update(booking);

                    inNotificationDTO.RoomNo = await _context.RoomMaster
                                                     .Where(x => x.RoomId == booking.RoomId)
                                                     .Select(x => x.RoomNo)
                                                     .FirstOrDefaultAsync() ?? "";
                    var guestDetails = await _context.GuestDetails
                                                .Where(x => x.GuestId == booking.GuestId)

                                                .FirstOrDefaultAsync();
                    inNotificationDTO.GuestName = guestDetails.GuestName ?? "";
                    inNotificationDTO.GuestPhoneNo = guestDetails.PhoneNumber ?? "";
                    inNotificationDTO.GuestEmail = guestDetails.Email ?? "";
                    inNotificationDTO.Pax = booking.Pax;

                    inNotificationDTO.CheckInDateTime = booking.CheckInDateTime.ToString("f");
                    inNotificationDTO.CheckOutDateTime = booking.CheckOutDateTime.ToString("f");
                    inNotificationDTO.RoomType = await _context.RoomCategoryMaster.Where(x => x.Id == booking.RoomTypeId).Select(x => x.Type).FirstOrDefaultAsync() ?? "";
                    inNotificationDTO.ReservationNo = booking.ReservationNo;
                    notificationDTOs.Add(inNotificationDTO);



                    var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId && x.RoomStatus == Constants.Constants.CheckIn);
                    if (roomAvailability == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Room availability not found" });
                    }
                    roomAvailability.CheckOutDate = booking.CheckOutDate;
                    roomAvailability.CheckOutTime = booking.CheckOutTime;
                    roomAvailability.CheckOutDateTime = DateTimeMethod.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);
                    roomAvailability.RoomStatus = Constants.Constants.Dirty;
                    roomAvailability.UpdatedDate = currentTime;
                    _context.RoomAvailability.Update(roomAvailability);

                }

                await _context.SaveChangesAsync();

                foreach (var pay in request.PaymentDetails)
                {
                    var payment = await _context.PaymentDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.PaymentId == pay.PaymentId);
                    if (payment == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Payment not found" });
                    }
                    payment.PaymentLeft = pay.PaymentLeft;
                    payment.IsReceived = pay.IsReceived;
                    payment.RefundAmount = pay.RefundAmount;
                    foreach (var invoice in pay.InvoiceHistories)
                    {
                        Constants.Constants.SetMastersDefault(invoice, companyId, userId, currentTime);
                        invoice.InvoiceDate = request.InvoiceDate;
                        invoice.InvoiceNo = request.InvoiceNo;
                        await _context.InvoiceHistory.AddAsync(invoice);
                    }
                    _context.PaymentDetails.Update(payment);

                }


                string result = await DocumentHelper.UpdateDocumentNo(_context,Constants.Constants.DocumentInvoice, companyId, financialYear);
                if (result == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Error while updating document" });
                }


                if (property.IsEmailNotification)
                {
                    CheckOutEmailNotification outEmailNotification = new CheckOutEmailNotification(_context, notificationDTOs, companyId, property);

                    await outEmailNotification.SendEmail();
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Bookings Checkout successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        

        [HttpGet("GetRoomsById")]
        public async Task<IActionResult> GetRoomsById(int bookingId, int roomId)
        {
            try
            {
               
               
                if (bookingId == 0)
                {
                    return Ok(new { code = 400, Message = "Invalid data" });
                }

                var BookingDetail = await (from booking in _context.BookingDetail
                                                join bookrooms in _context.RoomMaster on booking.RoomId equals bookrooms.RoomId
                                                join guest in _context.GuestDetails on booking.GuestId equals guest.GuestId
                                                join category in _context.RoomCategoryMaster on booking.RoomTypeId equals category.Id
                                                where booking.IsActive == true && booking.CompanyId == companyId && booking.BookingId == bookingId
                                                select new BookingDetail
                                                {
                                                    BookingId = booking.BookingId,
                                                    GuestId = booking.GuestId,
                                                    RoomId = booking.RoomId,
                                                    RoomTypeId = booking.RoomTypeId,
                                                    CheckInDate = booking.CheckInDate,
                                                    CheckInTime = booking.CheckInTime,
                                                    CheckOutDate = booking.CheckOutDate,
                                                    CheckOutTime = booking.CheckOutTime,
                                                    CheckInDateTime = booking.CheckInDateTime,
                                                    CheckOutDateTime = booking.CheckOutDateTime,
                                                    CheckoutFormat = booking.CheckoutFormat,
                                                    NoOfNights = booking.NoOfNights,
                                                    NoOfHours = booking.NoOfHours,
                                                    RoomCount = booking.RoomCount,
                                                    Pax = booking.Pax,
                                                    Status = booking.Status,
                                                    Remarks = booking.Remarks,
                                                    ReservationNo = booking.ReservationNo,
                                                    BookingDate = booking.BookingDate,
                                                    CreatedDate = booking.CreatedDate,
                                                    UpdatedDate = booking.UpdatedDate,
                                                    IsActive = booking.IsActive,
                                                    PrimaryGuestId = booking.PrimaryGuestId,
                                                    InitialBalanceAmount = booking.InitialBalanceAmount,
                                                    BalanceAmount = booking.BalanceAmount,
                                                    AdvanceAmount = booking.AdvanceAmount,
                                                    ReceivedAmount = booking.ReceivedAmount,
                                                    AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                                    RefundAmount = booking.RefundAmount,
                                                    InvoiceDate = booking.InvoiceDate,
                                                    InvoiceNo = booking.InvoiceNo,
                                                    UserId = booking.UserId,
                                                    GstType = booking.GstType,
                                                    CompanyId = booking.CompanyId,
                                                    BookingAmount = booking.BookingAmount,
                                                    GstAmount = booking.GstAmount,
                                                    TotalBookingAmount = booking.TotalBookingAmount,
                                                    BookingSource = booking.BookingSource,
                                                    ReservationDate = booking.ReservationDate,
                                                    ReservationTime = booking.ReservationTime,
                                                    ReservationDateTime = booking.ReservationDateTime,
                                                    InitialCheckOutDate = booking.InitialCheckOutDate,
                                                    InitialCheckOutTime = booking.InitialCheckOutTime,
                                                    InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                                    ServicesAmount = booking.ServicesAmount,
                                                    TotalAmount = booking.TotalAmount,
                                                    TotalAmountWithOutDiscount = booking.TotalAmountWithOutDiscount,
                                                    AgentAdvanceAmount = booking.AgentAdvanceAmount,
                                                    InvoiceName = booking.InvoiceName,
                                                    BillTo = booking.BillTo,
                                                    TotalServicesAmount = booking.TotalServicesAmount,
                                                    ServicesTaxAmount = booking.ServicesTaxAmount,
                                                    CancelAmount = booking.CancelAmount,
                                                    CancelDate = booking.CancelDate,
                                                    CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                                    IsEarlyCheckIn = booking.IsEarlyCheckIn,
                                                    EarlyCheckInPolicyName = booking.EarlyCheckInPolicyName,
                                                    EarlyCheckInDeductionBy = booking.EarlyCheckInDeductionBy,
                                                    EarlyCheckInApplicableOn = booking.EarlyCheckInApplicableOn,
                                                    EarlyCheckInFromHour = booking.EarlyCheckInFromHour,
                                                    EarlyCheckInToHour = booking.EarlyCheckInToHour,
                                                    EarlyCheckInCharges = booking.EarlyCheckInCharges,
                                                    IsLateCheckOut = booking.IsLateCheckOut,
                                                    LateCheckOutPolicyName = booking.LateCheckOutPolicyName,
                                                    LateCheckOutDeductionBy = booking.LateCheckOutDeductionBy,
                                                    LateCheckOutApplicableOn = booking.LateCheckOutApplicableOn,
                                                    LateCheckOutFromHour = booking.LateCheckOutFromHour,
                                                    LateCheckOutToHour = booking.LateCheckOutToHour,
                                                    LateCheckOutCharges = booking.LateCheckOutCharges,
                                                    DiscountType = booking.DiscountType,
                                                    DiscountPercentage = booking.DiscountPercentage,
                                                    DiscountAmount = booking.DiscountAmount,
                                                    DiscountTotalAmount = booking.DiscountTotalAmount,
                                                    BookingAmountWithoutDiscount = booking.BookingAmountWithoutDiscount,
                                                    CalculateRoomRates = booking.CalculateRoomRates,
                                                    IsSelectedValue = false,
                                                    // NotMapped fields
                                                    RoomTypeName = category.Type == null ? "" : category.Type,
                                                    RoomNo = bookrooms.RoomNo == null ? "" : bookrooms.RoomNo,
                                                    GuestDetails = guest
                                                }).FirstOrDefaultAsync();

                


                if (BookingDetail == null)
                {
                    return Ok(new { Code = 400, Message = "No booking found" });
                }

                
                var response = new 
                {
                    BookingDetail = BookingDetail,
                    Hours = BookingDetail.CheckoutFormat == Constants.Constants.SameDayFormat ? await _context.HourMaster.Where(x=>x.IsActive == true && x.CompanyId == companyId).ToListAsync() : new List<HourMaster>()

                };
                return Ok(new { Code = 200, Message = "Data fetched", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        
        


        [HttpGet("GetBookingsForCancel")]
        public async Task<IActionResult> GetBookingsForCancel(string reservationNo, int guestId)
        {
            try
            {

                var cancelBookingResponse = new CancelBookingResponse();
                if (string.IsNullOrWhiteSpace(reservationNo) || guestId == 0)
                {
                    return Ok(new { Code = 500, Message = "Invalid data" });
                }
                
                var statusList = new List<string> { Constants.Constants.Pending, Constants.Constants.Confirmed, Constants.Constants.CheckIn };

                DateTime cancelDateTime = DateTime.Now;

                var getbookingno = await DocumentHelper.GetDocumentNo(_context, Constants.Constants.DocumentInvoice, companyId, financialYear);

                if (getbookingno == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found.", data = getbookingno });
                }
                cancelBookingResponse.InvoiceNo = getbookingno;

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if(property == null)
                {
                    return Ok(new { Code = 400, message = "Property not found", data = getbookingno });
                }

                cancelBookingResponse.ReservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);
                if (cancelBookingResponse.ReservationDetails == null)
                {
                    return Ok(new { Code = 400, Message = $"No details for {reservationNo} reservation" });
                }

                cancelBookingResponse.GuestDetails = await _context.GuestDetails.Where(x => x.CompanyId == companyId && x.IsActive && x.GuestId == guestId).FirstOrDefaultAsync();
                if (cancelBookingResponse.GuestDetails == null)
                {
                    return Ok(new { Code = 400, Message = $"No details for {reservationNo} reservation" });
                }


                List<BookingDetail> allBookings = await _context.BookingDetail.Where(x => x.IsActive == true && x.ReservationNo == reservationNo && x.CompanyId == companyId && x.Status != Constants.Constants.CheckOut).ToListAsync();

                
                List<BookingDetail> bookingDetails =
                     (
                                        from booking in allBookings
                                        join room in _context.RoomMaster
                                            on new { RoomId = booking.RoomId, CompanyId = companyId }
                                            equals new { RoomId = room.RoomId, CompanyId = room.CompanyId } into rooms
                                        from bookrooms in rooms.DefaultIfEmpty()
                                        join category in _context.RoomCategoryMaster
                                            on new { RoomTypeId = booking.RoomTypeId, CompanyId = companyId }
                                            equals new { RoomTypeId = category.Id, CompanyId = category.CompanyId }
                                        join guest in _context.GuestDetails
                                            on booking.PrimaryGuestId equals guest.GuestId
                                        where booking.IsActive == true
                                              && booking.CompanyId == companyId
                                              && statusList.Contains(booking.Status)
                                              && !(booking.Status == Constants.Constants.CheckIn && booking.ServicesAmount > 0)
                                              && booking.ReservationNo == reservationNo

                                        select new BookingDetail
                                        {
                                            BookingId = booking.BookingId,
                                            GuestId = booking.GuestId,
                                            RoomId = booking.RoomId,
                                            RoomTypeId = booking.RoomTypeId,
                                            CheckInDate = booking.CheckInDate,
                                            CheckInTime = booking.CheckInTime,
                                            CheckOutDate = booking.CheckOutDate,
                                            CheckOutTime = booking.CheckOutTime,
                                            CheckInDateTime = booking.CheckInDateTime,
                                            CheckOutDateTime = booking.CheckOutDateTime,
                                            CheckoutFormat = booking.CheckoutFormat,
                                            NoOfNights = booking.NoOfNights,
                                            NoOfHours = booking.NoOfHours,
                                            RoomCount = booking.RoomCount,
                                            Pax = booking.Pax,
                                            Status = booking.Status,
                                            Remarks = booking.Remarks,
                                            ReservationNo = booking.ReservationNo,
                                            BookingDate = booking.BookingDate,
                                            CreatedDate = booking.CreatedDate,
                                            UpdatedDate = booking.UpdatedDate,
                                            IsActive = booking.IsActive,
                                            PrimaryGuestId = booking.PrimaryGuestId,
                                            InitialBalanceAmount = booking.InitialBalanceAmount,
                                            BalanceAmount = booking.BalanceAmount,
                                            AdvanceAmount = booking.AdvanceAmount,
                                            ReceivedAmount = booking.ReceivedAmount,
                                            AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                            RefundAmount = booking.RefundAmount,
                                            
                                            InvoiceNo = cancelBookingResponse.InvoiceNo,
                                            UserId = booking.UserId,
                                            GstType = booking.GstType,
                                            CompanyId = booking.CompanyId,
                                            BookingAmount = booking.BookingAmount,
                                            GstAmount = booking.GstAmount,
                                            TotalBookingAmount = booking.TotalBookingAmount,
                                            BookingSource = booking.BookingSource,
                                            ReservationDate = booking.ReservationDate,
                                            ReservationTime = booking.ReservationTime,
                                            ReservationDateTime = booking.ReservationDateTime,
                                            InitialCheckOutDate = booking.InitialCheckOutDate,
                                            InitialCheckOutTime = booking.InitialCheckOutTime,
                                            InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                            ServicesAmount = booking.ServicesAmount,
                                            TotalAmount = booking.TotalAmount,
                                            TotalAmountWithOutDiscount = booking.TotalAmountWithOutDiscount,

                                            AgentAdvanceAmount = booking.AgentAdvanceAmount,
                                            InvoiceName = booking.InvoiceName,
                                            BillTo = booking.BillTo,
                                            TotalServicesAmount = booking.TotalServicesAmount,
                                            ServicesTaxAmount = booking.ServicesTaxAmount,
                                            CancelAmount = booking.CancelAmount,
                                            CancelDate = cancelDateTime,
                                            CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                            IsEarlyCheckIn = booking.IsEarlyCheckIn,
                                            EarlyCheckInPolicyName = booking.EarlyCheckInPolicyName,
                                            EarlyCheckInDeductionBy = booking.EarlyCheckInDeductionBy,
                                            EarlyCheckInApplicableOn = booking.EarlyCheckInApplicableOn,
                                            EarlyCheckInFromHour = booking.EarlyCheckInFromHour,
                                            EarlyCheckInToHour = booking.EarlyCheckInToHour,
                                            EarlyCheckInCharges = booking.EarlyCheckInCharges,
                                            IsLateCheckOut = booking.IsLateCheckOut,
                                            LateCheckOutPolicyName = booking.LateCheckOutPolicyName,
                                            LateCheckOutDeductionBy = booking.LateCheckOutDeductionBy,
                                            LateCheckOutApplicableOn = booking.LateCheckOutApplicableOn,
                                            LateCheckOutFromHour = booking.LateCheckOutFromHour,
                                            LateCheckOutToHour = booking.LateCheckOutToHour,
                                            LateCheckOutCharges = booking.LateCheckOutCharges,
                                            DiscountType = booking.DiscountType,
                                            DiscountPercentage = booking.DiscountPercentage,
                                            DiscountAmount = booking.DiscountAmount,
                                            DiscountTotalAmount = booking.DiscountTotalAmount,
                                            BookingAmountWithoutDiscount = booking.BookingAmountWithoutDiscount,
                                            CalculateRoomRates = booking.CalculateRoomRates,
                                            TransactionCharges = booking.TransactionCharges,
                                            IsSelectedValue = true,
                                           
                                            // NotMapped fields
                                            RoomTypeName = category.Type,
                                            RoomNo = bookrooms == null ?  "" : bookrooms.RoomNo,
                                            BookedRoomRates = _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList(),
                                            GuestDetails = _context.GuestDetails.FirstOrDefault(x=>x.IsActive == true && x.GuestId == booking.GuestId) ?? guest,
                                            AdvanceServices = _context.AdvanceServices.Where(x => x.IsActive == true && x.BookingId == booking.BookingId).ToList(),
                                        }
                                    ).ToList();

                //check all rooms cancel
                bookingDetails = bookingDetails.Where(x => x.AdvanceServices.Count == 0).ToList();
                cancelBookingResponse.IsAllCancel = allBookings.Count == bookingDetails.Count;

                

                List<PaymentDetails> paymentDetails = await _context.PaymentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId && x.PaymentLeft > 0 && x.ReservationNo == reservationNo).ToListAsync();


                List<CancelPolicyMaster> cancelPolicies = await _context.CancelPolicyMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.DeductionBy == property.CancelCalculatedBy).ToListAsync();


                

                bool flag = CalculateCancelAmount(bookingDetails, property.CancelMethod, cancelPolicies, cancelDateTime);
                if (flag == false)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }
                bookingDetails = bookingDetails.OrderBy(x => x.CancelAmount).ToList();
                CancelSummary cancelSummary = new CancelSummary();

                cancelBookingResponse.CancelSummary = CalculatePaymentSummary(cancelSummary, paymentDetails,bookingDetails);

               

                CalculateInvoice(bookingDetails, paymentDetails, Constants.Constants.Cancel);

                foreach (var item in bookingDetails)
                {
                    item.BalanceAmount = CalculateBalanceCancelAmount(item);
                }

               

                if (cancelBookingResponse.IsAllCancel)
                {
                    SetRefundAmount(paymentDetails, bookingDetails);
                }
                else
                {
                    SetResidualAmount(paymentDetails, bookingDetails);
                }

                cancelBookingResponse.CancelSummary = CalculateCancelSummary(cancelSummary, bookingDetails);

                cancelBookingResponse.bookingDetails = bookingDetails;
                cancelBookingResponse.PaymentDetails = paymentDetails;


                return Ok(new { Code = 200, Message = "Bookings fetched successfully", data = cancelBookingResponse });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("CalculateAmountForCancel")]
        public async Task<IActionResult> CalculateAmountForCancel([FromBody] CalculateCancelAmountRequest request)
        {
            try
            {

                var cancelBookingResponse = new CancelBookingResponse();
                cancelBookingResponse.CancelDate = request.CancelDate;
                if (string.IsNullOrWhiteSpace(request.ReservationNo) || request.CancelDate == null || request.CancelDate == new DateTime(1900, 01, 01))
                {
                    return Ok(new { Code = 500, Message = "Invalid data" });
                }

                if (request.BookingIds.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "Bookings fetched successfully", data = cancelBookingResponse });
                }

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if (property == null)
                {
                    return Ok(new { Code = 400, message = "Property not found"});
                }

                var statusList = new List<string> { Constants.Constants.Pending, Constants.Constants.Confirmed, Constants.Constants.CheckIn };


                List<BookingDetail> allbookingDetails = await _context.BookingDetail.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == request.ReservationNo && x.Status != Constants.Constants.CheckOut).ToListAsync();

                List<BookingDetail> bookingDetails =
                   
                                 (    from booking in allbookingDetails
                                        join room in _context.RoomMaster
                                            on new { RoomId = booking.RoomId, CompanyId = companyId }
                                            equals new { RoomId = room.RoomId, CompanyId = room.CompanyId } into rooms
                                        from bookrooms in rooms.DefaultIfEmpty()
                                        join guest in _context.GuestDetails
                                            on booking.PrimaryGuestId equals guest.GuestId
                                        join category in _context.RoomCategoryMaster
                                            on new { RoomTypeId = booking.RoomTypeId, CompanyId = companyId }
                                            equals new { RoomTypeId = category.Id, CompanyId = category.CompanyId }

                                        where booking.IsActive == true
                                              && booking.CompanyId == companyId
                                              && statusList.Contains(booking.Status)
                                              && !(booking.Status == Constants.Constants.CheckIn || booking.ServicesAmount > 0)
                                              && booking.ReservationNo == request.ReservationNo
                                              && request.BookingIds.Contains(booking.BookingId)

                                      select new BookingDetail
                                      {
                                          BookingId = booking.BookingId,
                                          GuestId = booking.GuestId,
                                          RoomId = booking.RoomId,
                                          RoomTypeId = booking.RoomTypeId,
                                          CheckInDate = booking.CheckInDate,
                                          CheckInTime = booking.CheckInTime,
                                          CheckOutDate = booking.CheckOutDate,
                                          CheckOutTime = booking.CheckOutTime,
                                          CheckInDateTime = booking.CheckInDateTime,
                                          CheckOutDateTime = booking.CheckOutDateTime,
                                          CheckoutFormat = booking.CheckoutFormat,
                                          NoOfNights = booking.NoOfNights,
                                          NoOfHours = booking.NoOfHours,
                                          RoomCount = booking.RoomCount,
                                          Pax = booking.Pax,
                                          Status = booking.Status,
                                          Remarks = booking.Remarks,
                                          ReservationNo = booking.ReservationNo,
                                          BookingDate = booking.BookingDate,
                                          CreatedDate = booking.CreatedDate,
                                          UpdatedDate = booking.UpdatedDate,
                                          IsActive = booking.IsActive,
                                          PrimaryGuestId = booking.PrimaryGuestId,
                                          InitialBalanceAmount = booking.InitialBalanceAmount,
                                          BalanceAmount = booking.BalanceAmount,
                                          AdvanceAmount = booking.AdvanceAmount,
                                          ReceivedAmount = booking.ReceivedAmount,
                                          AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                          RefundAmount = booking.RefundAmount,

                                          InvoiceNo = request.InvoiceNo,
                                          UserId = booking.UserId,
                                          GstType = booking.GstType,
                                          CompanyId = booking.CompanyId,
                                          BookingAmount = booking.BookingAmount,
                                          GstAmount = booking.GstAmount,
                                          TotalBookingAmount = booking.TotalBookingAmount,
                                          BookingSource = booking.BookingSource,
                                          ReservationDate = booking.ReservationDate,
                                          ReservationTime = booking.ReservationTime,
                                          ReservationDateTime = booking.ReservationDateTime,
                                          InitialCheckOutDate = booking.InitialCheckOutDate,
                                          InitialCheckOutTime = booking.InitialCheckOutTime,
                                          InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                          ServicesAmount = booking.ServicesAmount,
                                          TotalAmount = booking.TotalAmount,
                                          TotalAmountWithOutDiscount = booking.TotalAmountWithOutDiscount,

                                          AgentAdvanceAmount = booking.AgentAdvanceAmount,
                                          InvoiceName = booking.InvoiceName,
                                          BillTo = booking.BillTo,
                                          TotalServicesAmount = booking.TotalServicesAmount,
                                          ServicesTaxAmount = booking.ServicesTaxAmount,
                                          CancelAmount = booking.CancelAmount,
                                          CancelDate = request.CancelDate,
                                          CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                          IsEarlyCheckIn = booking.IsEarlyCheckIn,
                                          EarlyCheckInPolicyName = booking.EarlyCheckInPolicyName,
                                          EarlyCheckInDeductionBy = booking.EarlyCheckInDeductionBy,
                                          EarlyCheckInApplicableOn = booking.EarlyCheckInApplicableOn,
                                          EarlyCheckInFromHour = booking.EarlyCheckInFromHour,
                                          EarlyCheckInToHour = booking.EarlyCheckInToHour,
                                          EarlyCheckInCharges = booking.EarlyCheckInCharges,
                                          IsLateCheckOut = booking.IsLateCheckOut,
                                          LateCheckOutPolicyName = booking.LateCheckOutPolicyName,
                                          LateCheckOutDeductionBy = booking.LateCheckOutDeductionBy,
                                          LateCheckOutApplicableOn = booking.LateCheckOutApplicableOn,
                                          LateCheckOutFromHour = booking.LateCheckOutFromHour,
                                          LateCheckOutToHour = booking.LateCheckOutToHour,
                                          LateCheckOutCharges = booking.LateCheckOutCharges,
                                          DiscountType = booking.DiscountType,
                                          DiscountPercentage = booking.DiscountPercentage,
                                          DiscountAmount = booking.DiscountAmount,
                                          DiscountTotalAmount = booking.DiscountTotalAmount,
                                          BookingAmountWithoutDiscount = booking.BookingAmountWithoutDiscount,
                                          CalculateRoomRates = booking.CalculateRoomRates,
                                          TransactionCharges = booking.TransactionCharges,
                                          IsSelectedValue = true,

                                          // NotMapped fields
                                          RoomTypeName = category.Type,
                                          RoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
                                          BookedRoomRates = _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList(),
                                          GuestDetails = _context.GuestDetails.FirstOrDefault(x => x.IsActive == true && x.GuestId == booking.GuestId) ?? guest,
                                          AdvanceServices = _context.AdvanceServices.Where(x => x.IsActive == true && x.BookingId == booking.BookingId).ToList(),
                                      }
                                    ).ToList();


                bookingDetails = bookingDetails.Where(x => x.AdvanceServices.Count == 0).ToList();

                cancelBookingResponse.IsAllCancel = allbookingDetails.Count == bookingDetails.Count ? true : false;
               

                

                List<PaymentDetails> paymentDetails = await _context.PaymentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId && x.PaymentLeft > 0 && x.ReservationNo == request.ReservationNo).ToListAsync();


                List<CancelPolicyMaster> cancelPolicies = await _context.CancelPolicyMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.DeductionBy == property.CancelCalculatedBy).ToListAsync();

               
                bool flag = CalculateCancelAmount(bookingDetails, property.CancelMethod, cancelPolicies, request.CancelDate);
                if (flag == false)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }

                CancelSummary cancelSummary = new CancelSummary();
                cancelBookingResponse.CancelSummary = CalculatePaymentSummary(cancelSummary, paymentDetails, bookingDetails);
                bookingDetails = bookingDetails.OrderBy(x => x.CancelAmount).ToList();
                CalculateInvoice(bookingDetails, paymentDetails, Constants.Constants.Cancel);

                foreach (var item in bookingDetails)
                {
                    item.BalanceAmount = CalculateBalanceCancelAmount(item);
                }

               


                if (cancelBookingResponse.IsAllCancel)
                {
                    SetRefundAmount(paymentDetails, bookingDetails);
                }
                else
                {
                    SetResidualAmount(paymentDetails, bookingDetails);
                }

                cancelBookingResponse.CancelSummary = CalculateCancelSummary(cancelSummary, bookingDetails);

                cancelBookingResponse.bookingDetails = bookingDetails;
                cancelBookingResponse.PaymentDetails = paymentDetails;





                return Ok(new { Code = 200, Message = "Bookings fetched successfully", data = cancelBookingResponse });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        private void SetRefundAmount(List<PaymentDetails> paymentDetails, List<BookingDetail> bookingDetails)
        {
            Dictionary<int, decimal> refundAmouts = new Dictionary<int, decimal>();
            //calculate refund
            foreach (var pay in paymentDetails)
            {

                if (pay.RoomId == 0 && pay.BookingId == 0)
                {
                    pay.IsReceived = true;
                    pay.RefundAmount = pay.PaymentLeft;
                    pay.PaymentLeft = 0;

                    decimal equallydivide = 0;
                    if (pay.RefundAmount > 0)
                    {
                        equallydivide = EquallyDivideValue(pay.RefundAmount, pay.InvoiceHistories.Count == 0 ? bookingDetails.Count : pay.InvoiceHistories.Count);
                    }
                    if (pay.InvoiceHistories.Count == 0)
                    {
                        foreach (var item in bookingDetails)
                        {
                            if (refundAmouts.ContainsKey(item.BookingId))
                            {
                                refundAmouts[item.BookingId] = refundAmouts[item.BookingId] + equallydivide;
                            }
                            else
                            {
                                refundAmouts.Add(item.BookingId, equallydivide);
                            }
                        }
                    }
                    foreach (var invoice in pay.InvoiceHistories)
                    {
                        if (pay.RefundAmount > 0)
                        {
                            invoice.RefundAmount = equallydivide;
                            invoice.PaymentLeft = 0;

                            if (refundAmouts.ContainsKey(invoice.BookingId))
                            {
                                refundAmouts[invoice.BookingId] = refundAmouts[invoice.BookingId] + invoice.RefundAmount;
                            }
                            else
                            {
                                refundAmouts.Add(invoice.BookingId, invoice.RefundAmount);
                            }
                        }



                    }
                }


            }

            foreach (var kvp in refundAmouts)
            {
                foreach (var item in bookingDetails)
                {
                    if (item.BookingId == kvp.Key)
                    {
                        item.RefundAmount = kvp.Value;
                    }
                }


            }
        }

        private void SetResidualAmount(List<PaymentDetails> paymentDetails, List<BookingDetail> bookingDetails)
        {
            Dictionary<int, decimal> residualAmount = new Dictionary<int, decimal>();
            //calculate refund
            foreach (var pay in paymentDetails)
            {

                if (pay.RoomId == 0 && pay.BookingId == 0)
                {




                    decimal equallydivide = 0;
                    if (pay.PaymentLeft > 0)
                    {
                        equallydivide = EquallyDivideValue(pay.PaymentLeft, bookingDetails.Count);
                    }

                    foreach (var item in bookingDetails)
                    {
                        item.ResidualAmount += equallydivide;


                    }

                }


            }

        }

        //CANCEL BOOKING INVOICE CANCELLATION
        private bool CalculateCancelAmount(List<BookingDetail> bookings, string cancelMethod, List<CancelPolicyMaster> cancelPolicies, DateTime cancelDate)
        {
            bool flag = true;

            if(cancelPolicies.Count == 0)
            {

                foreach(var item in bookings)
                {
                    int noOfHours = NoOfHours(cancelDate, item.ReservationDateTime);
                    item.CancelAmount = 0;
                    CancelPolicyMaster cancelPolicy = Constants.Constants.CreateDefaultCancelPolicyMaster(); ;

                    item.RoomCancelHistory.Add(CreateRoomCancelHistory(item, cancelPolicy, cancelDate, bookings.Count, noOfHours, item.ReservationDateTime, item.CancelAmount, cancelMethod));
                }
                return true;

            }

            if (cancelMethod == Constants.Constants.DateWiseCancel)
            {
                foreach (var item in bookings)
                {
                    int nights = item.NoOfNights;
                    DateTime dateTime = item.ReservationDateTime;
                    while (nights > 0)
                    {

                        int noOfHours = NoOfHours(cancelDate, dateTime);
                        noOfHours = noOfHours <= 0 ? 1 : noOfHours;
                        var cancelPolicy = FindCancelPolicy(bookings.Count, noOfHours, cancelPolicies);
                        if (cancelPolicy != null)
                        {
                            if (cancelPolicy.DeductionBy == Constants.Constants.DeductionByAmount)
                            {
                                item.CancelAmount = item.CancelAmount + cancelPolicy.DeductionAmount;

                                item.RoomCancelHistory.Add(CreateRoomCancelHistory(item, cancelPolicy, cancelDate, bookings.Count, noOfHours, dateTime, cancelPolicy.DeductionAmount, cancelMethod));
                            }
                            else
                            {
                                //find room rate for that date
                                BookedRoomRate dateWiseRate = item.BookedRoomRates.FirstOrDefault(x =>x.BookingDate == DateOnly.FromDateTime(dateTime));
                                if (dateWiseRate == null)
                                {
                                    flag = false;
                                    return flag;
                                }
                                decimal cancelAmt = 0;
                                if (cancelPolicy.ChargesApplicableOn == Constants.Constants.ChargesOnTotalAmount)
                                {
                                    cancelAmt = Constants.Calculation.CalculatePercentage(dateWiseRate.TotalRoomRate, cancelPolicy.DeductionAmount);
                                }
                                else
                                {
                                    cancelAmt = Constants.Calculation.CalculatePercentage(dateWiseRate.RoomRate, cancelPolicy.DeductionAmount);
                                }

                                item.CancelAmount = item.CancelAmount + cancelAmt;

                                item.RoomCancelHistory.Add(CreateRoomCancelHistory(item, cancelPolicy, cancelDate, bookings.Count, noOfHours, dateTime, cancelAmt, cancelMethod));

                            }

                        }
                        else
                        {
                            flag = false;
                            return flag;
                        }

                        dateTime = dateTime.AddDays(1);
                        nights--;
                    }
                }
            }
            else
            {
                foreach (var item in bookings)
                {
                    int noOfHours = NoOfHours(cancelDate, item.ReservationDateTime);
                    if(noOfHours < 0)
                    {
                        noOfHours = 0;
                    }
                    var cancelPolicy = FindCancelPolicy(bookings.Count, noOfHours, cancelPolicies);
                    if (cancelPolicy != null)
                    {
                        if (cancelPolicy.DeductionBy == Constants.Constants.DeductionByAmount)
                        {
                            item.CancelAmount = cancelPolicy.DeductionAmount;


                        }
                        else
                        {
                            if (cancelPolicy.ChargesApplicableOn == Constants.Constants.ChargesOnTotalAmount)
                            {
                                item.CancelAmount = Constants.Calculation.CalculatePercentage(item.TotalBookingAmount, cancelPolicy.DeductionAmount);
                            }
                            else
                            {
                                item.CancelAmount = Constants.Calculation.CalculatePercentage(item.BookingAmount, cancelPolicy.DeductionAmount);
                            }

                        }
                        item.RoomCancelHistory.Add(CreateRoomCancelHistory(item, cancelPolicy, cancelDate, bookings.Count, noOfHours, item.ReservationDateTime, item.CancelAmount, cancelMethod));
                    }
                    else
                    {
                        item.CancelAmount =  0;
                        CancelPolicyMaster defaultCancelPolicy = Constants.Constants.CreateDefaultCancelPolicyMaster(); ;
                        item.RoomCancelHistory.Add(CreateRoomCancelHistory(item, defaultCancelPolicy, cancelDate, bookings.Count, noOfHours, item.ReservationDateTime, item.CancelAmount, cancelMethod));
                    }
                }
            }
            return flag;
        }

        private RoomCancelHistory CreateRoomCancelHistory(BookingDetail booking, CancelPolicyMaster cancelPolicy, DateTime cancelDate, int noOfRooms, int noOfHours, DateTime toTime, decimal CancelAmount, string cancelMethod)
        {
            RoomCancelHistory history = new RoomCancelHistory();
            history.BookingId = booking.BookingId;
            history.RoomId = booking.RoomId;
            history.ReservationNo = booking.ReservationNo;
            history.PolicyId = cancelPolicy.Id;
            history.PolicyCode = cancelPolicy.PolicyCode;
            history.PolicyDescription = cancelPolicy.PolicyDescription;
            history.DeductionBy = cancelPolicy.DeductionBy;
            history.ChargesApplicableOn = cancelPolicy.ChargesApplicableOn;
            history.CancellationTime = cancelPolicy.CancellationTime;
            history.FromTime = cancelPolicy.FromTime;
            history.ToTime = cancelPolicy.ToTime;
            history.NoOfRooms = noOfRooms;
            history.CancelHours = noOfHours;
            history.CancelFromDate = cancelDate;
            history.CancelToDate = toTime;
            history.CancelAmount = CancelAmount;
            history.CancelPercentage = cancelPolicy.DeductionAmount;
            history.CancelFormat = cancelMethod;
            return history;
        }

        private int NoOfHours(DateTime startDate, DateTime endDate)
        {
            return (int)(endDate - startDate).TotalHours;
        }

        private CancelPolicyMaster FindCancelPolicy(int noOfRooms, int hours, List<CancelPolicyMaster> cancelPolicies)
        {
            var cancelPolicy = cancelPolicies.FirstOrDefault(x => (x.MinRoom <= noOfRooms && x.MaxRoom >= noOfRooms)
             && (x.FromTime <= hours && x.ToTime > hours)
            );

            return cancelPolicy;
        }
        
        private CancelSummary CalculatePaymentSummary(CancelSummary cancelSummary , List<PaymentDetails> paymentDetails,List<BookingDetail> bookingDetails)
        {
            foreach (var pay in paymentDetails)
            {
                if (pay.PaymentStatus == Constants.Constants.AdvancePayment || pay.PaymentStatus == Constants.Constants.AgentPayment)
                {
                    cancelSummary.AdvanceAmount = cancelSummary.AdvanceAmount + pay.PaymentLeft;
                    cancelSummary.TransactionCharges += pay.TransactionAmount;
                }
                
                else
                {
                    if(pay.BookingId == 0 && pay.RoomId == 0)
                    {
                        cancelSummary.ReceivedAmount = cancelSummary.ReceivedAmount + pay.PaymentLeft;
                        cancelSummary.TransactionCharges += pay.TransactionAmount;
                    }
                    else if (bookingDetails.Select(x => x.BookingId).Contains(pay.BookingId))
                    {
                        cancelSummary.ReceivedAmount = cancelSummary.ReceivedAmount + pay.PaymentLeft;
                        cancelSummary.TransactionCharges += pay.TransactionAmount;
                    }
                    
                }

            }

            cancelSummary.TotalPaid = cancelSummary.AgentAmount + cancelSummary.AdvanceAmount + cancelSummary.ReceivedAmount;
           

            return cancelSummary;
        }

        private CancelSummary CalculateCancelSummary(CancelSummary cancelSummary, List<BookingDetail> bookingDetails)
        {
        
            cancelSummary.TotalRooms = bookingDetails.Count;
            
            foreach (var item in bookingDetails)
            {
                cancelSummary.CancelAmount = cancelSummary.CancelAmount + item.CancelAmount;
                cancelSummary.BalanceAmount = cancelSummary.BalanceAmount + item.BalanceAmount;
                cancelSummary.RefundAmount = cancelSummary.RefundAmount + item.RefundAmount;
            }
           
            return cancelSummary;
        }



        private decimal CalculateBalanceCancelAmount(BookingDetail booking)
        {
            return (booking.CancelAmount - (booking.AgentAdvanceAmount + booking.AdvanceAmount + booking.ReceivedAmount));
        }

        private int GetBalanceCancelCount(List<BookingDetail> bookings)
        {
            int count = 0;
            foreach (var item in bookings)
            {
                decimal balance = CalculateBalanceCancelAmount(item);
                if (balance > 0)
                {
                    count++;
                }
            }
            return count;
        }
        [HttpPost("UpdateRoomsCancel")]
        public async Task<IActionResult> UpdateRoomsCancel([FromBody] CancelBookingResponse request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            DateTime currentDate = DateTime.Now;
            List<CancelBookingNotificationDTO> notificationDTOs = new List<CancelBookingNotificationDTO>();
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();
                if (request.ReservationDetails == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Reservation details not found" });
                }

                if (request.bookingDetails.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "No bookings found for cancellation" });
                }

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if (property == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Property not found" });
                }

                foreach (var item in request.bookingDetails)
                {
                    CancelBookingNotificationDTO inNotificationDTO = new CancelBookingNotificationDTO();
                    var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId);
                    if (booking == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Booking not found" });
                    }
                    booking.Status = Constants.Constants.Cancel;
                    booking.CancelDate = item.CancelDate;
                    booking.CancelAmount = item.CancelAmount;
                    booking.AdvanceAmount = item.AdvanceAmount;
                    booking.ReceivedAmount = item.ReceivedAmount;
                    booking.IsActive = false;
                    booking.UpdatedDate = currentDate;
                    booking.BalanceAmount = item.BalanceAmount;
                    booking.RefundAmount = item.RefundAmount;
                    booking.ResidualAmount = item.ResidualAmount;
                    booking.InvoiceNo = item.InvoiceNo;
                    booking.InvoiceDate = DateOnly.FromDateTime(currentDate);
                    booking.CheckOutInvoiceFormat = property.CheckOutInvoice;
                    _context.BookingDetail.Update(booking);


                    inNotificationDTO.RoomNo = await _context.RoomMaster
                                                     .Where(x => x.RoomId == booking.RoomId)
                                                     .Select(x => x.RoomNo)
                                                     .FirstOrDefaultAsync() ?? "";
                    var guestDetails = await _context.GuestDetails
                                                .Where(x => x.GuestId == booking.PrimaryGuestId)

                                                .FirstOrDefaultAsync();
                    inNotificationDTO.GuestName = guestDetails?.GuestName ?? "";
                    inNotificationDTO.GuestPhoneNo = guestDetails?.PhoneNumber ?? "";
                    inNotificationDTO.GuestEmail = guestDetails?.Email ?? "";
                    inNotificationDTO.Pax = booking.Pax;


                    inNotificationDTO.CancelDate = booking.CheckOutDateTime.ToString("f");
                    inNotificationDTO.RoomType = await _context.RoomCategoryMaster.Where(x => x.Id == booking.RoomTypeId).Select(x => x.Type).FirstOrDefaultAsync() ?? "";
                    inNotificationDTO.ReservationNo = booking.ReservationNo;
                    notificationDTOs.Add(inNotificationDTO);

                    var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId);
                    if (roomAvailability == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Room availability not found" });
                    }

                    _context.RoomAvailability.Remove(roomAvailability);
                }

                await _context.SaveChangesAsync();

                //booked room rates and cancel history
                foreach (var item in request.bookingDetails)
                {
                    List<BookedRoomRate> rates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();

                    foreach (var rate in rates)
                    {
                        rate.IsActive = false;
                        rate.UpdatedDate = currentDate;
                        _context.BookedRoomRates.Update(rate);
                    }

                    foreach (var cancelHistory in item.RoomCancelHistory)
                    {
                        Constants.Constants.SetMastersDefault(cancelHistory, companyId, userId, currentDate);
                        await _context.RoomCancelHistory.AddAsync(cancelHistory);
                    }
                }

                //payment
                foreach (var pay in request.PaymentDetails)
                {
                    var payment = await _context.PaymentDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.PaymentId == pay.PaymentId);
                    if (payment == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Payment not found" });
                    }
                    payment.PaymentLeft = pay.PaymentLeft;
                    payment.IsReceived = pay.IsReceived;
                    payment.RefundAmount = pay.RefundAmount;
                    foreach (var invoice in pay.InvoiceHistories)
                    {
                        Constants.Constants.SetMastersDefault(invoice, companyId, userId, currentDate);
                        invoice.InvoiceDate = DateOnly.FromDateTime(currentDate);
                        await _context.InvoiceHistory.AddAsync(invoice);
                    }
                    _context.PaymentDetails.Update(payment);

                }




                string result = await DocumentHelper.UpdateDocumentNo(_context,Constants.Constants.DocumentInvoice, companyId, financialYear);
                if (result == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Error while updating document" });
                }

                foreach (var item in notificationDTOs)
                {
                    //send email
                    string subject = $"Cancellation Successful - {item.ReservationNo}";
                    string htmlBody = @$"
                <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Cancellation Confirmation</title>
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">

    <table width=""100%"" style=""max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        <tr>
            <td style=""padding: 20px; text-align: center; background-color: #007bff; color: white; border-top-left-radius: 8px; border-top-right-radius: 8px;"">
                <h2>Booking Cancelled</h2>
            </td>
        </tr>

        <tr>
            <td style=""padding: 20px; color: #333333;"">
                <p>Dear <strong>{item.GuestName}</strong>,</p>

                <p>We are pleased to inform you that your check-out has been successfully confirmed!</p>

                <table width=""100%"" style=""margin-top: 20px;"">
                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Reservation Number:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.ReservationNo}</td>
                    </tr>
<tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Room Type:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.RoomNo}</td>
                    </tr>

  <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Room Category:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.RoomType}</td>
                    </tr>

  
                   
                    <tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Cancel Date:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.CancelDate}</td>
                    </tr>

<tr>
                        <td style=""padding: 8px; color: #555555;""><strong>Pax:</strong></td>
                        <td style=""padding: 8px; color: #555555;"">{item.Pax}</td>
                    </tr>
                </table>

                <p style=""margin-top: 20px;"">If you have any questions or special requests, feel free to contact us.  
                <br>We look forward to welcoming you and ensuring you have a wonderful stay!</p>

                <p style=""margin-top: 30px;"">Thank you for choosing <strong>{property.CompanyName}</strong>!</p>
            </td>
        </tr>

        <tr>
            <td style=""padding: 20px; text-align: center; font-size: 12px; color: #888888; background-color: #f1f1f1; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px;"">
                {property.CompanyName} | {property.ContactNo1} | {property.CompanyAddress} <br/>
                
            </td>
        </tr>
    </table>

</body>
</html>
";

                    await Notifications.Notification.SendMail(_context, subject, htmlBody, companyId, item.GuestEmail);

                }


                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Code = 200, Message = "Booking Cancel Successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetCheckOutInvoiceData")]
        public async Task<IActionResult> GetCheckOutInvoiceData(int bookingId)
        {
            try
            {
                
                var response = new CheckOutResponse();

               

                var bookingdetail = await _context.BookingDetail.FirstOrDefaultAsync(x => x.BookingId == bookingId && x.CompanyId == companyId && x.IsActive == true);

                if(bookingdetail == null)
                {
                    return Ok(new { Code = 400, Message = "Booking details not found" });
                }
                response.CheckOutFormat = bookingdetail.CheckOutInvoiceFormat;
                var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.ReservationNo == bookingdetail.ReservationNo);

                if (reservationDetails == null)
                {
                    return Ok(new { Code = 400, Message = "Reservation details not found" });
                }
                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == reservationDetails.PrimaryGuestId);
                List<BookingDetail> BookingDetails = new List<BookingDetail>();

                if (bookingdetail.CheckOutInvoiceFormat == Constants.Constants.ReservationInvoice)
                {
                    BookingDetails = await (from booking in _context.BookingDetail
                                            join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                            join guest in _context.GuestDetails
                                            on booking.GuestId equals guest.GuestId
                                            where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == bookingdetail.ReservationNo && booking.Status == Constants.Constants.CheckOut && booking.InvoiceNo == bookingdetail.InvoiceNo
                                            select new BookingDetail
                                            {
                                                BookingId = booking.BookingId,
                                                GuestId = booking.GuestId,
                                                RoomId = booking.RoomId,
                                                RoomTypeId = booking.RoomTypeId,
                                                CheckInDate = booking.CheckInDate,
                                                CheckInTime = booking.CheckInTime,
                                                CheckOutDate = booking.CheckOutDate,
                                                CheckOutTime = booking.CheckOutTime,
                                                CheckInDateTime = booking.CheckInDateTime,
                                                CheckOutDateTime = booking.CheckOutDateTime,
                                                CheckoutFormat = booking.CheckoutFormat,
                                                NoOfNights = booking.NoOfNights,
                                                NoOfHours = booking.NoOfHours,
                                               
                                                RoomCount = booking.RoomCount,
                                                Pax = booking.Pax,
                                                Status = booking.Status,
                                                Remarks = booking.Remarks,
                                                ReservationNo = booking.ReservationNo,
                                                BookingDate = booking.BookingDate,
                                                CreatedDate = booking.CreatedDate,
                                                UpdatedDate = booking.UpdatedDate,
                                                IsActive = booking.IsActive,
                                                PrimaryGuestId = booking.PrimaryGuestId,
                                                InitialBalanceAmount = booking.InitialBalanceAmount,
                                                BalanceAmount = booking.BalanceAmount,
                                                AdvanceAmount = booking.AdvanceAmount,
                                                ReceivedAmount = booking.ReceivedAmount,
                                                AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                                RefundAmount = booking.RefundAmount,

                                                UserId = booking.UserId,
                                                GstType = booking.GstType,
                                                CompanyId = booking.CompanyId,
                                                BookingAmount = booking.BookingAmount,
                                                GstAmount = booking.GstAmount,
                                                TotalBookingAmount = booking.TotalBookingAmount,
                                                BookingSource = booking.BookingSource,
                                                ReservationDate = booking.ReservationDate,
                                                ReservationTime = booking.ReservationTime,
                                                ReservationDateTime = booking.ReservationDateTime,
                                                RoomTypeName = roomType.Type,
                                                RoomNo = rooms.RoomNo,
                                                InitialCheckOutDate = booking.InitialCheckOutDate,
                                                InitialCheckOutTime = booking.InitialCheckOutTime,
                                                InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                                InvoiceName = booking.InvoiceName,
                                                BillTo = "",
                                                InvoiceNo = booking.InvoiceNo,
                                                InvoiceDate = booking.InvoiceDate,
                                                TotalAmount = booking.TotalAmount,
                                                ServicesAmount = booking.ServicesAmount,
                                                ServicesTaxAmount = booking.ServicesTaxAmount,
                                                TotalServicesAmount = booking.TotalServicesAmount,
                                                BookedRoomRates = _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList(),
                                                GuestDetails = guest,
                                                AdvanceServices = _context.AdvanceServices.Where(x => x.IsActive == true && x.BookingId == booking.BookingId).ToList()
                                            }).ToListAsync();


                    PaymentCheckOutSummary paymentSummary = new PaymentCheckOutSummary();
                    foreach(var item in BookingDetails)
                    {
                        paymentSummary.TotalBookingAmount += item.TotalBookingAmount;
                        paymentSummary.EarlyCheckIn += item.EarlyCheckInCharges;
                        paymentSummary.LateCheckOut += item.LateCheckOutCharges;
                        paymentSummary.TotalServiceAmount += item.TotalServicesAmount;
                        paymentSummary.TotalAmount += item.TotalAmountWithOutDiscount;
                        paymentSummary.CheckOutDiscoutAmount += item.CheckOutDiscoutAmount;
                        paymentSummary.TotalBill += item.TotalAmount;
                        paymentSummary.TotalAmountPaid += item.AdvanceAmount + item.ReceivedAmount + item.RefundAmount + item.ResidualAmount;
                        paymentSummary.AdvanceAmount += item.AdvanceAmount;
                        paymentSummary.ReceivedAmount += item.ReceivedAmount;
                        item.BalanceAmount += item.BalanceAmount;
                        paymentSummary.RefundAmount += item.RefundAmount;
                        paymentSummary.ResidualAmount += item.ResidualAmount;

                    }
                    response.PaymentSummary = paymentSummary;

                }
                else
                {
                    BookingDetails = await (from booking in _context.BookingDetail
                                            join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                            join guest in _context.GuestDetails
                                            on booking.GuestId equals guest.GuestId
                                            where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == booking.ReservationNo && booking.Status == Constants.Constants.CheckOut && booking.BookingId == bookingId
                                            select new BookingDetail
                                            {

                                                BookingId = booking.BookingId,
                                                GuestId = booking.GuestId,
                                                RoomId = booking.RoomId,
                                                RoomTypeId = booking.RoomTypeId,
                                                CheckInDate = booking.CheckInDate,
                                                CheckInTime = booking.CheckInTime,
                                                CheckOutDate = booking.CheckOutDate,
                                                CheckOutTime = booking.CheckOutTime,
                                                CheckInDateTime = booking.CheckInDateTime,
                                                CheckOutDateTime = booking.CheckOutDateTime,
                                                CheckoutFormat = booking.CheckoutFormat,
                                                NoOfNights = booking.NoOfNights,
                                                NoOfHours = booking.NoOfHours,
                                                RoomCount = booking.RoomCount,
                                                Pax = booking.Pax,
                                                Status = booking.Status,
                                                Remarks = booking.Remarks,
                                                ReservationNo = booking.ReservationNo,
                                                BookingDate = booking.BookingDate,
                                                CreatedDate = booking.CreatedDate,
                                                UpdatedDate = booking.UpdatedDate,
                                                IsActive = booking.IsActive,
                                                PrimaryGuestId = booking.PrimaryGuestId,
                                                InitialBalanceAmount = booking.InitialBalanceAmount,
                                                BalanceAmount = booking.BalanceAmount,
                                                AdvanceAmount = booking.AdvanceAmount,
                                                ReceivedAmount = booking.ReceivedAmount,
                                                AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                                RefundAmount = booking.RefundAmount,
                                                InvoiceDate = booking.InvoiceDate,
                                                InvoiceNo = booking.InvoiceNo,
                                                UserId = booking.UserId,
                                                GstType = booking.GstType,
                                                CompanyId = booking.CompanyId,
                                                BookingAmount = booking.BookingAmount,
                                                GstAmount = booking.GstAmount,
                                                TotalBookingAmount = booking.TotalBookingAmount,
                                                BookingSource = booking.BookingSource,
                                                ReservationDate = booking.ReservationDate,
                                                ReservationTime = booking.ReservationTime,
                                                ReservationDateTime = booking.ReservationDateTime,
                                                InitialCheckOutDate = booking.InitialCheckOutDate,
                                                InitialCheckOutTime = booking.InitialCheckOutTime,
                                                InitialCheckOutDateTime = booking.InitialCheckOutDateTime,
                                                ServicesAmount = booking.ServicesAmount,
                                                TotalAmount = booking.TotalAmount,
                                                TotalAmountWithOutDiscount = booking.TotalAmountWithOutDiscount,

                                                AgentAdvanceAmount = booking.AgentAdvanceAmount,
                                                InvoiceName = booking.InvoiceName,
                                                BillTo = booking.BillTo,
                                                TotalServicesAmount = booking.TotalServicesAmount,
                                                ServicesTaxAmount = booking.ServicesTaxAmount,
                                                CancelAmount = booking.CancelAmount,
                                                CancelDate = booking.CancelDate,
                                                CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                                IsEarlyCheckIn = booking.IsEarlyCheckIn,
                                                EarlyCheckInPolicyName = booking.EarlyCheckInPolicyName,
                                                EarlyCheckInDeductionBy = booking.EarlyCheckInDeductionBy,
                                                EarlyCheckInApplicableOn = booking.EarlyCheckInApplicableOn,
                                                EarlyCheckInFromHour = booking.EarlyCheckInFromHour,
                                                EarlyCheckInToHour = booking.EarlyCheckInToHour,
                                                EarlyCheckInCharges = booking.EarlyCheckInCharges,
                                                IsLateCheckOut = booking.IsLateCheckOut,
                                                LateCheckOutPolicyName = booking.LateCheckOutPolicyName,
                                                LateCheckOutDeductionBy = booking.LateCheckOutDeductionBy,
                                                LateCheckOutApplicableOn = booking.LateCheckOutApplicableOn,
                                                LateCheckOutFromHour = booking.LateCheckOutFromHour,
                                                LateCheckOutToHour = booking.LateCheckOutToHour,
                                                LateCheckOutCharges = booking.LateCheckOutCharges,
                                                DiscountType = booking.DiscountType,
                                                DiscountPercentage = booking.DiscountPercentage,
                                                DiscountAmount = booking.DiscountAmount,
                                                DiscountTotalAmount = booking.DiscountTotalAmount,
                                                BookingAmountWithoutDiscount = booking.BookingAmountWithoutDiscount,
                                                CalculateRoomRates = booking.CalculateRoomRates,
                                                TransactionCharges = booking.TransactionCharges,
                                                IsSelectedValue = true,
                                                CheckOutDiscountType = booking.CheckOutDiscountType,
                                                CheckOutDiscountPercentage = booking.CheckOutDiscountPercentage,
                                                CheckOutDiscoutAmount = booking.CheckOutDiscoutAmount,
                                                
                                                ResidualAmount = booking.ResidualAmount,

                                                // NotMapped fields
                                                RoomTypeName = roomType.Type,
                                                RoomNo = rooms.RoomNo,
                                                BookedRoomRates = _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList(),
                                                GuestDetails = guest,
                                                AdvanceServices = _context.AdvanceServices.Where(x => x.IsActive == true && x.BookingId == booking.BookingId).ToList(),
                                            }).ToListAsync();

                }

                if (BookingDetails.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "No bookings found" });
                }

                response.InvoiceDate = BookingDetails[0].InvoiceDate;
                response.InvoiceNo = BookingDetails[0].InvoiceNo;
                response.InvoiceName = BookingDetails[0].InvoiceName;

                //PaymentCheckOutSummary paymentSummary = await CalculateSummaryForCheckOut(reservationDetails, BookingDetails,paymentDetails);

                response.BookingDetails = BookingDetails;
                response.ReservationDetails = reservationDetails;
                //response.PaymentSummary = paymentSummary;
                return Ok(new { Code = 200, Message = "Data fetched successfully", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetCancelInvoiceData")]
        public async Task<IActionResult> GetCancelInvoiceData(int bookingId)
        {
            try
            {

                var response = new CancelBookingResponse();

                var bookings = await _context.BookingDetail.FirstOrDefaultAsync(x => x.BookingId == bookingId && x.CompanyId == companyId);
                if(bookings == null)
                {
                    return Ok(new { Code = 400, Message = "Booking detail not found" });
                }
                response.CheckOutFormat = bookings.CheckOutInvoiceFormat;

                var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.ReservationNo == bookings.ReservationNo);

                if (reservationDetails == null)
                {
                    return Ok(new { Code = 400, Message = "Reservation details not found" });
                }
                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == reservationDetails.PrimaryGuestId);
                List<BookingDetail> BookingDetails = new List<BookingDetail>();

                if (bookings.CheckOutInvoiceFormat == Constants.Constants.ReservationInvoice)
                {
                    BookingDetails = await (from booking in _context.BookingDetail
                                            join rm in _context.RoomMaster on booking.RoomId equals rm.RoomId into rms
                                            from room in rms.DefaultIfEmpty()
                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                            join guest in _context.GuestDetails
                                            on booking.GuestId equals guest.GuestId
                                            where booking.Status == Constants.Constants.Cancel && booking.CompanyId == companyId && booking.InvoiceNo == bookings.InvoiceNo
                                            select new BookingDetail
                                            {
                                                BookingId = booking.BookingId,
                                                GuestId = booking.GuestId,
                                                RoomId = booking.RoomId,
                                                RoomTypeId = booking.RoomTypeId,
                                                NoOfNights = booking.NoOfNights,
                                                NoOfHours = booking.NoOfHours,
                                                CancelAmount = booking.CancelAmount,
                                                CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                                RoomCount = booking.RoomCount,
                                                Pax = booking.Pax,
                                                Status = booking.Status,
                                                Remarks = booking.Remarks,
                                                ReservationNo = booking.ReservationNo,
                                                InitialBalanceAmount = booking.InitialBalanceAmount,
                                                BalanceAmount = booking.BalanceAmount,
                                                AdvanceAmount = booking.AdvanceAmount,
                                                ReceivedAmount = booking.ReceivedAmount,
                                                AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                                RefundAmount = booking.RefundAmount,
                                                RoomTypeName = roomType.Type,
                                                RoomNo = room == null ? "" : room.RoomNo,
                                                InvoiceName = booking.InvoiceName,
                                                BillTo = "",
                                                InvoiceNo = booking.InvoiceNo,
                                                InvoiceDate = booking.InvoiceDate,
                                                RoomCancelHistory = _context.RoomCancelHistory.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList()
                                            }).ToListAsync();

                    CancelSummary paymentSummary = new CancelSummary();
                    foreach (var item in BookingDetails)
                    {
                        paymentSummary.AgentAmount += item.AgentAdvanceAmount;
                        paymentSummary.AdvanceAmount += item.AdvanceAmount;
                        paymentSummary.ReceivedAmount += item.ReceivedAmount;
                        paymentSummary.CancelAmount += item.CancelAmount;
                        paymentSummary.BalanceAmount += item.BalanceAmount;
                        paymentSummary.RefundAmount += item.RefundAmount;
                    }
                    paymentSummary.TotalPaid = paymentSummary.AgentAmount + paymentSummary.AdvanceAmount + paymentSummary.ReceivedAmount + paymentSummary.RefundAmount;
                    response.CancelSummary = paymentSummary;

                }
                else
                {
                    BookingDetails = await (from booking in _context.BookingDetail
                                            join rm in _context.RoomMaster on booking.RoomId equals rm.RoomId into rms
                                            from room in rms.DefaultIfEmpty()
                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                            join guest in _context.GuestDetails
                                            on booking.GuestId equals guest.GuestId
                                            where booking.Status == Constants.Constants.Cancel && booking.CompanyId == companyId && booking.BookingId == bookingId
                                            select new BookingDetail
                                            {
                                                BookingId = booking.BookingId,
                                                GuestId = booking.GuestId,
                                                RoomId = booking.RoomId,
                                                RoomTypeId = booking.RoomTypeId,
                                                NoOfNights = booking.NoOfNights,
                                                NoOfHours = booking.NoOfHours,
                                                CancelAmount = booking.CancelAmount,
                                                CheckOutInvoiceFormat = booking.CheckOutInvoiceFormat,
                                                RoomCount = booking.RoomCount,
                                                Pax = booking.Pax,
                                                Status = booking.Status,
                                                Remarks = booking.Remarks,
                                                ReservationNo = booking.ReservationNo,
                                                InitialBalanceAmount = booking.InitialBalanceAmount,
                                                BalanceAmount = booking.BalanceAmount,
                                                AdvanceAmount = booking.AdvanceAmount,
                                                ReceivedAmount = booking.ReceivedAmount,
                                                AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                                RefundAmount = booking.RefundAmount,
                                                RoomTypeName = roomType.Type,
                                                RoomNo = room == null ? "" : room.RoomNo,
                                                InvoiceName = booking.InvoiceName,
                                                BillTo = "",
                                                InvoiceNo = booking.InvoiceNo,
                                                InvoiceDate = booking.InvoiceDate,
                                                RoomCancelHistory = _context.RoomCancelHistory.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList(),
                                                GuestDetails=guest
                                            }).ToListAsync();

                    
                }

                if (BookingDetails.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "No bookings found" });
                }

                response.InvoiceDate = BookingDetails[0].InvoiceDate;
                response.InvoiceNo = BookingDetails[0].InvoiceNo;
                response.InvoiceName = BookingDetails[0].InvoiceName;
                response.CancelDate = BookingDetails[0].CancelDate;
               
                response.bookingDetails = BookingDetails;
                response.ReservationDetails = reservationDetails;
                
                return Ok(new { Code = 200, Message = "Data fetched successfully", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        //GET ROOM AVAILABILITY
        private async Task<DataSet> GetRoomAvailability(DateOnly checkInDate, string checkInTime, DateOnly checkOutDate, string checkOutTime, string pageName = "", int roomTypeId = 0, int roomId = 0, string roomStatus = "")
        {
            DataSet dataSet = null;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("check_room_availaibility", connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@userCheckInDate", checkInDate);
                        command.Parameters.AddWithValue("@userCheckOutDate", checkOutDate);
                        command.Parameters.AddWithValue("@userCheckInTime", checkInTime);
                        command.Parameters.AddWithValue("@userCheckOutTime", checkOutTime);
                        command.Parameters.AddWithValue("@companyId", companyId);
                        command.Parameters.AddWithValue("@pageName", pageName);
                        command.Parameters.AddWithValue("@roomTypeId", roomTypeId);
                        command.Parameters.AddWithValue("@roomId", roomId);
                        command.Parameters.AddWithValue("@roomStatus", roomStatus);
                        await connection.OpenAsync();

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            dataSet = new DataSet();
                            adapter.Fill(dataSet);
                        }
                        await connection.CloseAsync();
                    }
                }
                return dataSet;
            }
            catch (Exception)
            {
                return dataSet;
            }
        }

        //GET ROOM NO AVAILABLE OR NOT
        private async Task<string> CheckRoomAvailable(DateOnly checkInDate, string checkInTime, DateOnly checkOutDate, string checkOutTime, int roomTypeId, int roomId)
        {
            //room is assigned
            DataSet dataSet = await GetRoomAvailability(checkInDate, checkInTime, checkOutDate, checkOutTime, "checkbyroomid", roomTypeId, roomId);
            if (dataSet == null)
            {

                return Constants.Constants.ErrorMessage;
            }
            //Check room is available or not
            var rows = new List<Dictionary<string, object>>();

            var dataTable = dataSet.Tables[0];
            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                rows.Add(dict);
            }
            if (rows.Count == 0)
            {
                return "Room not found";

            }
            else
            {
                if (rows[0]["roomStatus"].ToString() != Constants.Constants.Clean)
                {
                    return "Room " + rows[0]["RoomNo"] + " is already reserved with Reservation No " + rows[0]["ReservationNo"];
                }
                else
                {
                    return "success";
                }
            }
        }


        



        //CALCULATE ROOM RATE DATE WISE
        private async Task<(int Code, string Message, RoomRateResponse? Response)> CalculateRoomRateAsync(
            int companyId, int roomTypeId, DateOnly checkInDate, DateOnly checkOutDate,
            int noOfRooms, int noOfNights, string gstType, int noOfHours, string checkInTime, string checkOutTime, string discountType, decimal discount, string checkOutFormat, string roomRatesCalculatedBy, CompanyDetails property)
        {
            var roomRateResponse = new RoomRateResponse();

            if (roomTypeId == 0)
            {
                return (400, "Invalid data", roomRateResponse);
                //return Ok(new { Code = 400, Message = "Invalid data" });
            }

          

            var gstPercentage = await GetGstPercetage(Constants.Constants.Reservation);
            if (gstPercentage == null)
            {
                return (400, "Gst percentage not found for reservation", roomRateResponse);

            }
            //if checkout format is sameday
            if (checkOutFormat == Constants.Constants.SameDayFormat)
            {
                var hourObject = await _context.HourMaster.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.Hour == noOfHours);

                var roomRateDate = new BookedRoomRate();
                roomRateDate.BookingDate = checkInDate;
                roomRateDate.GstType = gstType;
                roomRateDate.RoomTypeId = roomTypeId;
                var roomRates = hourObject == null ? null : await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId && x.HourId == hourObject.Id).FirstOrDefaultAsync();
                if (roomRates == null)
                {
                    //get rates of that check in date 
                    //1. find custom rates
                    var customRoomRates = await _context.RoomRateDateWise.Where(x => x.IsActive == true && x.CompanyId == companyId && (x.FromDate <= checkInDate && x.ToDate >= checkInDate) && x.RoomTypeId == roomTypeId).OrderByDescending(x => x.RatePriority).FirstOrDefaultAsync();
                    if(customRoomRates == null)
                    {
                        //fetch standard rates
                        var standardRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId && x.HourId == 0).FirstOrDefaultAsync();
                        if (standardRates == null)
                        {

                            return (400, "No Room Rates found", roomRateResponse);

                        }
                        else
                        {
                            roomRateDate.RoomRateWithoutDiscount = standardRates.RoomRate;
                            //if any discount
                            if (discount > 0)
                            {
                                roomRateDate.DiscountType = discountType;
                                //discount type is percentage
                                if (discountType == Constants.Constants.DeductionByPercentage)
                                {
                                    roomRateDate.DiscountPercentage = discount;
                                    roomRateDate.DiscountAmount = Constants.Calculation.CalculatePercentage(standardRates.RoomRate, discount); 
                                    roomRateDate.RoomRate = standardRates.RoomRate - roomRateDate.DiscountAmount;

                                }
                                //discount type is amount
                                else
                                {
                                    roomRateDate.DiscountAmount = discount;
                                    roomRateDate.RoomRate = standardRates.RoomRate - discount;
                                }
                            }
                            //no discount
                            else
                            {
                                roomRateDate.RoomRate = standardRates.RoomRate;
                            }
                            if(roomRateDate.RoomRate < 0)
                            {
                                return (400, "Room rate is cannot be less than discount amount", roomRateResponse);
                            }
                            roomRateResponse.DiscountTotalAmount = roomRateResponse.DiscountTotalAmount + roomRateDate.DiscountAmount;
                            
                        }
                    }
                    else
                    {
                        roomRateDate.RoomRateWithoutDiscount = customRoomRates.RoomRate;
                        //if any discount
                        if (discount > 0)
                        {
                            roomRateDate.DiscountType = discountType;
                            //discount type is percentage
                            if (discountType == Constants.Constants.DeductionByPercentage)
                            {
                                roomRateDate.DiscountPercentage = discount;
                                roomRateDate.DiscountAmount = Constants.Calculation.CalculatePercentage(customRoomRates.RoomRate, discount);
                                roomRateDate.RoomRate = customRoomRates.RoomRate - roomRateDate.DiscountAmount;

                            }
                            //discount type is amount
                            else
                            {
                                roomRateDate.DiscountAmount = discount;
                                roomRateDate.RoomRate = customRoomRates.RoomRate - discount;
                            }
                        }
                        //no discount
                        else
                        {
                            roomRateDate.RoomRate = customRoomRates.RoomRate;
                        }

                        if (roomRateDate.RoomRate < 0)
                        {
                            return (400, "Room rate is cannot be less than discount amount", roomRateResponse);
                        }

                        roomRateResponse.DiscountAmount = roomRateResponse.DiscountAmount + roomRateDate.DiscountAmount;
                    }


                }
                //hour based rates
                else
                {
                    roomRateDate.RoomRateWithoutDiscount = roomRates.RoomRate;
                    //if any discount
                    if (discount > 0)
                    {
                        roomRateDate.DiscountType = discountType;
                        //discount type is percentage
                        if (discountType == Constants.Constants.DeductionByPercentage)
                        {
                            roomRateDate.DiscountPercentage = discount;
                            roomRateDate.DiscountAmount = Constants.Calculation.CalculatePercentage(roomRates.RoomRate, discount); 
                            roomRateDate.RoomRate = roomRates.RoomRate - roomRateDate.DiscountAmount;

                        }
                        //discount type is amount
                        else
                        {
                            roomRateDate.DiscountAmount = discount;
                            roomRateDate.RoomRate = roomRates.RoomRate - discount;
                        }
                    }
                    //no discount
                    else
                    {
                        roomRateDate.RoomRate = roomRates.RoomRate;
                    }
                    if (roomRateDate.RoomRate < 0)
                    {
                        return (400, "Room rate is cannot be less than discount amount", roomRateResponse);
                    }
                    roomRateResponse.DiscountTotalAmount = roomRateResponse.DiscountTotalAmount + roomRateDate.DiscountAmount;

                }

               
               
                if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                {
                    var gstRangeMaster = GetApplicableGstRange(gstPercentage.ranges, roomRateResponse.BookingAmount);
                    if (gstRangeMaster == null)
                    {
                        roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                    }
                    else
                    {
                        roomRateDate.GstPercentage = gstRangeMaster.TaxPercentage;
                    }
                }
                else
                {
                    roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                }
                (roomRateDate.RoomRate, roomRateDate.GstAmount) = Calculation.CalculateGst(Calculation.RoundOffDecimal(roomRateDate.RoomRate), roomRateDate.GstPercentage, gstType);

                roomRateDate.TotalRoomRate = Calculation.RoundOffDecimal(roomRateDate.RoomRate + roomRateDate.GstAmount);
                roomRateDate.CGST = Constants.Calculation.CalculateCGST(roomRateDate.GstPercentage);
                roomRateDate.CGSTAmount = Constants.Calculation.CalculateCGST(roomRateDate.GstAmount);
                roomRateDate.SGST = roomRateDate.CGST;
                roomRateDate.SGSTAmount = roomRateDate.CGSTAmount;
                roomRateResponse.BookedRoomRates.Add(roomRateDate);

                roomRateResponse.BookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRateDate.RoomRate);

                roomRateResponse.GstAmount = Calculation.RoundOffDecimal(roomRateResponse.GstAmount + roomRateDate.GstAmount);

                roomRateResponse.BookingAmountWithoutDiscount = Calculation.RoundOffDecimal(roomRateDate.RoomRateWithoutDiscount);

            }

            //checkout format - 24 hours/ night
            else
            {
                decimal bookingAmount = 0;
                decimal totalBookingAmount = 0;
                roomRateResponse.CalculateRoomRates = roomRatesCalculatedBy;
                //if checkin date room rates
                if (roomRatesCalculatedBy == Constants.Constants.CheckInRoomRates)
                {
                    var roomRateDate = new BookedRoomRate();
                    roomRateDate.BookingDate = checkInDate;
                    roomRateDate.GstType = gstType;
                    roomRateDate.RoomTypeId = roomTypeId;
                    //find custom rates
                    var customRoomRates = await _context.RoomRateDateWise.Where(x => x.IsActive == true && x.CompanyId == companyId && (x.FromDate <= checkInDate && x.ToDate >= checkInDate) && x.RoomTypeId == roomTypeId).OrderByDescending(x => x.RatePriority).FirstOrDefaultAsync();
                    if (customRoomRates == null)
                    {
                        //fetch standard room rates
                        var roomRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId && x.HourId == 0).FirstOrDefaultAsync();
                        if (roomRates == null)
                        {
                            return (400, "No Room Rates found", roomRateResponse);
                        }
                        else
                        {
                            roomRateDate.RoomRateWithoutDiscount = roomRates.RoomRate;                            
                                                      
                            //if any discount
                            if(discount > 0)
                            {
                                roomRateDate.DiscountType = discountType;
                                //discount type is percentage
                                if(discountType == Constants.Constants.DeductionByPercentage)
                                {
                                    roomRateDate.DiscountPercentage = discount;
                                    roomRateDate.DiscountAmount = Constants.Calculation.CalculatePercentage(roomRates.RoomRate, discount);
                                    roomRateDate.RoomRate = roomRates.RoomRate - roomRateDate.DiscountAmount;

                                }
                                //discount type is amount
                                else
                                {
                                    roomRateDate.DiscountAmount = discount;
                                    roomRateDate.RoomRate = roomRates.RoomRate - discount;
                                }
                            }
                            //no discount
                            else
                            {
                                roomRateDate.RoomRate = roomRates.RoomRate;
                            }
                            if (roomRateDate.RoomRate < 0)
                            {
                                return (400, "Room rate is cannot be less than discount amount", roomRateResponse);
                            }
                            roomRateResponse.DiscountTotalAmount = roomRateResponse.DiscountTotalAmount + roomRateDate.DiscountAmount;
                        }
                    }
                    else
                    {
                        roomRateDate.RoomRateWithoutDiscount = customRoomRates.RoomRate;

                        //if any discount
                        if (discount > 0)
                        {
                            roomRateDate.DiscountType = discountType;
                            //discount type is percentage
                            if (discountType == Constants.Constants.DeductionByPercentage)
                            {
                                roomRateDate.DiscountPercentage = discount;
                                roomRateDate.DiscountAmount = Constants.Calculation.CalculatePercentage(customRoomRates.RoomRate, discount);
                                roomRateDate.RoomRate = roomRateDate.RoomRate - roomRateDate.DiscountAmount;

                            }
                            //discount type is amount
                            else
                            {
                                roomRateDate.DiscountAmount = discount;
                                roomRateDate.RoomRate = customRoomRates.RoomRate - Constants.Calculation.CalculatePercentage(customRoomRates.RoomRate, discount);
                            }
                        }
                        else
                        {
                            roomRateDate.RoomRate = customRoomRates.RoomRate;
                        }
                        if (roomRateDate.RoomRate < 0)
                        {
                            return (400, "Room rate is cannot be less than discount amount", roomRateResponse);
                        }
                        roomRateResponse.DiscountTotalAmount = roomRateResponse.DiscountTotalAmount + roomRateDate.DiscountAmount;
                    }

                    if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                    {
                        var gstRangeMaster = GetApplicableGstRange(gstPercentage.ranges, (Calculation.RoundOffDecimal(roomRateDate.RoomRate)));
                        if (gstRangeMaster == null)
                        {
                            roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                        }
                        else
                        {
                            roomRateDate.GstPercentage = gstRangeMaster.TaxPercentage;
                        }
                    }
                    else
                    {
                        roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                    }
                    (roomRateDate.RoomRate, roomRateDate.GstAmount) = Calculation.CalculateGst(Calculation.RoundOffDecimal(roomRateDate.RoomRate), roomRateDate.GstPercentage, gstType);

                    roomRateDate.TotalRoomRate = Calculation.RoundOffDecimal(roomRateDate.RoomRate + roomRateDate.GstAmount);
                    roomRateDate.CGST = Constants.Calculation.CalculateCGST(roomRateDate.GstPercentage);
                    roomRateDate.CGSTAmount = Constants.Calculation.CalculateCGST(roomRateDate.GstAmount);
                    roomRateDate.SGST = roomRateDate.CGST;
                    roomRateDate.SGSTAmount = roomRateDate.CGSTAmount;
                    roomRateResponse.BookedRoomRates.Add(roomRateDate);


                    //set amount for day one if early check in applicable                    
                    bookingAmount = roomRateDate.RoomRate;
                    totalBookingAmount = roomRateDate.TotalRoomRate;

                    roomRateResponse.BookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRateDate.RoomRate);

                    roomRateResponse.GstAmount = Calculation.RoundOffDecimal(roomRateResponse.GstAmount + roomRateDate.GstAmount);

                    roomRateResponse.BookingAmountWithoutDiscount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmountWithoutDiscount + roomRateDate.RoomRateWithoutDiscount);

                    //set room rates for other dates
                    int nights = noOfNights - 1;

                    DateOnly currentDate = checkInDate.AddDays(1);
                    while (nights > 0)
                    {
                        var eachDateRoomRate = roomRateDate;
                        eachDateRoomRate.BookingDate = currentDate;
                        roomRateResponse.BookedRoomRates.Add(eachDateRoomRate);

                        roomRateResponse.BookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRateDate.RoomRate);


                        roomRateResponse.GstAmount = Calculation.RoundOffDecimal(roomRateResponse.GstAmount + roomRateDate.GstAmount);

                        roomRateResponse.DiscountAmount = roomRateResponse.DiscountAmount + roomRateDate.DiscountAmount;

                        roomRateResponse.BookingAmountWithoutDiscount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmountWithoutDiscount + roomRateDate.RoomRateWithoutDiscount);

                        nights--;
                        currentDate.AddDays(1);
                    }
                }

                else
                {
                    DateOnly currentDate = checkInDate;
                    
                    while (noOfNights > 0)
                    {
                        var roomRateDate = new BookedRoomRate();
                        roomRateDate.BookingDate = currentDate;
                        roomRateDate.GstType = gstType;
                        roomRateDate.RoomTypeId = roomTypeId;
                        var customRoomRates = await _context.RoomRateDateWise.Where(x => x.IsActive == true && x.CompanyId == companyId && (x.FromDate <= currentDate && x.ToDate >= currentDate) && x.RoomTypeId == roomTypeId).OrderByDescending(x => x.RatePriority).FirstOrDefaultAsync();
                        if (customRoomRates == null)
                        {
                            //fetch standard room rates
                            var roomRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId).FirstOrDefaultAsync();
                            if (roomRates == null)
                            {

                                return (400, "No Room Rates found", roomRateResponse);

                            }
                            else
                            {
                                roomRateDate.RoomRateWithoutDiscount = roomRates.RoomRate;

                                //if any discount
                                if (discount > 0)
                                {
                                    roomRateDate.DiscountType = discountType;
                                    //discount type is percentage
                                    if (discountType == Constants.Constants.DeductionByPercentage)
                                    {
                                        roomRateDate.DiscountPercentage = discount;
                                        roomRateDate.DiscountAmount = Constants.Calculation.CalculatePercentage(roomRates.RoomRate, discount);
                                        roomRateDate.RoomRate = roomRates.RoomRate - roomRateDate.DiscountAmount;

                                    }
                                    //discount type is amount
                                    else
                                    {
                                        roomRateDate.DiscountAmount = discount;
                                        roomRateDate.RoomRate = roomRates.RoomRate - discount;
                                    }
                                }
                                //no discount
                                else
                                {
                                    roomRateDate.RoomRate = roomRates.RoomRate;
                                }
                                if (roomRateDate.RoomRate < 0)
                                {
                                    return (400, "Room rate is cannot be less than discount amount", roomRateResponse);
                                }

                                roomRateResponse.DiscountTotalAmount = roomRateResponse.DiscountTotalAmount + roomRateDate.DiscountAmount;

                            }
                        }
                        else
                        {
                            roomRateDate.RoomRateWithoutDiscount = customRoomRates.RoomRate;

                            //if any discount
                            if (discount > 0)
                            {
                                roomRateDate.DiscountType = discountType;
                                //discount type is percentage
                                if (discountType == Constants.Constants.DeductionByPercentage)
                                {
                                    roomRateDate.DiscountPercentage = discount;
                                    roomRateDate.DiscountAmount = Constants.Calculation.CalculatePercentage(customRoomRates.RoomRate, discount);
                                    roomRateDate.RoomRate = customRoomRates.RoomRate - roomRateDate.DiscountAmount ;

                                }
                                //discount type is amount
                                else
                                {
                                    roomRateDate.DiscountAmount = discount;
                                    roomRateDate.RoomRate = customRoomRates.RoomRate - Constants.Calculation.CalculatePercentage(customRoomRates.RoomRate, discount);
                                }
                            }
                            else
                            {
                                roomRateDate.RoomRate = customRoomRates.RoomRate;
                            }
                            if (roomRateDate.RoomRate < 0)
                            {
                                return (400, "Room rate is cannot be less than discount amount", roomRateResponse);
                            }
                            roomRateResponse.DiscountTotalAmount = roomRateResponse.DiscountTotalAmount + roomRateDate.DiscountAmount;
                            
                        }

                        if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                        {
                            var gstRangeMaster = GetApplicableGstRange(gstPercentage.ranges, (Calculation.RoundOffDecimal(roomRateDate.RoomRate)));
                            if (gstRangeMaster == null)
                            {
                                roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                            }
                            else
                            {
                                roomRateDate.GstPercentage = gstRangeMaster.TaxPercentage;
                            }
                        }
                        else
                        {
                            roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                        }
                        
                        (roomRateDate.RoomRate, roomRateDate.GstAmount) = Calculation.CalculateGst(Calculation.RoundOffDecimal(roomRateDate.RoomRate), roomRateDate.GstPercentage, gstType);

                       
                        roomRateDate.TotalRoomRate = Calculation.RoundOffDecimal(roomRateDate.RoomRate + roomRateDate.GstAmount);

                        //set amount for day one if early check in applicable
                        if (currentDate == checkInDate)
                        {
                            bookingAmount = roomRateDate.RoomRate;
                            totalBookingAmount = roomRateDate.TotalRoomRate;
                        }

                        roomRateDate.CGST = Constants.Calculation.CalculateCGST(roomRateDate.GstPercentage);
                        roomRateDate.CGSTAmount = Constants.Calculation.CalculateCGST(roomRateDate.GstAmount);
                        roomRateDate.SGST = roomRateDate.CGST;
                        roomRateDate.SGSTAmount = roomRateDate.CGSTAmount;
                        roomRateResponse.BookedRoomRates.Add(roomRateDate);

                        roomRateResponse.BookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRateDate.RoomRate);

                        roomRateResponse.GstAmount = Calculation.RoundOffDecimal(roomRateResponse.GstAmount + roomRateDate.GstAmount);

                        roomRateResponse.BookingAmountWithoutDiscount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmountWithoutDiscount + roomRateDate.RoomRateWithoutDiscount);

                        noOfNights--;
                        currentDate = currentDate.AddDays(1);
                    }


                   
                }

                //check earlycheckin 
                if (property.IsDefaultCheckInTimeApplicable && property.IsEarlyCheckInPolicyEnable && checkOutFormat == Constants.Constants.NightFormat)
                {
                    var extraPolicy = await _context.ExtraPolicies.Where(x => x.IsActive == true && x.Status == Constants.Constants.EARLYCHECKIN && x.CompanyId == companyId).ToListAsync();
                    if (extraPolicy.Count == 0)
                    {
                        return (400, "Early checkin policies not found", roomRateResponse);
                    }

                    int differenceHours = DateTimeMethod.FindEarlyCheckInHourDifference(property.CheckInTime, checkInTime);
                    if (differenceHours > 0)
                    {
                        var applicablePolicy = extraPolicy.FirstOrDefault(x => x.FromHour <= differenceHours && x.ToHour > differenceHours);
                        if (applicablePolicy == null)
                        {
                            return (400, "Suitable early checkin policy not found", roomRateResponse);
                        }
                        else
                        {
                            roomRateResponse.IsEarlyCheckIn = true;
                            roomRateResponse.EarlyCheckInPolicyName = applicablePolicy.PolicyName;
                            roomRateResponse.EarlyCheckInDeductionBy = applicablePolicy.DeductionBy;
                            roomRateResponse.EarlyCheckInApplicableOn = applicablePolicy.ChargesApplicableOn;
                            roomRateResponse.EarlyCheckInFromHour = applicablePolicy.FromHour;
                            roomRateResponse.EarlyCheckInToHour = applicablePolicy.ToHour;
                            if (applicablePolicy.DeductionBy == Constants.Constants.DeductionByAmount)
                            {

                                roomRateResponse.EarlyCheckInCharges = applicablePolicy.Amount;
                            }
                            else
                            {
                                if (applicablePolicy.ChargesApplicableOn == Constants.Constants.ChargesOnTotalAmount)
                                {
                                    roomRateResponse.EarlyCheckInCharges = Constants.Calculation.CalculatePercentage(totalBookingAmount, applicablePolicy.Amount);

                                }
                                else
                                {
                                    roomRateResponse.EarlyCheckInCharges = Constants.Calculation.CalculatePercentage(bookingAmount, applicablePolicy.Amount);
                                }
                            }
                        }
                    }

                }



                //check late check out
                if (property.IsDefaultCheckOutTimeApplicable && property.IsLateCheckOutPolicyEnable && checkOutFormat == Constants.Constants.NightFormat)
                {
                    var extraPolicy = await _context.ExtraPolicies.Where(x => x.IsActive == true && x.Status == Constants.Constants.LATECHECKOUT && x.CompanyId == companyId).ToListAsync();
                    if (extraPolicy.Count == 0)
                    {
                        return (400, "Late check out policies not found", roomRateResponse);
                    }

                    int differenceHours = DateTimeMethod.FindLateCheckOutHourDifference(property.CheckOutTime, checkOutTime);
                    if (differenceHours > 0)
                    {
                        var applicablePolicy = extraPolicy.FirstOrDefault(x => x.FromHour <= differenceHours && x.ToHour > differenceHours);
                        if (applicablePolicy == null)
                        {
                            return (400, "Suitable late checkout policy not found", roomRateResponse);
                        }
                        else
                        {
                            roomRateResponse.IsLateCheckOut = true;
                            roomRateResponse.LateCheckOutPolicyName = applicablePolicy.PolicyName;
                            roomRateResponse.LateCheckOutDeductionBy = applicablePolicy.DeductionBy;
                            roomRateResponse.LateCheckOutApplicableOn = applicablePolicy.ChargesApplicableOn;
                            roomRateResponse.LateCheckOutFromHour = applicablePolicy.FromHour;
                            roomRateResponse.LateCheckOutToHour = applicablePolicy.ToHour;
                            if (applicablePolicy.DeductionBy == Constants.Constants.DeductionByAmount)
                            {

                                roomRateResponse.LateCheckOutCharges = applicablePolicy.Amount;
                            }
                            else
                            {
                                if (applicablePolicy.ChargesApplicableOn == Constants.Constants.ChargesOnTotalAmount)
                                {
                                    roomRateResponse.LateCheckOutCharges = Constants.Calculation.CalculatePercentage(totalBookingAmount, applicablePolicy.Amount);

                                }
                                else
                                {
                                    roomRateResponse.LateCheckOutCharges = Constants.Calculation.CalculatePercentage(bookingAmount, applicablePolicy.Amount);
                                }
                            }
                        }
                    }

                }

            }

            roomRateResponse.TotalBookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRateResponse.GstAmount );

            //set discount
            roomRateResponse.DiscountType = discountType;
            if(discount > 0)
            {
                if (discountType == Constants.Constants.DeductionByAmount)
                {
                    roomRateResponse.DiscountAmount = discount;
                }
                else
                {
                    roomRateResponse.DiscountPercentage = discount;
                }
            }
            

                //total amount
            roomRateResponse.AllRoomsAmount = Calculation.RoundOffDecimal((noOfRooms * roomRateResponse.BookingAmount) + (noOfRooms * roomRateResponse.EarlyCheckInCharges) + (noOfRooms * roomRateResponse.LateCheckOutCharges));
            roomRateResponse.AllRoomsGst = Calculation.RoundOffDecimal(noOfRooms * roomRateResponse.GstAmount);
            roomRateResponse.TotalRoomsAmount = Calculation.RoundOffDecimal(roomRateResponse.AllRoomsAmount + roomRateResponse.AllRoomsGst);

            return (200, "Room rate fetched successfully", roomRateResponse);


        }

       


        private async Task<GstMaster> GetGstPercetage(string service)        {
            
            var gstPercentage = await _context.GstMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ApplicableServices == service).FirstOrDefaultAsync();

            if (gstPercentage == null)
            {
                return null;
            }
            else
            {
                if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                {
                    var ranges = await _context.GstRangeMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.GstId == gstPercentage.Id).ToListAsync();

                    gstPercentage.ranges = ranges;

                    return gstPercentage;
                }
                else
                {
                    return gstPercentage;
                }
            }
        }

        private GstRangeMaster? GetApplicableGstRange(List<GstRangeMaster> gstRanges, decimal bookingAmount)
        {
            return gstRanges.FirstOrDefault(range => bookingAmount >= range.RangeStart && bookingAmount <= range.RangeEnd);
        }

        //CALCULATE AGENT COMMISION
        private async Task<(int Code, string Message, AgentCommissionResponse? Response)> CalculateAgentCommisionAsync(int agentId, decimal bookingAmount, decimal totalAmountWithGst)
        {
            var agentCommissionResponse = new AgentCommissionResponse();
            if (agentId == 0)
            {
                return (400, "Invalid data", agentCommissionResponse);
              
            }
            var agentDetails = await _context.AgentDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.AgentId == agentId);
            if (agentDetails == null)
            {
                return (500, "No agent found", agentCommissionResponse);
                
            }
            agentCommissionResponse.AgentGstType = agentDetails.GstType;
            agentCommissionResponse.AgentCommissionPercentage = agentDetails.Commission;
            agentCommissionResponse.TdsPercentage = agentDetails.Tds;
            agentCommissionResponse.TcsPercentage = agentDetails.Tcs;
            if (agentDetails.GstType == Constants.Constants.WithGst)
            {
                //commmision                    
                agentCommissionResponse.AgentCommisionAmount = Calculation.CalculatePercentage(totalAmountWithGst, agentCommissionResponse.AgentCommissionPercentage);

                //tcs
                agentCommissionResponse.TdsAmount = Calculation.CalculatePercentage(totalAmountWithGst, agentCommissionResponse.TdsPercentage);

                //tds
                agentCommissionResponse.TcsAmount = Calculation.CalculatePercentage(totalAmountWithGst, agentCommissionResponse.TcsPercentage);
            }
            else
            {
                //commmision                    
                agentCommissionResponse.AgentCommisionAmount = Calculation.CalculatePercentage(bookingAmount, agentCommissionResponse.AgentCommissionPercentage);

                //tcs
                agentCommissionResponse.TdsAmount = Calculation.CalculatePercentage(bookingAmount, agentCommissionResponse.TdsPercentage);

                //tds
                agentCommissionResponse.TcsAmount = Calculation.CalculatePercentage(bookingAmount, agentCommissionResponse.TcsPercentage);
            }
            return (200, "Agent Commission calculated successfully", agentCommissionResponse);
            
        }

        //CREATE INVOICE HISTORY
        private InvoiceHistory CreateInvoiceHistory(BookingDetail booking, PaymentDetails payment, decimal paymentUsed)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            var currentDate = DateTime.Now;
            var invoiceHistory = new InvoiceHistory();
            Constants.Constants.SetMastersDefault(invoiceHistory, companyId, userId, currentDate);
            invoiceHistory.InvoiceNo = booking.InvoiceNo;
            invoiceHistory.InvoiceDate = booking.InvoiceDate;
            invoiceHistory.PaymentId = payment.PaymentId;
            invoiceHistory.BookingId = booking.BookingId;
            invoiceHistory.ReservationNo = booking.ReservationNo;
            invoiceHistory.RoomId = booking.RoomId;
            invoiceHistory.PaymentAmount = payment.PaymentAmount - payment.TransactionAmount;
            invoiceHistory.PaymentStatus = payment.PaymentStatus;
            invoiceHistory.PaymentAmountUsed = paymentUsed;
            invoiceHistory.PaymentLeft = payment.PaymentLeft;
            invoiceHistory.RefundAmount = payment.RefundAmount;
            return invoiceHistory;

        }

        //BOOKED ROOM RATES TOTAL
        private (decimal BookingAmount, decimal GstAmount, decimal TotalBookingAmount) CalculateTotalBookedRoomRate(List<BookedRoomRate> roomRates)
        {
            decimal BookingAmount = 0;
            decimal GstAmount = 0;
            decimal TotalBookingAmount = 0;
            foreach (var item in roomRates)
            {
                BookingAmount = BookingAmount + item.RoomRate;
                GstAmount = GstAmount + item.GstAmount;
                TotalBookingAmount = TotalBookingAmount + item.TotalRoomRate;
            }
            return (BookingAmount, GstAmount, TotalBookingAmount);
        }

        private async Task<PaymentCheckOutSummary> CalculateSummaryForCheckOut(ReservationDetails reservationDetails, List<BookingDetail> bookings, List<PaymentDetails> paymentDetails, bool isAllRoomCheckOut)
        {

            var summary = new PaymentCheckOutSummary();

            summary.TotalRooms = bookings.Count;
            foreach (var item in bookings)
            {
                summary.BookingAmount += item.BookingAmount;
                summary.GstAmount += item.GstAmount;
                summary.TotalBookingAmount += item.TotalBookingAmount;
                summary.EarlyCheckIn += item.EarlyCheckInCharges;
                summary.LateCheckOut += item.LateCheckOutCharges;
                summary.ServicesAmount += item.ServicesAmount;
                summary.ServiceAmountGst += item.ServicesTaxAmount;
                summary.TotalServiceAmount += item.TotalServicesAmount;
                summary.TotalTaxAmount = summary.TotalTaxAmount + item.GstAmount + item.ServicesTaxAmount;
                summary.CheckOutDiscoutAmount += item.CheckOutDiscoutAmount;
            }
            summary.AgentServiceCharge = reservationDetails.AgentServiceCharge;
            summary.AgentServiceGst = reservationDetails.AgentServiceGstAmount;
            summary.AgentServiceTotal = reservationDetails.AgentTotalServiceCharge;

            

           

            foreach (var pay in paymentDetails)
            {
                if (pay.PaymentStatus == Constants.Constants.AgentPayment || pay.PaymentStatus == Constants.Constants.AdvancePayment)
                {
                    
                    summary.AdvanceAmount = summary.AdvanceAmount + pay.PaymentLeft;
                    summary.TransactionsAmount += pay.TransactionAmount;
                }
                else
                {
                    //status wise payment summary
                    if (pay.PaymentFormat == Constants.Constants.RoomWisePayment)
                    {
                        if (bookings.Select(x => x.BookingId).Contains(pay.BookingId))
                        {
                            summary.ReceivedAmount = summary.ReceivedAmount + pay.PaymentLeft;
                            summary.TransactionsAmount += pay.TransactionAmount;
                        }
                    }
                    else
                    {
                        summary.ReceivedAmount = summary.ReceivedAmount + pay.PaymentLeft;
                        summary.TransactionsAmount += pay.TransactionAmount;
                    }
                }
            }

            summary.TotalAmount = summary.TotalBookingAmount + summary.EarlyCheckIn + summary.LateCheckOut + summary.TotalServiceAmount +  summary.AgentServiceTotal;

            summary.TotalAmountPaid = summary.ReceivedAmount + summary.AdvanceAmount;

            summary.TotalBill = summary.TotalAmount - summary.CheckOutDiscoutAmount;

            //var balance = (summary.TotalBill) - (summary.AdvanceAmount + summary.ReceivedAmount);
            //if (balance > 0)
            //{
            //    summary.BalanceAmount = balance;
            //}
            //else
            //{
            //    if (isAllRoomCheckOut)
            //    {
            //        summary.RefundAmount = Math.Abs(balance);
            //    }
            //    else
            //    {
            //        summary.ResidualAmount = Math.Abs(balance);
            //    }
                
            //}


            return summary;
        }

        private async Task<PaymentSummary> CalculateSummary(ReservationDetails reservationDetails, List<BookingDetail> bookings)
        {
           
            var summary = new PaymentSummary();


            foreach (var item in bookings)
            {
                //booking
                summary.TotalRoomAmount = summary.TotalRoomAmount + item.BookingAmount;
                summary.TotalGstAmount = summary.TotalGstAmount + item.GstAmount;
                summary.TotalAmount = summary.TotalAmount + item.TotalBookingAmount;
                summary.TotalAllAmount = summary.TotalAllAmount + item.TotalAmount;

                //advanc services
                summary.RoomServiceAmount = summary.RoomServiceAmount + item.ServicesAmount;
                summary.RoomServiceTaxAmount = summary.RoomServiceTaxAmount + item.ServicesTaxAmount;
                summary.TotalRoomServicesAmount = summary.TotalRoomServicesAmount + item.TotalServicesAmount;

                //total tax = room tax + service tax
                summary.TotalTaxAmount = summary.TotalTaxAmount + item.GstAmount + item.ServicesTaxAmount;

                summary.EarlyCheckIn = summary.EarlyCheckIn + item.EarlyCheckInCharges;
                summary.LateCheckOut = summary.LateCheckOut + item.LateCheckOutCharges;
            }
            summary.AgentServiceCharge = reservationDetails.AgentServiceCharge;
            summary.AgentServiceGst = reservationDetails.AgentServiceGstAmount;
            summary.AgentServiceTotal = reservationDetails.AgentTotalServiceCharge;

            summary.TotalPayable = summary.TotalAllAmount + summary.AgentServiceTotal ;

            var payments = await _context.PaymentDetails.Where(x => x.IsActive == true && x.IsReceived == false && x.CompanyId == companyId && x.ReservationNo == reservationDetails.ReservationNo).OrderBy(x => x.PaymentId).ToListAsync();

            foreach (var pay in payments)
            {
                if (pay.PaymentStatus == Constants.Constants.AgentPayment)
                {
                    summary.AgentPaid = summary.AgentPaid + pay.PaymentLeft;
                }
                else if (pay.PaymentStatus == Constants.Constants.AdvancePayment)
                {
                    summary.AdvanceAmount = summary.AdvanceAmount + pay.PaymentLeft;
                }
                else
                {
                    //status wise payment summary
                    if (pay.PaymentFormat == Constants.Constants.RoomWisePayment)
                    {
                        if (bookings.Select(x => x.BookingId).Contains(pay.BookingId))
                        {
                            summary.ReceivedAmount = summary.ReceivedAmount + pay.PaymentLeft;
                        }
                    }
                    else
                    {
                        summary.ReceivedAmount = summary.ReceivedAmount + pay.PaymentLeft;
                    }
                }
            }
            var balance = (summary.TotalPayable) - (summary.AgentPaid + summary.AdvanceAmount + summary.ReceivedAmount);
            if (balance > 0)
            {
                summary.BalanceAmount = balance;
            }
            else
            {
                summary.RefundAmount = Math.Abs(balance);
            }


            return summary;
        }

        private PaymentCheckInSummary CalculateCheckInSummary(ReservationDetails reservationDetails, List<BookingDetail> bookings, List<PaymentDetails> payments)
        {

            var summary = new PaymentCheckInSummary();


            foreach (var item in bookings)
            {
                //booking
                summary.RoomAmount = summary.RoomAmount + item.BookingAmount;
                summary.GstAmount = summary.GstAmount + item.GstAmount;
                summary.EarlyCheckIn = summary.EarlyCheckIn + item.EarlyCheckInCharges;
                summary.LateCheckOut = summary.LateCheckOut + item.LateCheckOutCharges;

                //advance services
                summary.RoomServicesAmount = summary.RoomServicesAmount + item.TotalServicesAmount;
                
            }
            summary.AgentServiceCharge = reservationDetails.AgentTotalServiceCharge;

            foreach (var pay in payments)
            {
                summary.TransactionCharges = summary.TransactionCharges + pay.TransactionAmount;
                if (pay.PaymentStatus == Constants.Constants.AgentPayment || pay.PaymentStatus == Constants.Constants.AdvancePayment)
                {
                    summary.AdvanceAmount = summary.AdvanceAmount + pay.PaymentLeft;
                    
                }
               
                else
                {
                    //status wise payment summary
                    if (pay.PaymentFormat == Constants.Constants.RoomWisePayment)
                    {
                        if (bookings.Select(x => x.BookingId).Contains(pay.BookingId))
                        {
                            summary.ReceivedAmount = summary.ReceivedAmount + pay.PaymentLeft;
                        }
                    }
                    else
                    {
                        summary.ReceivedAmount = summary.ReceivedAmount + pay.PaymentLeft;
                    }
                   
                }
            }

            summary.TotalAmount = summary.RoomAmount + summary.GstAmount + summary.EarlyCheckIn + summary.LateCheckOut + summary.AgentServiceCharge + summary.TransactionCharges;

            var balance = (summary.TotalAmount) - (summary.AdvanceAmount + summary.ReceivedAmount);
            if (balance > 0)
            {
                summary.BalanceAmount = balance;
            }
            else
            {
                summary.RefundAmount = Math.Abs(balance);
            }


            return summary;
        }

        private bool IsAgentPaymentLeft(List<PaymentDetails> payments, int paymentIndex)
        {
            return (payments[paymentIndex].PaymentStatus == Constants.Constants.AgentPayment && payments[paymentIndex].IsReceived == false && payments[paymentIndex].PaymentLeft > 0);
        }

        private bool IsAdvancePaymentLeft(List<PaymentDetails> payments, int paymentIndex)
        {
            return (payments[paymentIndex].PaymentStatus == Constants.Constants.AdvancePayment && payments[paymentIndex].IsReceived == false && payments[paymentIndex].PaymentLeft > 0);
        }

        private bool IsReceivedPaymentLeft(List<PaymentDetails> payments, int paymentIndex)
        {
            return (payments[paymentIndex].PaymentStatus != Constants.Constants.AdvancePayment && payments[paymentIndex].PaymentStatus != Constants.Constants.AgentPayment && payments[paymentIndex].IsReceived == false && payments[paymentIndex].PaymentLeft > 0 && payments[paymentIndex].BookingId == 0);
        }

        private List<decimal> EquallyDivideAmount(decimal amount, int length)
        {
            decimal equallyDivide = Constants.Calculation.RoundOffDecimal(amount / length);
            List<decimal> equallyDivideArr = new List<decimal>();
            decimal totalDivide = 0;
            for (int i = 0; i < length; i++)
            {
                equallyDivideArr.Add(equallyDivide);
                totalDivide += equallyDivide;
            }
            decimal balanceDiv = Constants.Calculation.RoundOffDecimal(amount - totalDivide);
            if (balanceDiv > 0)
            {
                equallyDivideArr[equallyDivideArr.Count - 1] = equallyDivideArr[equallyDivideArr.Count - 1] + balanceDiv;
            }
            else
            {
                equallyDivideArr[equallyDivideArr.Count - 1] = equallyDivideArr[equallyDivideArr.Count - 1] - Math.Abs(balanceDiv);
            }
            return equallyDivideArr;
        }

        private decimal EquallyDivideValue(decimal amount, int length)
        {
            return Constants.Calculation.RoundOffDecimal(amount / length);
        }

        private async Task<(int Code,string Message,int Result)> AddUpdateGuest(GuestDetails guestDetails, int companyId, int userId, DateTime currentDate)
        {
            if (guestDetails.GuestId == 0)
            {
                Constants.Constants.SetMastersDefault(guestDetails, companyId, userId, currentDate);
                await _context.GuestDetails.AddAsync(guestDetails);
                await _context.SaveChangesAsync();
                return (200, "Guest addess successfully", guestDetails.GuestId);
            }
            else
            {
                var guest = await _context.GuestDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.GuestId == guestDetails.GuestId);
                if (guest == null)
                {
                    return (400, "Guest details not found", -1);

                    
                }
                else
                {

                    guest.GuestName = guestDetails.GuestName;
                    guest.Nationality = guestDetails.Nationality;
                    guest.StateName = guestDetails.StateName;
                    guest.Address = guestDetails.Address;
                    guest.City = guestDetails.City;
                    guest.PhoneNumber = guestDetails.PhoneNumber;
                    guest.Email = guestDetails.Email;
                    guest.CityId = guestDetails.CityId;
                    guest.StateId = guestDetails.StateId;
                    guest.CountryId = guestDetails.CountryId;
                    guest.Gender = guestDetails.Gender;
                    guest.IdType = guestDetails.IdType;
                    guest.IdNumber = guestDetails.IdNumber;
                    guest.UpdatedDate = currentDate;

                    _context.GuestDetails.Update(guest);
                    await _context.SaveChangesAsync();

                    return (200, "Guest updated successfully", guestDetails.GuestId);
                }
            }
        }
        private (decimal agentAdvance, decimal advance, decimal adjustedAmount) CalculatePayment(List<PaymentDetails> paymentDetails, List<BookingDetail> bookings)
        {
            decimal agentAdvance = 0;
            decimal advance = 0;
            decimal received = 0;
            foreach (var pay in paymentDetails)
            {
                if (pay.PaymentStatus == Constants.Constants.AdvancePayment && pay.IsReceived == false)
                {
                    advance += pay.PaymentLeft;
                }
                else if (pay.PaymentStatus == Constants.Constants.ReceivedPayment && pay.IsReceived == false)
                {
                    if (pay.BookingId > 0 && pay.RoomId > 0)
                    {
                        if (bookings.Select(x => x.BookingId).Contains(pay.BookingId))
                        {
                            received += pay.PaymentLeft;
                        }
                    }
                    else
                    {
                        received += pay.PaymentLeft;
                    }

                }
                else if (pay.PaymentStatus == Constants.Constants.AgentPayment && pay.IsReceived == false)
                {
                    agentAdvance += pay.PaymentLeft;
                }
            }
            return (agentAdvance, advance, received);
        }

        //[HttpGet("GetCancellableBookings")]
        //public async Task<IActionResult> GetCancellableBookings(string reservationNo, int guestId)
        //{
        //    try
        //    {
        //        var checkInResponse = new CheckInResponse();
        //        if (string.IsNullOrEmpty(reservationNo) || guestId == 0)
        //        {
        //            return Ok(new { Code = 500, Message = "Invalid data" });
        //        }
        //        int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
        //        int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

        //        //Get reservation details
        //        checkInResponse.ReservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);
        //        if (checkInResponse.ReservationDetails == null)
        //        {
        //            return Ok(new { Code = 400, Message = $"No details for {reservationNo} reservation" });
        //        }

        //        checkInResponse.GuestDetails = await _context.GuestDetails.Where(x => x.CompanyId == companyId && x.IsActive && x.GuestId == guestId).FirstOrDefaultAsync();
        //        if (checkInResponse.GuestDetails == null)
        //        {
        //            return Ok(new { Code = 400, Message = $"No details for {reservationNo} reservation" });
        //        }

        //        var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo).ToListAsync();


        //        checkInResponse.BookingDetailCheckInDTO = await (
        //                                from booking in _context.BookingDetail
        //                                join room in _context.RoomMaster
        //                                    on new { RoomId = booking.RoomId, CompanyId = companyId }
        //                                    equals new { RoomId = room.RoomId, CompanyId = room.CompanyId } into rooms
        //                                from bookrooms in rooms.DefaultIfEmpty()
        //                                join category in _context.RoomCategoryMaster
        //                                    on new { RoomTypeId = booking.RoomTypeId, CompanyId = companyId }
        //                                    equals new { RoomTypeId = category.Id, CompanyId = category.CompanyId }

        //                                where booking.IsActive == true
        //                                    && booking.CompanyId == companyId
        //                                    && booking.ReservationNo == reservationNo
        //                                select new BookingDetailCheckInDTO
        //                                {
        //                                    BookingId = booking.BookingId,
        //                                    GuestId = booking.GuestId,
        //                                    RoomId = booking.RoomId,
        //                                    RoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
        //                                    OriginalRoomId = booking.RoomId,
        //                                    OriginalRoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
        //                                    RoomTypeId = booking.RoomTypeId,
        //                                    RoomCategoryName = category.Type,
        //                                    OriginalRoomTypeId = booking.RoomTypeId,
        //                                    OriginalRoomCategoryName = category.Type,
        //                                    CheckInDate = booking.CheckInDate.ToString("yyyy-MM-dd"),
        //                                    CheckInTime = booking.CheckInTime,
        //                                    CheckOutDate = booking.CheckOutDate.ToString("yyyy-MM-dd"),
        //                                    CheckOutTime = booking.CheckOutTime,
        //                                    CheckInDateTime = booking.CheckInDateTime,
        //                                    CheckOutDateTime = booking.CheckOutDateTime,
        //                                    NoOfNights = booking.NoOfNights,
        //                                    NoOfHours = booking.NoOfHours,
                                            
        //                                    Status = booking.Status,
        //                                    Remarks = booking.Remarks,
        //                                    ReservationNo = booking.ReservationNo,
        //                                    UserId = booking.UserId,
        //                                    CompanyId = booking.CompanyId,
        //                                    BookingAmount = booking.BookingAmount,
        //                                    GstType = booking.GstType,
        //                                    GstAmount = booking.GstAmount,
        //                                    TotalBookingAmount = booking.TotalBookingAmount,
        //                                    BookingSource = booking.BookingSource,
        //                                    ReservationDate = booking.ReservationDate.ToString("yyyy-MM-dd"),
        //                                    ReservationTime = booking.ReservationTime,
        //                                    ReservationDateTime = booking.ReservationDateTime,
        //                                    Pax = booking.Pax,
        //                                    OriginalPax = booking.Pax,
        //                                    IsSameGuest = booking.PrimaryGuestId == booking.GuestId ? true : false,
        //                                    OriginalReservationDateTime = booking.ReservationDateTime,
        //                                    OriginalReservationDate = booking.ReservationDate.ToString("yyyy-MM-dd"),
        //                                    OriginalReservationTime = booking.ReservationTime,
        //                                    OriginalCheckInDate = booking.CheckInDate.ToString("yyyy-MM-dd"),
        //                                    OriginalCheckInTime = booking.CheckInTime,
        //                                    OriginalCheckOutDate = booking.CheckOutDate.ToString("yyyy-MM-dd"),
        //                                    OriginalCheckOutTime = booking.CheckOutTime,
        //                                    CheckOutFormat = booking.CheckoutFormat,
        //                                    IsCheckBox = booking.Status != Constants.Constants.CheckOut && _context.AdvanceServices.Any(s => s.BookingId == booking.BookingId && s.CompanyId == companyId && s.IsActive)
        //                                }
        //                            ).ToListAsync();

        //        foreach (var item in checkInResponse.BookingDetailCheckInDTO)
        //        {
        //            item.BookedRoomRates = roomRates.Where(x => x.BookingId == item.BookingId).ToList();
        //            item.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == item.GuestId) ?? new GuestDetails();
        //        }



        //        //payment details
        //        checkInResponse.PaymentDetails = await (from x in _context.PaymentDetails
        //                                                join room in _context.RoomMaster on x.RoomId equals room.RoomId into roomT
        //                                                from rm in roomT.DefaultIfEmpty()
        //                                                where x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo
        //                                                select new PaymentDetails
        //                                                {
        //                                                    PaymentId = x.PaymentId,
        //                                                    BookingId = x.BookingId,
        //                                                    ReservationNo = x.ReservationNo,
        //                                                    PaymentDate = x.PaymentDate,
        //                                                    PaymentMethod = x.PaymentMethod,
        //                                                    TransactionId = x.TransactionId,
        //                                                    PaymentStatus = x.PaymentStatus,
        //                                                    PaymentType = x.PaymentType,
        //                                                    BankName = x.BankName,
        //                                                    PaymentReferenceNo = x.PaymentReferenceNo,
        //                                                    PaidBy = x.PaidBy,
        //                                                    Remarks = x.Remarks,
        //                                                    Other1 = x.Other1,
        //                                                    Other2 = x.Other2,
        //                                                    IsActive = x.IsActive,
        //                                                    IsReceived = x.IsReceived,
        //                                                    RoomId = x.RoomId,
        //                                                    UserId = x.UserId,
        //                                                    PaymentFormat = x.PaymentFormat,
        //                                                    RefundAmount = x.RefundAmount,
        //                                                    PaymentAmount = x.PaymentAmount,
        //                                                    CreatedDate = x.CreatedDate,
        //                                                    UpdatedDate = x.UpdatedDate,
        //                                                    CompanyId = x.CompanyId,
        //                                                    RoomNo = rm != null ? rm.RoomNo : ""
        //                                                }).ToListAsync();



        //        //payment summary
        //        var paymentSummary = new PaymentSummary();
        //        paymentSummary.TotalRoomAmount = checkInResponse.ReservationDetails.TotalRoomPayment;
        //        paymentSummary.TotalGstAmount = checkInResponse.ReservationDetails.TotalGst;
        //        paymentSummary.TotalAmount = checkInResponse.ReservationDetails.TotalAmount;
        //        paymentSummary.AgentPaid = checkInResponse.PaymentDetails.Where(x => x.PaymentStatus == Constants.Constants.AgentPayment).Sum(x => x.PaymentAmount);
        //        paymentSummary.AdvanceAmount = checkInResponse.PaymentDetails.Where(x => x.PaymentStatus == Constants.Constants.AdvancePayment).Sum(x => x.PaymentAmount);
        //        paymentSummary.ReceivedAmount = checkInResponse.PaymentDetails.Where(x => x.PaymentStatus == Constants.Constants.ReceivedPayment).Sum(x => x.PaymentAmount);
        //        var balance = paymentSummary.TotalAmount - (paymentSummary.AgentPaid + paymentSummary.AdvanceAmount + paymentSummary.ReceivedAmount);
        //        paymentSummary.BalanceAmount = balance > 0 ? balance : 0;
        //        paymentSummary.RefundAmount = balance < 0 ? Math.Abs(balance) : 0;

        //        //checkInResponse.PaymentSummary = paymentSummary;



        //        return Ok(new { Code = 200, Message = "Data fetched successfully", data = checkInResponse });
        //    }
        //    catch (Exception)
        //    {
        //        return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
        //    }
        //}


        [HttpGet("GetCheckAvailabilityFormData")]
        public async Task<IActionResult> GetCheckAvailabilityFormData()
        {
            try
            {
                var roomCategories = await _context.RoomCategoryMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).Select(x => new
                {
                    Id = x.Id,
                    Type = x.Type
                }).ToListAsync();

                var serviceStatus = await _context.ServicesStatus.ToListAsync();

                var result = new
                {
                    RoomCategories = roomCategories,
                    ServicesStatus = serviceStatus
                };

                return Ok(new { Code = 200, Message = "Data found",data = result });
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("RoomAvailaibility")]
        public async Task<IActionResult> RoomAvailaibility(DateOnly checkInDate, string checkInTime, DateOnly checkOutDate, string checkOutTime, string pageName = "", int roomTypeId = 0, string roomStatus = "")
        {
            try
            {

                if (checkInDate == null || checkOutDate == null || checkInDate == DateOnly.MinValue || checkOutDate == DateOnly.MinValue || checkInDate == Constants.Constants.DefaultDate || checkOutDate == Constants.Constants.DefaultDate)
                {
                    var result = await (from room in _context.RoomMaster
                                        join category in _context.RoomCategoryMaster on room.RoomTypeId equals category.Id
                                        where room.IsActive == true && room.CompanyId == companyId
                                        select new
                                        {
                                            RoomId = room.RoomId,
                                            RoomNo = room.RoomNo,
                                            RoomDesc = room.Description,
                                            CategoryId = room.RoomTypeId,
                                            RoomCategory = category.Type,
                                            RoomStatus = "Clean"
                                        }).ToListAsync();
                    return Ok(new { Code = 200, message = "Room availability retrieved successfully.", data = result, AvailableRooms = result.Count });
                }
                else
                {
                    DataSet dataSet = await GetRoomAvailability(checkInDate, checkInTime, checkOutDate, checkOutTime, pageName, roomTypeId, 0,roomStatus);
                    if (dataSet == null)
                    {
                        return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                    }
                    var rows = new List<Dictionary<string, object>>();
                    
                    var dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            dict[col.ColumnName] = row[col];
                        }
                        rows.Add(dict);
                    }

                    var countRows = new List<Dictionary<string, object>>();
                    var dataTable2 = dataSet.Tables[1];
                    foreach (DataRow row in dataTable2.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dataTable2.Columns)
                        {
                            dict[col.ColumnName] = row[col];
                        }
                        countRows.Add(dict);
                    }
                    return Ok(new { Code = 200, message = "Room availability retrieved successfully.", data = rows, countRows = countRows });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }



        [HttpPost("CheckRoomAvailabilityOnEdit")]
        public async Task<IActionResult> CheckRoomAvailabilityOnEdit([FromBody] RoomEditDTO roomEditDTO)
        {
            try
            {
                bool isAvailable = false;
                //find property details
                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if (property == null)
                {
                    return Ok(new { Code = 400, Message = "Property not found" });
                }
                string checkOutFormat = property.CheckOutFormat;
                string roomRatesCalcuatedBy = property.CalculateRoomRates;
                decimal discount = 0;
                string discountType = "";
                
                
                    var bookingDetails = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == roomEditDTO.BookingId);

                    if (bookingDetails == null)
                    {
                        return Ok(new { Code = 400, Message = "Booking not found" });
                    }
                    checkOutFormat = bookingDetails.CheckoutFormat ;
                    roomRatesCalcuatedBy = bookingDetails.CalculateRoomRates;
                    discountType = bookingDetails.DiscountType;
                    discount = bookingDetails.DiscountType == Constants.Constants.DeductionByPercentage ? bookingDetails.DiscountPercentage : bookingDetails.DiscountAmount;

                    DateOnly startDate;
                    DateOnly endDate;
                    string startTime;
                    string endTime;

                    //set start and end date time
                    if (roomEditDTO.ValueChanged == "reservationDate" || roomEditDTO.ValueChanged == "reservationTime" )
                    {

                        startDate = roomEditDTO.ReservationDate;
                        startTime = roomEditDTO.ReservationTime;
                        


                        //reservation date selected is before original reservation date
                        int minutesDiff = Constants.DateTimeMethod.GetDifferenceInMinutes(startDate,startTime, bookingDetails.ReservationDate, bookingDetails.ReservationTime);
                        if(minutesDiff > 0)
                        {
                            (endDate, endTime) = Constants.DateTimeMethod.GetAMinuteBefore(bookingDetails.ReservationDate, bookingDetails.ReservationTime);

                            var (code1, message1) = await CheckRoomAvailability(startDate, startTime, endDate, endTime, roomEditDTO.RoomTypeId, roomEditDTO.RoomId);
                            if(code1 == 200)
                            {
                                isAvailable = true;
                            }
                            else
                            {
                                return Ok(new { Code = code1, Message = message1 });
                            }
                        }
                        else
                        {
                            isAvailable = true;
                        } 
                    }
                    else if(roomEditDTO.ValueChanged == "checkInDate" || roomEditDTO.ValueChanged == "checkInTime")
                    {
                        startDate = roomEditDTO.CheckInDate;
                        startTime = roomEditDTO.CheckInTime;
                        int minutesDiff = Constants.DateTimeMethod.GetDifferenceInMinutes(startDate, startTime, bookingDetails.ReservationDate, bookingDetails.ReservationTime);
                        if (minutesDiff > 0)
                        {
                            (endDate, endTime) = Constants.DateTimeMethod.GetAMinuteBefore(bookingDetails.ReservationDate, bookingDetails.ReservationTime);

                            var (code1, message1) = await CheckRoomAvailability(startDate, startTime, endDate, endTime, roomEditDTO.RoomTypeId, roomEditDTO.RoomId);
                            if (code1 == 200)
                            {
                                isAvailable = true;
                            }
                            else
                            {
                                return Ok(new { Code = code1, Message = message1 });
                            }
                        }
                        else
                        {
                            isAvailable = true;
                        }

                        roomEditDTO.ReservationDate = startDate;
                        roomEditDTO.ReservationTime = startTime;

                    }
                    else if(roomEditDTO.ValueChanged == "checkOutDate" || roomEditDTO.ValueChanged == "checkOutTime")
                    {
                        endDate = roomEditDTO.CheckOutDate;
                        endTime = roomEditDTO.CheckOutTime;



                        //checkout date selected is after actual checkout date
                        int minutesDiff = Constants.DateTimeMethod.GetDifferenceInMinutes(bookingDetails.CheckOutDate, bookingDetails.CheckOutTime, endDate, endTime);
                        if (minutesDiff > 0)
                        {
                            if(bookingDetails.Status == Constants.Constants.CheckIn)
                        {
                            return Ok(new { Code = 400, Message = "Use ROOM EXTEND to extend Check Out." });
                        }
                            (startDate, startTime) = Constants.DateTimeMethod.GetAMinuteAfter(bookingDetails.CheckOutDate, bookingDetails.CheckOutTime);

                            var (code1, message1) = await CheckRoomAvailability(startDate, startTime, endDate, endTime, roomEditDTO.RoomTypeId, roomEditDTO.RoomId);
                            if (code1 == 200)
                            {
                                isAvailable = true;
                            }
                            else
                            {
                                return Ok(new { Code = code1, Message = message1 });
                            }
                        }
                        else
                        {
                            isAvailable = true;
                        }
                    }

                else if(roomEditDTO.ValueChanged == "roomTypeId")
                {
                    isAvailable = true;
                }


                if (isAvailable)
                {
                    roomEditDTO.NoOfHours = 0;
                    roomEditDTO.NoOfNights = 0;
                    //find no of nights and hours
                    if (checkOutFormat == Constants.Constants.SameDayFormat)
                    {
                        roomEditDTO.NoOfHours = Constants.Calculation.CalculateHour(roomEditDTO.ReservationDate, roomEditDTO.ReservationTime, roomEditDTO.CheckOutDate, roomEditDTO.CheckOutTime);
                        if (roomEditDTO.NoOfHours > 24)
                        {
                            return Ok(new { Code = 400, Message = "No of hours cannot exceed 24 hours" });
                        }
                    }

                    else
                    {
                        roomEditDTO.NoOfNights = Constants.Calculation.FindNightsAndHours(roomEditDTO.ReservationDate, roomEditDTO.ReservationTime, roomEditDTO.CheckOutDate, roomEditDTO.CheckOutTime, checkOutFormat);


                    }
                    //find rooms available between given dates
                    //get available rooms
                    DataSet dataSet = await GetRoomAvailability(roomEditDTO.CheckInDate, roomEditDTO.CheckInTime, roomEditDTO.CheckOutDate, roomEditDTO.CheckOutTime, "cleanroom", roomEditDTO.RoomTypeId);
                    if (dataSet == null)
                    {
                        return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                    }
                    var rows = new List<Dictionary<string, object>>();
                    var AvailableCount = new Dictionary<string, object>();
                    var dataTable = dataSet.Tables[0];
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            dict[col.ColumnName] = row[col];
                        }
                        rows.Add(dict);
                    }

                    var dataTable2 = dataSet.Tables[1];
                    foreach (DataRow row in dataTable2.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dataTable2.Columns)
                        {
                            dict[col.ColumnName] = row[col];
                        }
                        AvailableCount = dict;
                    }


                    //calcuate rates
                    var (code, message, response) = await CalculateRoomRateAsync(companyId, roomEditDTO.RoomTypeId, roomEditDTO.ReservationDate, roomEditDTO.CheckOutDate, 1, roomEditDTO.NoOfNights, roomEditDTO.GstType, roomEditDTO.NoOfHours, roomEditDTO.ReservationTime, roomEditDTO.CheckOutTime, discountType, discount, checkOutFormat, roomRatesCalcuatedBy, property);
                    return Ok(new { Code = code, Message = message, AvailableRooms = rows, AvailableCount = AvailableCount, Data = response, RoomDates = roomEditDTO });
                }


                return Ok(new { Code = 400, Message = Constants.Constants.ErrorMessage });





            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("CheckRoomAvailabilityOnAdd")]
        public async Task<IActionResult> CheckRoomAvailabilityOnAdd([FromBody] RoomEditDTO roomEditDTO)
        {
            try
            {
                
                //find property details
                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
                if (property == null)
                {
                    return Ok(new { Code = 400, Message = "Property not found" });
                }
                string checkOutFormat = property.CheckOutFormat;
                string roomRatesCalcuatedBy = property.CalculateRoomRates;
                decimal discount = 0;
                string discountType = "";


                roomEditDTO.NoOfHours = 0;
                roomEditDTO.NoOfNights = 0;
                //find no of nights and hours
                if (checkOutFormat == Constants.Constants.SameDayFormat)
                {
                    roomEditDTO.NoOfHours = Constants.Calculation.CalculateHour(roomEditDTO.ReservationDate, roomEditDTO.ReservationTime, roomEditDTO.CheckOutDate, roomEditDTO.CheckOutTime);
                    if (roomEditDTO.NoOfHours > 24)
                    {
                        return Ok(new { Code = 400, Message = "No of hours cannot exceed 24 hours" });
                    }
                }

                else
                {
                    roomEditDTO.NoOfNights = Constants.Calculation.FindNightsAndHours(roomEditDTO.ReservationDate, roomEditDTO.ReservationTime, roomEditDTO.CheckOutDate, roomEditDTO.CheckOutTime, checkOutFormat);


                }


                //get available rooms
                DataSet dataSet = await GetRoomAvailability(roomEditDTO.CheckInDate, roomEditDTO.CheckInTime, roomEditDTO.CheckOutDate, roomEditDTO.CheckOutTime, "cleanroom", roomEditDTO.RoomTypeId);
                if (dataSet == null)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }
                var rows = new List<Dictionary<string, object>>();
                var AvailableCount = new Dictionary<string, object>();
                var dataTable = dataSet.Tables[0];
                foreach (DataRow row in dataTable.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    rows.Add(dict);
                }

                var dataTable2 = dataSet.Tables[1];
                foreach (DataRow row in dataTable2.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dataTable2.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    AvailableCount = dict;
                }

                

                //if room id selected then check room is available or not
                if(roomEditDTO.RoomId > 0)
                {
                    var (code1, message1) = await CheckRoomAvailability(roomEditDTO.CheckInDate, roomEditDTO.CheckInTime, roomEditDTO.CheckOutDate, roomEditDTO.CheckOutTime, roomEditDTO.RoomTypeId, roomEditDTO.RoomId);
                    if (code1 != 200)
                    {
                        return Ok(new { Code = code1, Message = message1 });
                    }
                    
                }

                //if category and gst selected then calculate rates
                if (roomEditDTO.RoomTypeId > 0 && roomEditDTO.GstType != "")
                {
                    var (code, message, response) = await CalculateRoomRateAsync(companyId, roomEditDTO.RoomTypeId, roomEditDTO.ReservationDate, roomEditDTO.CheckOutDate, 1, roomEditDTO.NoOfNights, roomEditDTO.GstType, roomEditDTO.NoOfHours, roomEditDTO.ReservationTime, roomEditDTO.CheckOutTime, discountType, discount, checkOutFormat, roomRatesCalcuatedBy, property);
                    
                        return Ok(new { Code = code, Message = message, AvailableRooms = rows, AvailableCount = AvailableCount, RoomRates = response, RoomDates = roomEditDTO });
                    


                }
                else
                {
                    return Ok(new { Code = 200, Message = "Room fetched successfully", AvailableRooms = rows, AvailableCount = AvailableCount, RoomRates = new RoomRateResponse(), RoomDates = roomEditDTO});
                }



            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        //GET ROOM NO AVAILABLE OR NOT
        private async Task<(int Code, string Message)> CheckRoomAvailability(DateOnly startDate, string startTime, DateOnly endDate, string endTime, int roomTypeId, int roomId)
        {
           
            if (roomId == 0)
            {
                DataSet dataSet = await GetRoomAvailability(startDate, startTime, endDate, endTime, "cleanroom", roomTypeId, 0);
                if (dataSet == null)
                {
                    return (500, Constants.Constants.ErrorMessage);

                }
                else
                {
                    //Get available rooms on this category
                    int AvailableRooms = 0;
                    var dataTable2 = dataSet.Tables[1];
                    foreach (DataRow row in dataTable2.Rows)
                    {

                        foreach (DataColumn col in dataTable2.Columns)
                        {
                            AvailableRooms = Convert.ToInt32(row[col]);
                        }

                    }

                    if (AvailableRooms == 0)
                    {
                        return (400, "Rooms are not available on this date");

                    }
                    else
                    {
                        return (200, "Rooms are  available on this date");
                    }


                }
            }
            else
            {
                var response = await CheckRoomAvailable(startDate, startTime, endDate, endTime, roomTypeId, roomId);
                if(response == "success")
                {
                    return (200, "Room is  available on this date");
                }
                else
                {
                    return (400, response);
                }
                       

            }

        }



        [HttpGet("GetCheckInFormData")]
        public async Task<IActionResult> GetCheckInFormData()
        {
            try
            {
                

                var roomCategories = await _context.RoomCategoryMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).Select(x => new
                {
                    Id = x.Id,
                    Type = x.Type,
                    Description = x.Description,
                    MinPax = x.MinPax,
                    MaxPax = x.MaxPax,
                    DefaultPax = x.DefaultPax
                }).ToListAsync();

               

                var hours = await _context.HourMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync();

                var guestList = await _context.GuestDetails.Where(bm => bm.IsActive && bm.CompanyId == companyId).Select(x => new GuestDetails
                {
                    GuestId = x.GuestId,
                    GuestName = x.GuestName,
                    Nationality = x.Nationality,
                    StateName = x.StateName,
                    Address = x.Address,
                    City = x.City,
                    PhoneNumber = x.PhoneNumber,
                    Email = x.Email,
                    GuestImage = x.GuestImage,
                    Other1 = x.Other1,
                    Other2 = x.Other2,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate,
                    UserId = x.UserId,
                    CompanyId = x.CompanyId,
                    CityId = x.CityId,
                    StateId = x.StateId,
                    CountryId = x.CountryId,
                    Gender = x.Gender,
                    IdType = x.IdType,
                    IdNumber = x.IdNumber,
                    GuestNamePhone = x.GuestName + " : " + x.PhoneNumber
                }).ToListAsync();


                return Ok(new { Code = 200, Message = "Data get successfully", roomCategories = roomCategories,  hours = hours , guestList  = guestList });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetBookingById")]
        public async Task<IActionResult> GetBookingById(int bookingId)
        {
            try
            {
                var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.BookingId == bookingId && x.IsActive == true && x.CompanyId == companyId);
                if(booking != null)
                {
                    return Ok(new { Code = 200, Message = "Data fetched successfully", data = booking });
                }
                return Ok(new { Code = 400, Message = "Booking not found" });
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

      


        private MemoryStream GenerateInvoicePdf(CheckOutResponse invoiceData, string outputPath)
        {
           
            using(var ms = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                using (var document = new Document(pdf))
                {
                    
                    // Load font
                    PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    foreach (var room in invoiceData.BookingDetails)
                    {
                        // Company Header
                        var headerTable = new iText.Layout.Element.Table(new float[] { 9f, 1f })
                            .SetWidth(UnitValue.CreatePercentValue(100));

                        headerTable.AddCell(new Cell(1, 1)
                            .Add(new Paragraph(invoiceData.PropertyDetails.CompanyName).SetFont(font).SetFontSize(14))
                            .Add(new Paragraph(invoiceData.PropertyDetails.CompanyAddress).SetFontSize(10))
                            .Add(new Paragraph("Contact: " + invoiceData.PropertyDetails.ContactNo1).SetFontSize(10))
                            .SetBorder(Border.NO_BORDER));

                        headerTable.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER)); // Optional logo cell

                        document.Add(headerTable);
                        document.Add(new Paragraph("\n"));



                        // Guest and Invoice Info Table
                        var infoTable = new iText.Layout.Element.Table(new float[] { 1f, 1f })
                            .SetWidth(UnitValue.CreatePercentValue(100));

                        // Guest Details
                  
                        var guestTable = new iText.Layout.Element.Table(new float[] { 120, 150 })
                             .SetWidth(UnitValue.CreatePercentValue(100));


                        guestTable.AddCell(new Cell().Add(new Paragraph("Guest Name/Bill To:").SetFont(font).SetFontSize(10)).SetBorder(Border.NO_BORDER));
                        guestTable.AddCell(new Cell().Add(new Paragraph(string.IsNullOrWhiteSpace(invoiceData.InvoiceName) ? room.GuestDetails.GuestName : invoiceData.InvoiceName).SetFontSize(10)).SetBorder(Border.NO_BORDER));

                        guestTable.AddCell(new Cell().Add(new Paragraph("Phone No:").SetFont(font).SetFontSize(10)).SetBorder(Border.NO_BORDER));
                        guestTable.AddCell(new Cell().Add(new Paragraph(room.GuestDetails.PhoneNumber).SetFontSize(10)).SetBorder(Border.NO_BORDER));

                        guestTable.AddCell(new Cell().Add(new Paragraph("Room No:").SetFont(font).SetFontSize(10)).SetBorder(Border.NO_BORDER));
                        guestTable.AddCell(new Cell().Add(new Paragraph(room.RoomNo).SetFontSize(10)).SetBorder(Border.NO_BORDER));

                        if (invoiceData.PageName == "CANCELPAGE")
                        {
                            guestTable.AddCell(new Cell().Add(new Paragraph("Room Category:").SetFont(font).SetFontSize(10)).SetBorder(Border.NO_BORDER));
                            guestTable.AddCell(new Cell().Add(new Paragraph(room.RoomTypeName).SetFontSize(10)).SetBorder(Border.NO_BORDER));
                        }

                        

                        // Invoice Details
                        var invoiceTable = new iText.Layout.Element.Table(new float[] { 120, 80 })
                            .SetWidth(UnitValue.CreatePercentValue(100));

                        invoiceTable.AddCell(new Cell().Add(new Paragraph("Invoice No:").SetFont(font).SetFontSize(10)).SetBorder(Border.NO_BORDER));
                        invoiceTable.AddCell(new Cell().Add(new Paragraph(invoiceData.InvoiceNo).SetFontSize(10)).SetBorder(Border.NO_BORDER));

                        invoiceTable.AddCell(new Cell().Add(new Paragraph("Invoice Date:").SetFont(font).SetFontSize(10)).SetBorder(Border.NO_BORDER));
                        invoiceTable.AddCell(new Cell().Add(new Paragraph(invoiceData.InvoiceDate.ToString("dd/MM/yyyy")).SetFontSize(10)).SetBorder(Border.NO_BORDER));

                        invoiceTable.AddCell(new Cell().Add(new Paragraph("Pax:").SetFont(font).SetFontSize(10)).SetBorder(Border.NO_BORDER));
                        invoiceTable.AddCell(new Cell().Add(new Paragraph(room.Pax.ToString()).SetFontSize(10)).SetBorder(Border.NO_BORDER));


                        // === Wrapper Table with 2 columns, full width ===
                        iText.Layout.Element.Table wrapper = new iText.Layout.Element.Table(2);
                        wrapper.SetWidth(UnitValue.CreatePercentValue(100)); // Span entire page width

                        // Add spacing by setting padding/margin in cells
                        Cell wrapperCell1 = new Cell().Add(guestTable)
                                                      .SetBorder(Border.NO_BORDER)
                                                      .SetPaddingRight(100); // Space between tables

                        Cell wrapperCell2 = new Cell().Add(invoiceTable)
                                                      .SetBorder(Border.NO_BORDER);

                        wrapper.AddCell(wrapperCell1);
                        wrapper.AddCell(wrapperCell2);

                       

                        document.Add(wrapper);
                        document.Add(new Paragraph("\n"));

                        // Room Charges Table
                        if (invoiceData.PageName == "CheckOutPage")
                        {
                            document.Add(new Paragraph("Room Charges (Date-wise)").SetFont(font).SetFontSize(12));

                            var chargesTable = new iText.Layout.Element.Table(room.DiscountType == "Percentage" ? 11 : 10)
                         .SetWidth(UnitValue.CreatePercentValue(100));



                            // Header
                            // Header Row 1
                            // Main headers
                            chargesTable.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("Date").SetFont(font).SetFontSize(10)));
                            chargesTable.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("Category").SetFont(font).SetFontSize(10)));
                            chargesTable.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("Actual Rate").SetFont(font).SetFontSize(10)));

                            if (room.DiscountType == "Percentage")
                            {
                                chargesTable.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("Discount").SetFont(font).SetFontSize(10)));
                            }
                            else
                            {
                                chargesTable.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("Discount").SetFont(font).SetFontSize(10)));
                            }

                            chargesTable.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("Room Rate").SetFont(font).SetFontSize(10)));
                            chargesTable.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("CGST").SetFont(font).SetFontSize(10)));
                            chargesTable.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("SGST").SetFont(font).SetFontSize(10)));
                            chargesTable.AddHeaderCell(new Cell(2, 1).Add(new Paragraph("Total").SetFont(font).SetFontSize(10)));

                            // Header Row 2 (Only when DiscountType is Percentage)
                            if (room.DiscountType == "Percentage")
                            {
                                chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("%").SetFont(font).SetFontSize(10)));
                                chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("Amt").SetFont(font).SetFontSize(10)));
                            }

                            // GST breakdown cells (always added)
                            chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("%").SetFont(font).SetFontSize(10))); // CGST %
                            chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("Amt").SetFont(font).SetFontSize(10))); // CGST Amt
                            chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("%").SetFont(font).SetFontSize(10))); // SGST %
                            chargesTable.AddHeaderCell(new Cell().Add(new Paragraph("Amt").SetFont(font).SetFontSize(10))); // SGST Amt


                            foreach (var rate in room.BookedRoomRates)
                            {


                                chargesTable.AddCell(new Cell().Add(new Paragraph(rate.BookingDate.ToString("dd/MM/yyyy")).SetFontSize(10)));
                                chargesTable.AddCell(new Cell().Add(new Paragraph(room.RoomTypeName).SetFontSize(10)));
                                chargesTable.AddCell(new Cell().Add(new Paragraph(rate.RoomRateWithoutDiscount.ToString("F2")).SetFontSize(10)));

                                if (room.DiscountType == "Percentage")
                                {
                                    chargesTable.AddCell(new Cell().Add(new Paragraph(rate.DiscountPercentage.ToString("F2")).SetFontSize(10)));
                                    chargesTable.AddCell(new Cell().Add(new Paragraph(rate.DiscountAmount.ToString("F2")).SetFontSize(10)));
                                }
                                else
                                {
                                    chargesTable.AddCell(new Cell().Add(new Paragraph(rate.DiscountAmount.ToString("F2")).SetFontSize(10)));
                                }

                                chargesTable.AddCell(new Cell().Add(new Paragraph(rate.RoomRate.ToString("F2")).SetFontSize(10)));
                                chargesTable.AddCell(new Cell().Add(new Paragraph(rate.CGST.ToString("F2")).SetFontSize(10)));
                                chargesTable.AddCell(new Cell().Add(new Paragraph(rate.CGSTAmount.ToString("F2")).SetFontSize(10)));
                                chargesTable.AddCell(new Cell().Add(new Paragraph(rate.SGST.ToString("F2")).SetFontSize(10)));
                                chargesTable.AddCell(new Cell().Add(new Paragraph(rate.SGSTAmount.ToString("F2")).SetFontSize(10)));
                                chargesTable.AddCell(new Cell().Add(new Paragraph(rate.TotalRoomRate.ToString("F2")).SetFontSize(10)));
                            }

                            document.Add(chargesTable);
                            document.Add(new Paragraph("\n"));
                        }

                        // SERVICES TABLE
                        if (invoiceData.PageName == "CheckOutPage" && room.AdvanceServices.Any())
                        {
                            document.Add(new Paragraph("Room Services").SetFont(font).SetFontSize(12));

                            var serviceTable = new iText.Layout.Element.Table(9)
                        .SetWidth(UnitValue.CreatePercentValue(100));




                            string[] serviceHeaders = { "Date", "Service Name", "Price", "CGST", "SGST", "Qty", "Total" };

                            foreach (var header in serviceHeaders)
                            {
                                if (header == "CGST" || header == "SGST")
                                {
                                    // These headers span 2 columns in 1 row
                                    serviceTable.AddHeaderCell(new Cell(1, 2)
                                        .Add(new Paragraph(header).SetFont(font).SetFontSize(10)));
                                }
                                else
                                {
                                    // All other headers span 2 rows
                                    serviceTable.AddHeaderCell(new Cell(2, 1)
                                        .Add(new Paragraph(header).SetFont(font).SetFontSize(10)));
                                }
                            }

                            serviceTable.AddHeaderCell(new Cell().Add(new Paragraph("%").SetFont(font).SetFontSize(10)));
                            serviceTable.AddHeaderCell(new Cell().Add(new Paragraph("Amt").SetFont(font).SetFontSize(10)));
                            serviceTable.AddHeaderCell(new Cell().Add(new Paragraph("%").SetFont(font).SetFontSize(10)));
                            serviceTable.AddHeaderCell(new Cell().Add(new Paragraph("Amt").SetFont(font).SetFontSize(10)));
                            foreach (var service in room.AdvanceServices)
                            {
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.ServiceDate.ToString("dd/MM/yyyy")).SetFontSize(10)));
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.ServiceName).SetFontSize(10)));
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.ServicePrice.ToString("F2")).SetFontSize(10)));
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.CGSTPercentage.ToString("F2")).SetFontSize(10)));
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.CgstAmount.ToString("F2")).SetFontSize(10)));
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.SGSTPercentage.ToString("F2")).SetFontSize(10)));
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.SgstAmount.ToString("F2")).SetFontSize(10)));
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.Quantity.ToString()).SetFontSize(10)));
                                serviceTable.AddCell(new Cell().Add(new Paragraph(service.TotalAmount.ToString("F2")).SetFontSize(10)));
                            }

                            document.Add(serviceTable);
                            document.Add(new Paragraph("\n"));
                        }

                        // CHECKOUT SUMMARY
                        if (invoiceData.PageName == "CHECKOUTPAGE")
                        {
                            var summaryTable = new iText.Layout.Element.Table(2).SetWidth(300).SetHorizontalAlignment(HorizontalAlignment.RIGHT);

                            summaryTable.AddCell(CreateLabelCell("Booking Amount:"));
                            summaryTable.AddCell(CreateValueCell(room.TotalBookingAmount.ToString("F2")));

                            summaryTable.AddCell(CreateLabelCell("Early/Late CheckIn Charges:"));
                            summaryTable.AddCell(CreateValueCell((room.EarlyCheckInCharges + room.LateCheckOutCharges).ToString("F2")));

                            summaryTable.AddCell(CreateLabelCell("Service Amount:"));
                            summaryTable.AddCell(CreateValueCell(room.TotalServicesAmount.ToString("F2")));

                            summaryTable.AddCell(CreateLabelCell("Total Amount:"));
                            summaryTable.AddCell(CreateValueCell(room.TotalAmountWithOutDiscount.ToString("F2")));

                            summaryTable.AddCell(CreateLabelCell("Discount Amount:"));
                            summaryTable.AddCell(CreateValueCell(room.CheckOutDiscoutAmount.ToString("F2")));

                            summaryTable.AddCell(CreateLabelCell("Total Bill:"));
                            summaryTable.AddCell(CreateValueCell(room.TotalAmount.ToString("F2")));

                            summaryTable.AddCell(CreateLabelCell("Total Paid:"));
                            summaryTable.AddCell(CreateValueCell((room.AdvanceAmount + room.ReceivedAmount + room.RefundAmount + room.ResidualAmount).ToString("F2")));

                            summaryTable.AddCell(CreateLabelCell("Balance Amount:"));
                            summaryTable.AddCell(CreateValueCell(room.BalanceAmount.ToString("F2")));

                            if (room.RefundAmount > 0)
                            {
                                summaryTable.AddCell(CreateLabelCell("Refund Amount:"));
                                summaryTable.AddCell(CreateValueCell(room.RefundAmount.ToString("F2")));
                            }

                            if (room.ResidualAmount > 0)
                            {
                                summaryTable.AddCell(CreateLabelCell("Residual Amount:"));
                                summaryTable.AddCell(CreateValueCell(room.ResidualAmount.ToString("F2")));
                            }

                            document.Add(summaryTable);
                        }

                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE)); // Next room on new page
                    }
                   

                }
                return ms;
            }
           
            

            
        }

        private Cell CreateLabelCell(string text)
        {
            return new Cell().Add(new Paragraph(text).SetFontSize(10).SimulateBold()).SetBorder(Border.NO_BORDER);
        }

        private Cell CreateValueCell(string text, DeviceRgb color = null)
        {
            var paragraph = new Paragraph(text).SetFontSize(10);
            if (color != null)
                paragraph.SetFontColor(color);
            return new Cell().Add(paragraph).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER);
        }
        private bool IsTodayCheckOutDate(DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        private void CalculateCancelInvoice(List<BookingDetail> bookings, List<PaymentDetails> payments)
        {
            //set room payment if room wise payment
            foreach (var booking in bookings)
            {
                foreach (var pay in payments)
                {
                    if (pay.RoomId == booking.RoomId && pay.BookingId == booking.BookingId)
                    {
                        pay.IsReceived = true;
                        pay.PaymentLeft = 0;
                        decimal balance = booking.CancelAmount;
                        //amount left in room
                        if (balance > 0)
                        {
                            decimal currentBalance = balance >= pay.PaymentAmount ? pay.PaymentAmount : balance;
                            if (balance >= pay.PaymentAmount)
                            {
                                booking.ReceivedAmount = booking.ReceivedAmount + pay.PaymentAmount;
                                pay.RefundAmount = 0;
                            }
                            else
                            {
                                booking.ReceivedAmount = booking.ReceivedAmount + balance;
                                pay.RefundAmount = pay.PaymentAmount - balance;
                                pay.PaymentLeft = 0;
                                booking.RefundAmount = pay.RefundAmount;

                            }
                            pay.InvoiceHistories.Add(CreateInvoiceHistory(booking, pay, currentBalance));
                        }
                        else
                        {
                            pay.RefundAmount = pay.PaymentAmount;
                            booking.RefundAmount = pay.RefundAmount;
                            pay.InvoiceHistories.Add(CreateInvoiceHistory(booking, pay, 0));
                        }

                    }
                }
            }

            //calculate advance
            decimal agentAdvance = 0;
            decimal advanceAmount = 0;
            decimal receivedAmount = 0;
            (agentAdvance, advanceAmount, receivedAmount) = CalculatePayment(payments, bookings);

            //agent advance allocation
            if (agentAdvance > 0)
            {
                int roomCounts = GetBalanceCancelCount(bookings);
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(agentAdvance, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateBalanceCancelAmount(bookings[i]);
                    if (balance > 0)
                    {
                        decimal currentBalance = 0;
                        if (balance >= equallyDivideArr[divideArrIndex])
                        {
                            currentBalance = equallyDivideArr[divideArrIndex];
                        }
                        else
                        {
                            currentBalance = balance;
                            if (divideArrIndex < equallyDivideArr.Count - 1)
                            {
                                equallyDivideArr[divideArrIndex + 1] += (equallyDivideArr[divideArrIndex] - balance);

                            }
                        }

                        bookings[i].AgentAdvanceAmount += currentBalance;

                        //assign payment
                        while (paymentIndex != payments.Count && currentBalance != 0)
                        {
                            if (!IsAgentPaymentLeft(payments, paymentIndex))
                            {
                                paymentIndex++;
                            }
                            else
                            {
                                //payment     
                                if (payments[paymentIndex].PaymentLeft >= currentBalance)
                                {
                                    payments[paymentIndex].PaymentLeft -= currentBalance;
                                    bookings[i].AgentAdvanceAmount += currentBalance;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], currentBalance));
                                    currentBalance = 0;
                                }
                                else
                                {
                                    bookings[i].AgentAdvanceAmount += payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].PaymentLeft = 0;
                                    payments[paymentIndex].IsReceived = true;
                                    currentBalance = currentBalance - payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], payments[paymentIndex].PaymentLeft));
                                }
                                paymentIndex++;
                            }

                        }
                        divideArrIndex++;
                    }


                }

            }

            //advance allocation
            if (advanceAmount > 0)
            {
                int roomCounts = GetBalanceCancelCount(bookings);
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(advanceAmount, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateBalanceCancelAmount(bookings[i]);
                    if (balance > 0)
                    {
                        decimal currentBalance = 0;
                        if (balance >= equallyDivideArr[divideArrIndex])
                        {
                            currentBalance = equallyDivideArr[divideArrIndex];
                        }
                        else
                        {
                            currentBalance = balance;
                            if (divideArrIndex < equallyDivideArr.Count - 1)
                            {
                                equallyDivideArr[divideArrIndex + 1] += (equallyDivideArr[divideArrIndex] - balance);

                            }
                        }



                        //assign payment
                        while (paymentIndex != payments.Count && currentBalance != 0)
                        {
                            if (!IsAdvancePaymentLeft(payments, paymentIndex))
                            {
                                paymentIndex++;
                            }
                            else
                            {
                                //payment

                                if (payments[paymentIndex].PaymentLeft >= currentBalance)
                                {
                                    payments[paymentIndex].PaymentLeft -= currentBalance;
                                    bookings[i].AdvanceAmount += currentBalance;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], currentBalance));
                                    currentBalance = 0;
                                }
                                else
                                {
                                    bookings[i].AdvanceAmount += payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].PaymentLeft = 0;
                                    payments[paymentIndex].IsReceived = true;
                                    currentBalance = currentBalance - payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], payments[paymentIndex].PaymentLeft));
                                }

                                paymentIndex++;
                            }
                        }
                        divideArrIndex++;
                    }


                }

            }

            //receive amount
            if (receivedAmount > 0)
            {

                int roomCounts = GetBalanceCancelCount(bookings);
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(receivedAmount, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateBalanceCancelAmount(bookings[i]);
                    if (balance > 0)
                    {
                        decimal currentBalance = 0;
                        if (balance >= equallyDivideArr[divideArrIndex])
                        {
                            currentBalance = equallyDivideArr[divideArrIndex];
                        }
                        else
                        {
                            currentBalance = balance;
                            if (divideArrIndex < equallyDivideArr.Count - 1)
                            {
                                equallyDivideArr[divideArrIndex + 1] += (equallyDivideArr[divideArrIndex] - balance);

                            }
                        }



                        //assign payment
                        while (paymentIndex != payments.Count && currentBalance != 0)
                        {
                            if (!IsReceivedPaymentLeft(payments, paymentIndex))
                            {
                                paymentIndex++;
                            }
                            else
                            {
                                //payment     
                                if (payments[paymentIndex].PaymentLeft >= currentBalance)
                                {
                                    payments[paymentIndex].PaymentLeft -= currentBalance;
                                    bookings[i].ReceivedAmount += currentBalance;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], currentBalance));
                                    currentBalance = 0;
                                }
                                else
                                {
                                    bookings[i].ReceivedAmount += payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].PaymentLeft = 0;
                                    payments[paymentIndex].IsReceived = true;
                                    currentBalance = currentBalance - payments[paymentIndex].PaymentLeft;
                                    payments[paymentIndex].InvoiceHistories.Add(CreateInvoiceHistory(bookings[i], payments[paymentIndex], payments[paymentIndex].PaymentLeft));
                                }
                                paymentIndex++;
                            }
                        }
                        divideArrIndex++;
                    }


                }

            }


        }

        private async Task<(int Code, string Message, RoomRateResponse? Response)> CalculateRoomRateAsync(
            int companyId, int roomTypeId, DateOnly checkInDate, DateOnly checkOutDate,
            string checkOutFormat, int noOfRooms, int noOfNights, string gstType, int hourId)
        {
            var roomRateResponse = new RoomRateResponse();

            if (roomTypeId == 0)
            {
                return (400, "Invalid data", roomRateResponse);
                //return Ok(new { Code = 400, Message = "Invalid data" });
            }

            //find property details
            var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.PropertyId == companyId);
            if (property == null)
            {
                return (400, "Property not found", roomRateResponse);
            }

            var gstPercentage = await GetGstPercetage(Constants.Constants.Reservation);
            if (gstPercentage == null)
            {
                return (400, "Gst percentage not found for reservation", roomRateResponse);

            }
            //if checkout format is sameday
            if (property.CheckOutFormat == Constants.Constants.SameDayFormat)
            {
                var roomRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId && x.HourId == hourId).FirstOrDefaultAsync();
                if (roomRates == null)
                {
                    return (400, "No Room Rates found", roomRateResponse);

                }
                else
                {
                    roomRateResponse.BookingAmount = Calculation.RoundOffDecimal(roomRates.RoomRate);

                    var roomRateDate = new BookedRoomRate();
                    roomRateDate.BookingDate = checkInDate;
                    roomRateDate.GstType = gstType;
                    if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                    {
                        var gstRangeMaster = GetApplicableGstRange(gstPercentage.ranges, roomRateResponse.BookingAmount);
                        if (gstRangeMaster == null)
                        {
                            roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                        }
                        else
                        {
                            roomRateDate.GstPercentage = gstRangeMaster.TaxPercentage;
                        }
                    }
                    else
                    {
                        roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                    }
                    (roomRateDate.RoomRate, roomRateDate.GstAmount) = Calculation.CalculateGst(roomRateResponse.BookingAmount, roomRateDate.GstPercentage, gstType);
                    roomRateResponse.BookingAmount = roomRateDate.RoomRate;

                    roomRateDate.CGST = Constants.Calculation.CalculateCGST(roomRateDate.GstPercentage);
                    roomRateDate.CGSTAmount = Constants.Calculation.CalculateCGST(roomRateDate.GstAmount);
                    roomRateDate.SGST = roomRateDate.CGST;
                    roomRateDate.SGSTAmount = roomRateDate.CGSTAmount;
                    roomRateResponse.BookedRoomRates.Add(roomRateDate);
                }
            }

            //checkout format - 24 hours/ night
            else
            {

                DateOnly currentDate = checkInDate;
                while (noOfNights > 0)
                {
                    var roomRateDate = new BookedRoomRate();
                    roomRateDate.BookingDate = currentDate;
                    roomRateDate.GstType = gstType;

                    var customRoomRates = await _context.RoomRateDateWise.Where(x => x.IsActive == true && x.CompanyId == companyId && (x.FromDate <= currentDate && x.ToDate >= currentDate) && x.RoomTypeId == roomTypeId).OrderByDescending(x => x.RatePriority).FirstOrDefaultAsync();
                    if (customRoomRates == null)
                    {
                        //fetch standard room rates
                        var roomRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId).FirstOrDefaultAsync();
                        if (roomRates == null)
                        {

                            return (400, "No Room Rates found", roomRateResponse);

                        }
                        else
                        {
                            if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                            {
                                var gstRangeMaster = GetApplicableGstRange(gstPercentage.ranges, (Calculation.RoundOffDecimal(roomRates.RoomRate)));
                                if (gstRangeMaster == null)
                                {
                                    roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                                }
                                else
                                {
                                    roomRateDate.GstPercentage = gstRangeMaster.TaxPercentage;
                                }
                            }
                            else
                            {
                                roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                            }
                            (roomRateDate.RoomRate, roomRateDate.GstAmount) = Calculation.CalculateGst(Calculation.RoundOffDecimal(roomRates.RoomRate), roomRateDate.GstPercentage, gstType);

                        }
                    }
                    else
                    {
                        if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                        {
                            var gstRangeMaster = GetApplicableGstRange(gstPercentage.ranges, (Calculation.RoundOffDecimal(customRoomRates.RoomRate)));
                            if (gstRangeMaster == null)
                            {
                                roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                            }
                            else
                            {
                                roomRateDate.GstPercentage = gstRangeMaster.TaxPercentage;
                            }
                        }
                        else
                        {
                            roomRateDate.GstPercentage = gstPercentage.TaxPercentage;
                        }
                        (roomRateDate.RoomRate, roomRateDate.GstAmount) = Calculation.CalculateGst(Calculation.RoundOffDecimal(customRoomRates.RoomRate), roomRateDate.GstPercentage, gstType);

                    }

                    roomRateDate.TotalRoomRate = Calculation.RoundOffDecimal(roomRateDate.RoomRate + roomRateDate.GstAmount);
                    roomRateDate.CGST = Constants.Calculation.CalculateCGST(roomRateDate.GstPercentage);
                    roomRateDate.CGSTAmount = Constants.Calculation.CalculateCGST(roomRateDate.GstAmount);
                    roomRateDate.SGST = roomRateDate.CGST;
                    roomRateDate.SGSTAmount = roomRateDate.CGSTAmount;
                    roomRateResponse.BookedRoomRates.Add(roomRateDate);

                    roomRateResponse.BookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRateDate.RoomRate);

                    roomRateResponse.GstAmount = Calculation.RoundOffDecimal(roomRateResponse.GstAmount + roomRateDate.GstAmount);


                    noOfNights--;
                    currentDate = currentDate.AddDays(1);
                }


                //check earlycheckin 
                if (property.IsEarlyCheckInPolicyEnable)
                {
                    var extraPolicy = await _context.ExtraPolicies.Where(x => x.IsActive == true && x.Status == Constants.Constants.EARLYCHECKIN).ToListAsync();
                    if (extraPolicy.Count == 0)
                    {
                        return (400, "Early checkin polic not found", roomRateResponse);
                    }


                }

            }

            roomRateResponse.TotalBookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRateResponse.GstAmount);

            //total amount
            roomRateResponse.AllRoomsAmount = Calculation.RoundOffDecimal(noOfRooms * roomRateResponse.BookingAmount);
            roomRateResponse.AllRoomsGst = Calculation.RoundOffDecimal(noOfRooms * roomRateResponse.GstAmount);
            roomRateResponse.TotalRoomsAmount = Calculation.RoundOffDecimal(roomRateResponse.AllRoomsAmount + roomRateResponse.AllRoomsGst);

            return (200, "Room rate fetched successfully", roomRateResponse);


        }
    }
}
