using AutoMapper;
using Azure;
using Azure.Core;
using hotel_api.Constants;
using hotel_api.GeneralMethods;
using hotel_api.Notifications;
using hotel_api.Notifications.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Repository.DTO;
using Repository.Models;
using Repository.RequestDTO;
using RepositoryModels.Repository;
using System.ComponentModel.Design;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        

        [HttpGet("CheckRoomAvailaibility")]
        public async Task<IActionResult> CheckRoomAvailaibility(DateTime checkInDate, string checkInTime, DateTime checkOutDate, string checkOutTime, string pageName = "", int roomTypeId = 0)
        {
            try
            {
               
                if (checkInDate == null || checkOutDate == null || checkInDate == DateTime.MinValue || checkOutDate == DateTime.MinValue || checkInDate == Constants.Constants.DefaultDate || checkOutDate == Constants.Constants.DefaultDate)
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

                var getbookingno = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Type == Constants.Constants.DocumentReservation && x.FinancialYear == financialYear);
                if (getbookingno == null || getbookingno.Suffix == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found.", data = getbookingno });
                }
                var bookingno = getbookingno.Prefix + getbookingno.Separator + getbookingno.Prefix1 + getbookingno.Separator + getbookingno.Prefix2 + getbookingno.Suffix + getbookingno.Number + getbookingno.LastNumber;

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

                return Ok(new { Code = 200, Message = "Data get successfully", bookingno = bookingno, agentDetails = agentDetails, roomCategories = roomCategories, paymentModes = paymentModes });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("ReservationRoomRate")]
        public async Task<IActionResult> ReservationRoomRate(int roomTypeId, DateTime checkInDate, DateTime checkOutDate, string checkOutFormat, int noOfRooms, int noOfNights, string gstType, int hourId = 0)
        {
            try
            {
                var roomRateResponse = new RoomRateResponse();             
                var (code, message, response) = await CalculateRoomRateAsync(companyId, roomTypeId, checkInDate, checkOutDate, checkOutFormat, noOfRooms, noOfNights, gstType, hourId);
                return Ok(new { Code = code, Message = message, Data = response });

            }
            catch (Exception)
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

                //save reservation details
                var reservationDetails = _mapper.Map<ReservationDetails>(request.ReservationDetailsDTO);
                reservationDetails.PrimaryGuestId = guest.GuestId;
                Constants.Constants.SetMastersDefault(reservationDetails, companyId, userId, currentDate);

                (reservationDetails.TotalRoomPayment, reservationDetails.TotalGst, reservationDetails.TotalAmount) = BookingCalulation.ReservationRoomsTotal(request.BookingDetailsDTO);

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

                await _context.ReservationDetails.AddAsync(reservationDetails);
                

                //save booking
                int roomCount = 1;
                List<BookingDetail> bookings = new List<BookingDetail>();
                foreach (var item in request.BookingDetailsDTO)
                {
                    foreach (var room in item.AssignedRooms)
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
                        bookingDetails.Status = request.ReservationDetailsDTO.IsCheckIn == true ? Constants.Constants.CheckIn : bookingDetails.Status;
                        bookingDetails.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                        bookingDetails.BookingDate = currentDate;
                        bookingDetails.PrimaryGuestId = guest.GuestId;
                        bookingDetails.GuestId = request.ReservationDetailsDTO.IsCheckIn == true ? guest.GuestId : 0;
                        bookingDetails.RoomId = room.RoomId;
                        bookingDetails.RoomCount = request.BookingDetailsDTO.Count == 1 && item.AssignedRooms.Count == 1 ? 0 : roomCount;
                        bookingDetails.BookingSource = request.ReservationDetailsDTO.BookingSource;
                        bookingDetails.TotalAmount = BookingCalulation.BookingTotalAmount(bookingDetails);
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
                    paymentDetails.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                    paymentDetails.PaymentLeft = request.PaymentDetailsDTO.PaymentAmount;
                    await _context.PaymentDetails.AddAsync(paymentDetails);
                    
                }

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
                if (property.IsEmailNotification)
                {
                    ReservationEmailNotification emailNotification = new ReservationEmailNotification(_context, property, request.ReservationDetailsDTO.ReservationNo, roomCount - 1, guest, companyId);
                    await emailNotification.SendEmail();
                }

                //if (property.IsWhatsappNotification)
                //{
                //    ReservationWhatsAppNotification whatsAppNotification = new ReservationWhatsAppNotification(_context, property, guest,companyId, bookings);
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
                            .Where(x => x.BookingId == bookings.BookingId)
                            .ToListAsync();
                        if (bookedRoomRate == null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 404, Message = "Data Not Found" });
                        }
                        _context.BookedRoomRates.RemoveRange(bookedRoomRate);

                        var paymentDetails = await _context.PaymentDetails
                            .Where(x => x.BookingId == bookings.BookingId)
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
                    var reservation = await _context.ReservationDetails.FindAsync(id);
                    if (reservation == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 404, Message = "Data Not Found" });
                    }

                   

                    var booking = await _context.BookingDetail
                        .Where(x => x.ReservationNo == reservation.ReservationNo)
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

                            var roomAvailability = await _context.RoomAvailability.FindAsync(item.BookingId);
                            if (roomAvailability == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 404, Message = "Data Not Found" });
                            }

                            _context.RoomAvailability.Remove(roomAvailability);

                            var bookedRoomRate = await _context.BookedRoomRates
                                .Where(x => x.BookingId == item.BookingId)
                                .ToListAsync();
                            if (bookedRoomRate == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 404, Message = "Data Not Found" });
                            }
                            _context.BookedRoomRates.RemoveRange(bookedRoomRate);

                            var paymentDetails = await _context.PaymentDetails
                                .Where(x => x.BookingId == item.BookingId)
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

                if (pageName == "cancelBooking")
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


                checkInResponse.BookingDetailCheckInDTO = await (
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
                                        select new BookingDetailCheckInDTO
                                        {
                                            BookingId = booking.BookingId,
                                            GuestId = booking.GuestId,
                                            RoomId = booking.RoomId,
                                            RoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
                                            OriginalRoomId = booking.RoomId,
                                            OriginalRoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
                                            RoomTypeId = booking.RoomTypeId,
                                            RoomCategoryName = category.Type,
                                            OriginalRoomTypeId = booking.RoomTypeId,
                                            OriginalRoomCategoryName = category.Type,
                                            CheckInDate = booking.CheckInDate.ToString("yyyy-MM-dd"),
                                            CheckInTime = booking.CheckInTime,
                                            CheckOutDate = booking.CheckOutDate.ToString("yyyy-MM-dd"),
                                            CheckOutTime = booking.CheckOutTime,
                                            CheckInDateTime = booking.CheckInDateTime,
                                            CheckOutDateTime = booking.CheckOutDateTime,
                                            NoOfNights = booking.NoOfNights,
                                            NoOfHours = booking.NoOfHours,
                                            HourId = booking.HourId,
                                            Status = booking.Status,
                                            Remarks = booking.Remarks,
                                            ReservationNo = booking.ReservationNo,
                                            UserId = booking.UserId,
                                            CompanyId = booking.CompanyId,
                                            BookingAmount = booking.BookingAmount,
                                            GstType = booking.GstType,
                                            GstAmount = booking.GstAmount,
                                            TotalBookingAmount = booking.TotalBookingAmount,
                                            TotalAmount = booking.TotalAmount,
                                            BookingSource = booking.BookingSource,
                                            ReservationDate = booking.ReservationDate.ToString("yyyy-MM-dd"),
                                            ReservationTime = booking.ReservationTime,
                                            ReservationDateTime = booking.ReservationDateTime,
                                            Pax = booking.Pax,
                                            OriginalPax = booking.Pax,
                                            IsSameGuest = booking.PrimaryGuestId == booking.GuestId ? true : false,
                                            OriginalReservationDateTime = booking.ReservationDateTime,
                                            OriginalReservationDate = booking.ReservationDate.ToString("yyyy-MM-dd"),
                                            OriginalReservationTime = booking.ReservationTime,
                                            OriginalCheckInDate = booking.CheckInDate.ToString("yyyy-MM-dd"),
                                            OriginalCheckInTime = booking.CheckInTime,
                                            OriginalCheckOutDate = booking.CheckOutDate.ToString("yyyy-MM-dd"),
                                            OriginalCheckOutTime = booking.CheckOutTime,
                                            CheckOutFormat = booking.CheckoutFormat,
                                            IsCheckIn = false
                                        } // project the entity to map later
                                    ).ToListAsync();


                foreach (var item in checkInResponse.BookingDetailCheckInDTO)
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
                                                            RoomNo = rm != null ? rm.RoomNo : ""
                                                        }).ToListAsync();



                //payment summary
                var paymentSummary = CalculateCheckInSummary(checkInResponse.ReservationDetails, checkInResponse.BookingDetailCheckInDTO, checkInResponse.PaymentDetails);
               

                checkInResponse.PaymentSummary = paymentSummary;



                return Ok(new { Code = 200, Message = "Data fetched successfully", data = checkInResponse });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("UpdateRoomDetail")]
        public async Task<IActionResult> UpdateRoomDetail([FromBody] List<BookingDetailCheckInDTO> bookingList, string reservationNo)
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
                        bookingDetails.TotalAmount = BookingCalulation.BookingTotalAmount(bookingDetails);
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

                    }
                    else
                    {
                        var bookingDetails = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId);
                        if (bookingDetails == null)
                        {
                            return Ok(new { Code = 400, Message = "Booking not found" });
                        }

                        _mapper.Map(item, bookingDetails);
                        bookingDetails.CheckInDateTime = DateTime.ParseExact((bookingDetails.CheckInDate.ToString("yyyy-MM-dd")) + " " + bookingDetails.CheckInTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        bookingDetails.CheckOutDateTime = DateTime.ParseExact((bookingDetails.CheckOutDate.ToString("yyyy-MM-dd")) + " " + bookingDetails.CheckOutTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        bookingDetails.ReservationDateTime = DateTime.ParseExact((bookingDetails.ReservationDate.ToString("yyyy-MM-dd")) + " " + bookingDetails.ReservationTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        bookingDetails.InitialCheckOutDate = bookingDetails.CheckOutDate;
                        bookingDetails.InitialCheckOutTime = bookingDetails.CheckOutTime;
                        bookingDetails.InitialCheckOutDateTime = bookingDetails.CheckOutDateTime;
                        bookingDetails.UpdatedDate = currentDate;
                        bookingDetails.GuestId = item.GuestDetails.GuestId;
                        bookingDetails.TotalAmount = BookingCalulation.BookingTotalAmount(bookingDetails);
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



                List<BookingDetailDTO> bookings = details
                                                    .Select(x => _mapper.Map<BookingDetailDTO>(x))
                                                    .ToList();

                (reservationDetails.TotalRoomPayment, reservationDetails.TotalGst, reservationDetails.TotalAmount) = BookingCalulation.ReservationRoomsTotal(bookings);

                if(reservationDetails.AgentId > 0)
                {
                    var (code, message, agentCommisionResponse) = await CalculateAgentCommisionAsync(reservationDetails.AgentId, reservationDetails.TotalRoomPayment, reservationDetails.TotalAmount);
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
            try
            {
                
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
                            payment.PaymentLeft = paymentDetails.PaymentAmount;
                            payment.UpdatedDate = currentDate;
                        }


                        _context.PaymentDetails.Update(payment);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {

                    paymentDetails.PaymentLeft = paymentDetails.PaymentAmount;
                    Constants.Constants.SetMastersDefault(paymentDetails, companyId, userId, currentDate);
                    await _context.PaymentDetails.AddAsync(paymentDetails);
                    await _context.SaveChangesAsync();
                }
                return Ok(new { Code = 200, Message = "Payment Updated successfully" });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("UpdateRoomsCheckIn")]
        public async Task<IActionResult> UpdateRoomsCheckIn([FromBody] List<int> rooms)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            { 
                var currentDate = DateTime.Now;
                List<CheckInNotificationDTO> notificationDTOs = new List<CheckInNotificationDTO>();
                List<BookingDetail> bookings = new List<BookingDetail>();
                if (rooms.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "No rooms found for checkin" });
                }

                foreach (var item in rooms)
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


                        booking.Status = Constants.Constants.CheckIn;
                        booking.UpdatedDate = currentDate;
                        _context.BookingDetail.Update(booking);


                        booking.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == booking.GuestId && x.IsActive == true && x.CompanyId == companyId);
                        bookings.Add(booking);
                        

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


                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Rooms Check-In successfully" });
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

               
                //room shift
                if (request.Type == "Shift")
                {
                    var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == request.BookingId);
                    if (booking == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Booking not found" });
                    }

                    //check new room available or not
                    string isRoomAvailable = await CheckRoomAvailable(request.ShiftDate, Constants.Constants.DayStartTime, booking.CheckOutDate, booking.CheckOutTime, request.ShiftRoomTypeId, request.ShiftRoomId);

                    if (isRoomAvailable != "success")
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = isRoomAvailable });
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
                        HourId = booking.HourId,
                        RoomCount = booking.RoomCount,
                        Pax = booking.Pax,
                        Status = booking.Status,
                        Remarks = booking.Remarks,
                        ReservationNo = booking.ReservationNo,
                        BookingDate = booking.BookingDate,
                        CreatedDate = currentDate, // new booking timestamp
                        UpdatedDate = currentDate,
                        IsActive = true,
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
                        AgentAdvanceAmount = booking.AgentAdvanceAmount,
                        InvoiceName = booking.InvoiceName,
                        BillTo = booking.BillTo
                    };

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


                    //calculate no of nights
                    int nights = Constants.Calculation.CalculateNights(request.ShiftDate, booking.CheckOutDate);

                    //calculate room rates
                    var (code, message, response) = await CalculateRoomRateAsync(companyId, request.ShiftRoomTypeId, request.ShiftDate, booking.CheckOutDate, booking.CheckoutFormat, 1, nights, booking.GstType, 0);
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
                    var newbookedRoomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == newBooking.BookingId).OrderBy(x => x.BookingDate).ToListAsync();

                    (newBooking.BookingAmount, newBooking.GstAmount, newBooking.TotalBookingAmount) = CalculateTotalBookedRoomRate(newbookedRoomRates);

                    newBooking.TotalAmount = BookingCalulation.BookingTotalAmount(newBooking);

                    _context.BookingDetail.Update(newBooking);


                    //roomavailability
                    var roomAvailaibility = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId);
                    if (roomAvailaibility == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "No room availability found" });
                    }

                    roomAvailaibility.CheckOutDate = Constants.Calculation.GetADayBefore(request.ShiftDate);
                    roomAvailaibility.CheckOutTime = Constants.Constants.DayEndTime;
                    roomAvailaibility.CheckOutDateTime = Constants.Calculation.ConvertToDateTime(roomAvailaibility.CheckOutDate, roomAvailaibility.CheckOutTime);
                    roomAvailaibility.RoomStatus = Constants.Constants.Dirty;
                    roomAvailaibility.UpdatedDate = currentDate;
                    _context.RoomAvailability.Update(roomAvailaibility);


                    //create new room availability
                    var newRoomAvailability = new RoomAvailability();
                    newRoomAvailability.CheckInDate = request.ShiftDate;
                    newRoomAvailability.CheckInTime = Constants.Constants.DayStartTime;
                    newRoomAvailability.CheckInDateTime = Constants.Calculation.ConvertToDateTime(newRoomAvailability.CheckInDate, newRoomAvailability.CheckInTime);
                    newRoomAvailability.CheckOutDate = booking.CheckOutDate;
                    newRoomAvailability.CheckOutTime = booking.CheckOutTime;
                    newRoomAvailability.CheckOutDateTime = Constants.Calculation.ConvertToDateTime(newRoomAvailability.CheckOutDate, newRoomAvailability.CheckOutTime);
                    newRoomAvailability.ReservationNo = request.ReservationNo;
                    newRoomAvailability.BookingId = newBooking.BookingId;
                    newRoomAvailability.RoomId = newBooking.RoomId;
                    newRoomAvailability.RoomStatus = newBooking.Status;
                    newRoomAvailability.RoomTypeId = newBooking.RoomTypeId;
                    Constants.Constants.SetMastersDefault(newRoomAvailability, companyId, userId, currentDate);
                    await _context.RoomAvailability.AddAsync(newRoomAvailability);


                    //advance services
                    var advanceServices = await _context.AdvanceServices.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == request.BookingId).ToListAsync();

                    if (advanceServices.Count > 0)
                    {
                        foreach (var service in advanceServices)
                        {
                            if (Convert.ToDateTime(service.ServiceDate) >= request.ShiftDate)
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
                    DateTime tempDate = DateTime.Now;
                    string tempTime = "";
                    (tempDate, tempTime) = Constants.Calculation.GetAMinuteAfter(booking.CheckOutDate, booking.CheckOutTime);

                    string isRoomAvailable = await CheckRoomAvailable(tempDate, tempTime, request.ExtendedDate, booking.CheckOutTime, booking.RoomTypeId, booking.RoomId);

                    if (isRoomAvailable != "success")
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = isRoomAvailable });
                    }

                    //calculate nights
                    int nights = Constants.Calculation.CalculateNights(booking.CheckOutDate, request.ExtendedDate);

                    var (code, message, response) = await CalculateRoomRateAsync(companyId, booking.RoomTypeId, booking.CheckOutDate, request.ExtendedDate, booking.CheckoutFormat, 1, nights, booking.GstType, 0);
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

                    var newbookedRoomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).OrderBy(x => x.BookingDate).ToListAsync();

                    (booking.BookingAmount, booking.GstAmount, booking.TotalBookingAmount) = CalculateTotalBookedRoomRate(newbookedRoomRates);
                    booking.TotalAmount = BookingCalulation.BookingTotalAmount(booking);

                    booking.CheckOutDate = request.ExtendedDate;
                    booking.InitialCheckOutDate = request.ExtendedDate;
                    booking.CheckOutDateTime = Constants.Calculation.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);
                    booking.InitialCheckOutDateTime = Constants.Calculation.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);
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

                    roomAvailaibility.CheckOutDate = request.ExtendedDate;
                    roomAvailaibility.CheckOutDateTime = Constants.Calculation.ConvertToDateTime(roomAvailaibility.CheckOutDate, roomAvailaibility.CheckOutTime);
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
                                                     HourId = booking.HourId,
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
        public async Task<IActionResult> GetBookingsOnCheckOut(string reservationNo, string guestId)
        {
            try
            {
                var response = new CheckOutResponse();
                response.ReservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);


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

                //Get primary guest details
                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.GuestId == response.ReservationDetails.PrimaryGuestId);


                var allBookingsCount = await _context.BookingDetail.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo && x.Status != Constants.Constants.CheckOut).CountAsync();

                response.BookingDetails = await (from booking in _context.BookingDetail
                                                 join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                                 join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                                 join guest in _context.GuestDetails
                                                 on booking.GuestId equals guest.GuestId
                                                 where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == reservationNo && booking.Status == Constants.Constants.CheckIn
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
                                                     HourId = booking.HourId,
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
                                                     InvoiceName = "",
                                                     BillTo = "",
                                                     InvoiceNo = "",
                                                     InvoiceDate = DateOnly.MinValue,
                                                     TotalAmount = booking.TotalAmount,
                                                     ServicesAmount = booking.ServicesAmount,
                                                     ServicesTaxAmount = booking.ServicesTaxAmount,
                                                     TotalServicesAmount = booking.TotalServicesAmount,
                                                     BookedRoomRates = _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList(),
                                                     GuestDetails = guest,
                                                     AdvanceServices = _context.AdvanceServices.Where(x => x.IsActive == true && x.BookingId == booking.BookingId).ToList()
                                                 }).ToListAsync();


                if (response.BookingDetails.Count == 0)
                {
                    return Ok(new { Code = 500, Message = "Bookings not found" });
                }
                response.PaymentDetails = await _context.PaymentDetails.Where(x => x.IsActive == true && x.IsReceived == false && x.PaymentLeft > 0 && x.CompanyId == companyId && x.ReservationNo == reservationNo).ToListAsync();

                response.PaymentSummary = await CalculateSummary(response.ReservationDetails, response.BookingDetails);

                CalculateCheckOutInvoice(response.BookingDetails, response.PaymentDetails);

                foreach (var item in response.BookingDetails)
                {
                    item.BalanceAmount = CalculateCheckOutBalanceBooking(item);
                }

                if(allBookingsCount == response.BookingDetails.Count)
                {
                    Dictionary<int, decimal> refundAmouts = new Dictionary<int, decimal>();
                    //calculate refund
                    foreach (var pay in response.PaymentDetails)
                    {

                        if (pay.RoomId == 0 && pay.BookingId == 0)
                        {
                            pay.IsReceived = true;
                            pay.RefundAmount = pay.PaymentLeft;
                            pay.PaymentLeft = 0;

                            decimal equallydivide = 0;
                            if (pay.RefundAmount > 0)
                            {
                                equallydivide = EquallyDivideValue(pay.RefundAmount, pay.InvoiceHistories.Count);
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
                        foreach (var item in response.BookingDetails)
                        {
                            if (item.BookingId == kvp.Key)
                            {
                                item.RefundAmount = kvp.Value;
                            }
                        }


                    }
                }
                

                return Ok(new { Code = 200, Message = "Data fetch successfully", data = response });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
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

                // Or convert to a list if needed
                List<int> bookingIdList = request.Bookings.Keys.ToList();


                int allBookingsCount = await _context.BookingDetail.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == request.ReservationDetails.ReservationNo && x.Status != Constants.Constants.CheckOut).CountAsync();

                bool isAllRoomCheckOut = allBookingsCount == bookingIdList.Count ? true : false;

                List<BookingDetail> bookings = await (from booking in _context.BookingDetail
                                                            join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                                            join guest in _context.GuestDetails
                                                            on booking.GuestId equals guest.GuestId
                                                            where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == request.ReservationDetails.ReservationNo && bookingIdList.Contains(booking.BookingId)
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
                                                                HourId = booking.HourId,
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
                                                                InvoiceName = "",
                                                                BillTo = "",
                                                                InvoiceNo = "",
                                                                InvoiceDate = DateOnly.MinValue,
                                                                TotalAmount = booking.TotalAmount,
                                                                ServicesAmount = booking.ServicesAmount,
                                                                ServicesTaxAmount = booking.ServicesTaxAmount,
                                                                TotalServicesAmount = booking.TotalServicesAmount,
                                                                BookedRoomRates = _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList(),
                                                                GuestDetails = guest,
                                                                AdvanceServices = _context.AdvanceServices.Where(x => x.IsActive == true && x.BookingId == booking.BookingId).ToList()
                                                            }).ToListAsync();

               
                


                var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && bookingIdList.Contains(x.BookingId)).ToListAsync();

                foreach (var item in bookings)
                {
                    if (!(request.Bookings.TryGetValue(item.BookingId, out DateTime value)))
                    {

                        return Ok(new { Code = 400, Message = "Invalid data" });
                    }
                    //set checkout date
                    (item.CheckOutDate, item.CheckOutTime) = Constants.Calculation.GetDateTime(value);

                    item.CheckOutDateTime = Constants.Calculation.ConvertToDateTime(item.CheckOutDate, item.CheckOutTime);
                    item.NoOfNights = Constants.Calculation.CalculateNights(item.ReservationDate, item.CheckOutDate);
                    item.BookingAmount = 0;
                    item.GstAmount = 0;
                    item.TotalBookingAmount = 0;
                    item.TotalAmount = 0;


                    var eachRoomRate = roomRates.Where(x => x.BookingId == item.BookingId && (item.NoOfNights == 1
                    ? x.BookingDate <= item.CheckOutDate : x.BookingDate < item.CheckOutDate)).OrderBy(x => x.BookingDate).ToList();



                    item.BookedRoomRates = eachRoomRate;

                    foreach (var rate in eachRoomRate)
                    {
                        item.BookingAmount = item.BookingAmount + rate.RoomRate;
                        item.GstAmount = item.GstAmount + rate.GstAmount;
                        item.TotalBookingAmount = item.TotalBookingAmount + rate.TotalRoomRate;

                    }
                    item.TotalAmount = BookingCalulation.BookingTotalAmount(item);
                }

                PaymentSummary paymentSummary = await CalculateSummary(request.ReservationDetails, bookings);

                response.BookingDetails = bookings;
                response.PaymentSummary = paymentSummary;

                response.PaymentDetails = await _context.PaymentDetails.Where(x => x.IsActive == true && x.IsReceived == false && x.PaymentLeft > 0 && x.CompanyId == companyId && x.ReservationNo == request.ReservationDetails.ReservationNo).ToListAsync();



                CalculateCheckOutInvoice(response.BookingDetails, response.PaymentDetails);

                foreach (var item in response.BookingDetails)
                {
                    item.BalanceAmount = CalculateCheckOutBalanceBooking(item);
                }

                Dictionary<int, decimal> refundAmouts = new Dictionary<int, decimal>();
                //calculate refund
                if (isAllRoomCheckOut)
                {
                    foreach (var pay in response.PaymentDetails)
                    {

                        if (pay.RoomId == 0 && pay.BookingId == 0)
                        {
                            pay.IsReceived = true;
                            pay.RefundAmount = pay.PaymentLeft;
                            pay.PaymentLeft = 0;

                            decimal equallydivide = 0;
                            if (pay.RefundAmount > 0)
                            {
                                equallydivide = EquallyDivideValue(pay.RefundAmount, pay.InvoiceHistories.Count);
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
                        foreach (var item in response.BookingDetails)
                        {
                            if (item.BookingId == kvp.Key)
                            {
                                item.RefundAmount = kvp.Value;
                            }
                        }


                    }

                }



                return Ok(new { Code = 200, Message = "Rates fetched successfully", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        //CHECKOUT INVOICE CALCULATION
        private void CalculateCheckOutInvoice(List<BookingDetail> bookings, List<PaymentDetails> payments)
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
                        decimal balance = CalculateCheckOutBalanceBooking(booking);
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
                int roomCounts = GetBalanceRoomCount(bookings);
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(agentAdvance, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateCheckOutBalanceBooking(bookings[i]);
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
                int roomCounts = GetBalanceRoomCount(bookings);
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(advanceAmount, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateCheckOutBalanceBooking(bookings[i]);
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
                int roomCounts = GetBalanceRoomCount(bookings);
                if (roomCounts == 0)
                {
                    return;
                }
                List<decimal> equallyDivideArr = EquallyDivideAmount(receivedAmount, roomCounts);
                int divideArrIndex = 0;


                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateCheckOutBalanceBooking(bookings[i]);
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
                    if (!IsTodayCheckOutDate(item.CheckOutDate))
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Check Out Date is not equal to today's date" });
                    }
                    var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();
                    //item.BookingAmount = 0;
                    //item.GstAmount = 0;
                    //item.TotalBookingAmount = 0;
                    //item.TotalAmount = 0;
                    int noOfnights = Constants.Calculation.CalculateNights(item.ReservationDate, item.CheckOutDate);

                    foreach (var rate in roomRates)
                    {

                        if (noOfnights == 1 ? rate.BookingDate <= item.CheckOutDate : rate.BookingDate < item.CheckOutDate)
                        {
                            //item.BookingAmount = item.BookingAmount + rate.RoomRate;
                            //item.GstAmount = item.GstAmount + rate.GstAmount;
                            //item.TotalBookingAmount = item.TotalBookingAmount + rate.TotalRoomRate;


                        }
                        else
                        {
                            rate.IsActive = false;
                            rate.UpdatedDate = currentTime;
                            _context.BookedRoomRates.Update(rate);

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
                    booking.CheckOutDateTime = Constants.Calculation.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);
                    booking.NoOfNights = Constants.Calculation.CalculateNights(booking.ReservationDate, booking.CheckOutDate);
                    booking.Status = Constants.Constants.CheckOut;
                    booking.UpdatedDate = currentTime;
                    booking.InitialBalanceAmount = item.BalanceAmount;
                    booking.BalanceAmount = item.BalanceAmount;
                    booking.RefundAmount = item.RefundAmount;
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
                    booking.BillTo = item.BillTo;
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
                    roomAvailability.CheckOutDateTime = Constants.Calculation.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);
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

        private bool IsTodayCheckOutDate(DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        [HttpGet("GetRoomsById")]
        public async Task<IActionResult> GetRoomsById(int bookingId, int roomId)
        {
            try
            {
                var response = new CheckInRoomData();
               
                if (bookingId == 0)
                {
                    return Ok(new { code = 400, Message = "Invalid data" });
                }

                response.BookingDetail = await (from booking in _context.BookingDetail
                                                join room in _context.RoomMaster on booking.RoomId equals room.RoomId
                                                join guest in _context.GuestDetails on booking.GuestId equals guest.GuestId
                                                join type in _context.RoomCategoryMaster on booking.RoomTypeId equals type.Id
                                                where booking.IsActive == true && booking.CompanyId == companyId && booking.BookingId == bookingId
                                                select new BookingDetailCheckInDTO
                                                {
                                                    BookingId = booking.BookingId,
                                                    GuestId = booking.GuestId,
                                                    GuestName = guest.GuestName,
                                                    GuestPhone = guest.PhoneNumber,
                                                    RoomId = booking.RoomId,
                                                    RoomTypeId = booking.RoomTypeId,
                                                    CheckInDate = booking.CheckInDate.ToString("yyyy-MM-dd"),
                                                    CheckInTime = booking.CheckInTime,
                                                    CheckOutDate = booking.CheckOutDate.ToString("yyyy-MM-dd"),
                                                    CheckOutTime = booking.CheckOutTime,
                                                    CheckInDateTime = booking.CheckInDateTime,
                                                    CheckOutDateTime = booking.CheckOutDateTime,
                                                    NoOfNights = booking.NoOfNights,
                                                    NoOfHours = booking.NoOfHours,
                                                    HourId = booking.HourId,
                                                    Pax = booking.Pax,
                                                    Status = booking.Status,
                                                    Remarks = booking.Remarks,
                                                    ReservationNo = booking.ReservationNo,
                                                    UserId = booking.UserId,
                                                    GstType = booking.GstType,
                                                    CompanyId = booking.CompanyId,
                                                    BookingAmount = booking.BookingAmount,
                                                    GstAmount = booking.GstAmount,
                                                    TotalBookingAmount = booking.TotalBookingAmount,
                                                    BookingSource = booking.BookingSource,
                                                    ReservationDate = booking.ReservationDate.ToString("yyyy-MM-dd"),
                                                    ReservationTime = booking.ReservationTime,
                                                    ReservationDateTime = booking.ReservationDateTime,
                                                    RoomCategoryName = type.Type,
                                                    RoomNo = room.RoomNo,
                                                    TotalAmount = booking.TotalAmount,
                                                    ServicesAmount = booking.ServicesAmount
                                                }).FirstOrDefaultAsync();

                if (response.BookingDetail == null)
                {
                    return Ok(new { Code = 400, Message = "No booking found" });
                }
                return Ok(new { Code = 200, Message = "Data fetched", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        
        [HttpGet("GetBookingsForCancel")]
        public async Task<IActionResult> GetBookingsForCancel(string reservationNo, int guestId, string cancelMethod, string calculatedBy)
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
                                            RoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
                                            RoomTypeId = booking.RoomTypeId,
                                            RoomTypeName = category.Type,
                                            CheckInDateTime = booking.CheckInDateTime,
                                            CheckOutDateTime = booking.CheckOutDateTime,
                                            ReservationDateTime = booking.ReservationDateTime,
                                            NoOfNights = booking.NoOfNights,
                                            Status = booking.Status,
                                            ReservationNo = booking.ReservationNo,
                                            ReservationDate = booking.ReservationDate,
                                            AdvanceAmount = booking.AdvanceAmount,
                                            ReceivedAmount = booking.ReceivedAmount,
                                            AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                            CancelAmount = booking.CancelAmount,
                                            TotalBookingAmount = booking.TotalBookingAmount,
                                            BookingAmount = booking.BookingAmount,
                                            GstAmount = booking.GstAmount,
                                            InvoiceNo = cancelBookingResponse.InvoiceNo,
                                            CancelDate = cancelDateTime,
                                            Pax=booking.Pax,
                                            GuestDetails = guest
                                        }
                                    ).ToList();
                cancelBookingResponse.IsAllCancel = allBookings.Count == bookingDetails.Count;

                foreach (var item in bookingDetails)
                {
                    item.BookedRoomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();
                }


                List<PaymentDetails> paymentDetails = await _context.PaymentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId && x.PaymentLeft > 0 && x.ReservationNo == reservationNo).ToListAsync();


                List<CancelPolicyMaster> cancelPolicies = await _context.CancelPolicyMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.DeductionBy == calculatedBy).ToListAsync();

                if (cancelPolicies.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "No cancel policy found" });
                }
                bool flag = CalculateCancelAmount(bookingDetails, cancelMethod, cancelPolicies, cancelDateTime);
                if (flag == false)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }

                CancelSummary cancelSummary = new CancelSummary();

                cancelBookingResponse.CancelSummary = CalculatePaymentSummary(cancelSummary, paymentDetails,bookingDetails);

                CalculateCancelInvoice(bookingDetails, paymentDetails);

                foreach (var item in bookingDetails)
                {
                    item.BalanceAmount = CalculateBalanceCancelAmount(item);
                }

                cancelBookingResponse.CancelSummary = CalculateCancelSummary(cancelSummary, bookingDetails);

                if (cancelBookingResponse.IsAllCancel)
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
                                equallydivide = EquallyDivideValue(pay.RefundAmount, pay.InvoiceHistories.Count);
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
                if (string.IsNullOrWhiteSpace(request.ReservationNo) || request.CancelDate == null || request.CancelDate == new DateTime(1900, 01, 01) || string.IsNullOrWhiteSpace(request.cancelMethod) || string.IsNullOrWhiteSpace(request.calculatedBy))
                {
                    return Ok(new { Code = 500, Message = "Invalid data" });
                }

                if (request.BookingIds.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "Bookings fetched successfully", data = cancelBookingResponse });
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
                                            RoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
                                            RoomTypeId = booking.RoomTypeId,
                                            RoomTypeName = category.Type,
                                            CheckInDateTime = booking.CheckInDateTime,
                                            CheckOutDateTime = booking.CheckOutDateTime,
                                            ReservationDateTime = booking.ReservationDateTime,
                                            NoOfNights = booking.NoOfNights,
                                            Status = booking.Status,
                                            ReservationNo = booking.ReservationNo,
                                            ReservationDate = booking.ReservationDate,
                                            AdvanceAmount = booking.AdvanceAmount,
                                            ReceivedAmount = booking.ReceivedAmount,
                                            AdvanceReceiptNo = booking.AdvanceReceiptNo,
                                            CancelAmount = booking.CancelAmount,
                                            TotalBookingAmount = booking.TotalBookingAmount,
                                            BookingAmount = booking.BookingAmount,
                                            GstAmount = booking.GstAmount,
                                            InvoiceNo = cancelBookingResponse.InvoiceNo,
                                            CancelDate = request.CancelDate,
                                            GuestDetails = guest,
                                            Pax = booking.Pax,
                                        }
                                    ).ToList();


                cancelBookingResponse.IsAllCancel = allbookingDetails.Count == bookingDetails.Count ? true : false;
               

                foreach (var item in bookingDetails)
                {
                    item.BookedRoomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();
                }


                List<PaymentDetails> paymentDetails = await _context.PaymentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId && x.PaymentLeft > 0 && x.ReservationNo == request.ReservationNo).ToListAsync();


                List<CancelPolicyMaster> cancelPolicies = await _context.CancelPolicyMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.DeductionBy == request.calculatedBy).ToListAsync();

                if (cancelPolicies.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "No cancel policy found" });
                }
                bool flag = CalculateCancelAmount(bookingDetails, request.cancelMethod, cancelPolicies, request.CancelDate);
                if (flag == false)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }

                CancelSummary cancelSummary = new CancelSummary();
                cancelBookingResponse.CancelSummary = CalculatePaymentSummary(cancelSummary, paymentDetails, bookingDetails);

                CalculateCancelInvoice(bookingDetails, paymentDetails);

                foreach (var item in bookingDetails)
                {
                    item.BalanceAmount = CalculateBalanceCancelAmount(item);
                }

                cancelBookingResponse.CancelSummary = CalculateCancelSummary(cancelSummary, bookingDetails);


                Dictionary<int, decimal> refundAmouts = new Dictionary<int, decimal>();
                //calculate refund
                if (cancelBookingResponse.IsAllCancel)
                {
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
                                equallydivide = EquallyDivideValue(pay.RefundAmount, pay.InvoiceHistories.Count);
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

                cancelBookingResponse.bookingDetails = bookingDetails;
                cancelBookingResponse.PaymentDetails = paymentDetails;





                return Ok(new { Code = 200, Message = "Bookings fetched successfully", data = cancelBookingResponse });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        //CANCEL BOOKING INVOICE CANCELLATION
        private bool CalculateCancelAmount(List<BookingDetail> bookings, string cancelMethod, List<CancelPolicyMaster> cancelPolicies, DateTime cancelDate)
        {
            bool flag = true;
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
                                BookedRoomRate dateWiseRate = item.BookedRoomRates.FirstOrDefault(x => x.BookingDate.Date == dateTime.Date);
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
                    int noOfHours = NoOfHours(cancelDate, item.ReservationDate);
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
                        item.RoomCancelHistory.Add(CreateRoomCancelHistory(item, cancelPolicy, cancelDate, bookings.Count, noOfHours, item.ReservationDate, item.CancelAmount, cancelMethod));
                    }
                    else
                    {
                        flag = false;
                        return flag;
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
             && (x.FromTime < hours && x.ToTime >= hours)
            );

            return cancelPolicy;
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

        private CancelSummary CalculatePaymentSummary(CancelSummary cancelSummary , List<PaymentDetails> paymentDetails,List<BookingDetail> bookingDetails)
        {
            foreach (var pay in paymentDetails)
            {
                if (pay.PaymentStatus == Constants.Constants.AdvancePayment)
                {
                    cancelSummary.AdvanceAmount = cancelSummary.AdvanceAmount + pay.PaymentLeft;
                }
                else if (pay.PaymentStatus == Constants.Constants.AgentPayment)
                {
                    cancelSummary.AgentAmount = cancelSummary.AgentAmount + pay.PaymentLeft;
                }
                else
                {
                    if(pay.BookingId == 0 && pay.RoomId == 0)
                    {
                        cancelSummary.ReceivedAmount = cancelSummary.ReceivedAmount + pay.PaymentLeft;
                    }
                    else if (bookingDetails.Select(x => x.BookingId).Contains(pay.BookingId))
                    {
                        cancelSummary.ReceivedAmount = cancelSummary.ReceivedAmount + pay.PaymentLeft;
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
        public async Task<IActionResult> GetCheckOutInvoiceData(string reservationNo, int bookingId, string invoiceNo, string invoiceFormat)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                var response = new CheckOutResponse();

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.PropertyId == companyId && x.IsActive == true);

                if (property == null)
                {
                    return Ok(new { Code = 400, Message = "Company details not found" });
                }

                var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.ReservationNo == reservationNo);

                if (reservationDetails == null)
                {
                    return Ok(new { Code = 400, Message = "Reservation details not found" });
                }
                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == reservationDetails.PrimaryGuestId);
                List<BookingDetail> BookingDetails = new List<BookingDetail>();

                if (invoiceFormat == Constants.Constants.ReservationInvoice)
                {
                    BookingDetails = await (from booking in _context.BookingDetail
                                            join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                            join guest in _context.GuestDetails
                                            on booking.GuestId equals guest.GuestId
                                            where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == reservationNo && booking.Status == Constants.Constants.CheckOut && booking.InvoiceNo == invoiceNo
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
                                                HourId = booking.HourId,
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

                }
                else
                {
                    BookingDetails = await (from booking in _context.BookingDetail
                                            join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                            join guest in _context.GuestDetails
                                            on booking.GuestId equals guest.GuestId
                                            where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == reservationNo && booking.Status == Constants.Constants.CheckOut && booking.BookingId == bookingId
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
                                                HourId = booking.HourId,
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

                }

                if (BookingDetails.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "No bookings found" });
                }

                response.InvoiceDate = BookingDetails[0].InvoiceDate;
                response.InvoiceNo = BookingDetails[0].InvoiceNo;
                response.InvoiceName = BookingDetails[0].InvoiceName;

                PaymentSummary paymentSummary = await CalculateSummary(reservationDetails, BookingDetails);

                response.BookingDetails = BookingDetails;
                response.ReservationDetails = reservationDetails;
                response.PaymentSummary = paymentSummary;
                return Ok(new { Code = 200, Message = "Data fetched successfully", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetCancelInvoiceData")]
        public async Task<IActionResult> GetCancelInvoiceData(string reservationNo, int bookingId, string invoiceNo, string invoiceFormat)
        {
            try
            {

                var response = new CancelBookingResponse();

                var property = await _context.CompanyDetails.FirstOrDefaultAsync(x => x.PropertyId == companyId && x.IsActive == true);

                if (property == null)
                {
                    return Ok(new { Code = 400, Message = "Company details not found" });
                }

                var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.ReservationNo == reservationNo);

                if (reservationDetails == null)
                {
                    return Ok(new { Code = 400, Message = "Reservation details not found" });
                }
                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == reservationDetails.PrimaryGuestId);
                List<BookingDetail> BookingDetails = new List<BookingDetail>();

                if (invoiceFormat == Constants.Constants.ReservationInvoice)
                {
                    BookingDetails = await (from booking in _context.BookingDetail
                                            join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                            join guest in _context.GuestDetails
                                            on booking.GuestId equals guest.GuestId
                                            where booking.CompanyId == companyId && booking.ReservationNo == reservationNo && booking.Status == Constants.Constants.Cancel && booking.InvoiceNo == invoiceNo
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
                                                HourId = booking.HourId,
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
                                                RoomCancelHistory = _context.RoomCancelHistory.Where(x=>x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList()
                                            }).ToListAsync();

                }
                else
                {
                    BookingDetails = await (from booking in _context.BookingDetail
                                            join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                            join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                            join guest in _context.GuestDetails
                                            on booking.GuestId equals guest.GuestId
                                            where booking.IsActive == true && booking.CompanyId == companyId && booking.ReservationNo == reservationNo && booking.Status == Constants.Constants.CheckOut && booking.BookingId == bookingId
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
                                                HourId = booking.HourId,
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
                                                RoomCancelHistory = _context.RoomCancelHistory.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList()
                                            }).ToListAsync();

                }

                if (BookingDetails.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "No bookings found" });
                }

                response.InvoiceDate = BookingDetails[0].InvoiceDate;
                response.InvoiceNo = BookingDetails[0].InvoiceNo;
                response.InvoiceName = BookingDetails[0].InvoiceName;

              //  CancelSummary paymentSummary = await CalculateCancelSummary(reservationDetails, BookingDetails);

                response.bookingDetails = BookingDetails;
                response.ReservationDetails = reservationDetails;
                //response.CancelSummary = paymentSummary;
                return Ok(new { Code = 200, Message = "Data fetched successfully", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        //GET ROOM AVAILABILITY
        private async Task<DataSet> GetRoomAvailability(DateTime checkInDate, string checkInTime, DateTime checkOutDate, string checkOutTime, string pageName = "", int roomTypeId = 0, int roomId = 0)
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
        private async Task<string> CheckRoomAvailable(DateTime checkInDate, string checkInTime, DateTime checkOutDate, string checkOutTime, int roomTypeId, int roomId)
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
            int companyId, int roomTypeId, DateTime checkInDate, DateTime checkOutDate,
            string checkOutFormat, int noOfRooms, int noOfNights, string gstType, int hourId)
        {
            var roomRateResponse = new RoomRateResponse();

            if (roomTypeId == 0 || checkOutFormat == "")
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
            else
            {

                DateTime currentDate = checkInDate;
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
            }

            roomRateResponse.TotalBookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRateResponse.GstAmount);

            //total amount
            roomRateResponse.AllRoomsAmount = Calculation.RoundOffDecimal(noOfRooms * roomRateResponse.BookingAmount);
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
            invoiceHistory.PaymentAmount = payment.PaymentAmount;
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
            }
            summary.AgentServiceCharge = reservationDetails.AgentServiceCharge;
            summary.AgentServiceGst = reservationDetails.AgentServiceGstAmount;
            summary.AgentServiceTotal = reservationDetails.AgentTotalServiceCharge;

            summary.TotalPayable = summary.TotalAllAmount + summary.AgentServiceTotal;

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

        private PaymentSummary CalculateCheckInSummary(ReservationDetails reservationDetails, List<BookingDetailCheckInDTO> bookings, List<PaymentDetails> payments)
        {

            var summary = new PaymentSummary();


            foreach (var item in bookings)
            {
                //booking
                summary.TotalRoomAmount = summary.TotalRoomAmount + item.BookingAmount;
                summary.TotalGstAmount = summary.TotalGstAmount + item.GstAmount;
                summary.TotalAmount = summary.TotalAmount + item.TotalBookingAmount;
                summary.TotalAllAmount = summary.TotalAllAmount + item.TotalAmount;

                //advance services
                summary.RoomServiceAmount = summary.RoomServiceAmount + item.ServicesAmount;
                
            }
            summary.AgentServiceCharge = reservationDetails.AgentServiceCharge;
            summary.AgentServiceGst = reservationDetails.AgentServiceGstAmount;
            summary.AgentServiceTotal = reservationDetails.AgentTotalServiceCharge;

            summary.TotalPayable = summary.TotalAllAmount + summary.AgentServiceTotal;

            
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

        [HttpGet("GetCancellableBookings")]
        public async Task<IActionResult> GetCancellableBookings(string reservationNo, int guestId)
        {
            try
            {
                var checkInResponse = new CheckInResponse();
                if (string.IsNullOrEmpty(reservationNo) || guestId == 0)
                {
                    return Ok(new { Code = 500, Message = "Invalid data" });
                }
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

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


                checkInResponse.BookingDetailCheckInDTO = await (
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
                                            && booking.ReservationNo == reservationNo
                                        select new BookingDetailCheckInDTO
                                        {
                                            BookingId = booking.BookingId,
                                            GuestId = booking.GuestId,
                                            RoomId = booking.RoomId,
                                            RoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
                                            OriginalRoomId = booking.RoomId,
                                            OriginalRoomNo = bookrooms == null ? "" : bookrooms.RoomNo,
                                            RoomTypeId = booking.RoomTypeId,
                                            RoomCategoryName = category.Type,
                                            OriginalRoomTypeId = booking.RoomTypeId,
                                            OriginalRoomCategoryName = category.Type,
                                            CheckInDate = booking.CheckInDate.ToString("yyyy-MM-dd"),
                                            CheckInTime = booking.CheckInTime,
                                            CheckOutDate = booking.CheckOutDate.ToString("yyyy-MM-dd"),
                                            CheckOutTime = booking.CheckOutTime,
                                            CheckInDateTime = booking.CheckInDateTime,
                                            CheckOutDateTime = booking.CheckOutDateTime,
                                            NoOfNights = booking.NoOfNights,
                                            NoOfHours = booking.NoOfHours,
                                            HourId = booking.HourId,
                                            Status = booking.Status,
                                            Remarks = booking.Remarks,
                                            ReservationNo = booking.ReservationNo,
                                            UserId = booking.UserId,
                                            CompanyId = booking.CompanyId,
                                            BookingAmount = booking.BookingAmount,
                                            GstType = booking.GstType,
                                            GstAmount = booking.GstAmount,
                                            TotalBookingAmount = booking.TotalBookingAmount,
                                            BookingSource = booking.BookingSource,
                                            ReservationDate = booking.ReservationDate.ToString("yyyy-MM-dd"),
                                            ReservationTime = booking.ReservationTime,
                                            ReservationDateTime = booking.ReservationDateTime,
                                            Pax = booking.Pax,
                                            OriginalPax = booking.Pax,
                                            IsSameGuest = booking.PrimaryGuestId == booking.GuestId ? true : false,
                                            OriginalReservationDateTime = booking.ReservationDateTime,
                                            OriginalReservationDate = booking.ReservationDate.ToString("yyyy-MM-dd"),
                                            OriginalReservationTime = booking.ReservationTime,
                                            OriginalCheckInDate = booking.CheckInDate.ToString("yyyy-MM-dd"),
                                            OriginalCheckInTime = booking.CheckInTime,
                                            OriginalCheckOutDate = booking.CheckOutDate.ToString("yyyy-MM-dd"),
                                            OriginalCheckOutTime = booking.CheckOutTime,
                                            CheckOutFormat = booking.CheckoutFormat,
                                            IsCheckBox = booking.Status != Constants.Constants.CheckOut && _context.AdvanceServices.Any(s => s.BookingId == booking.BookingId && s.CompanyId == companyId && s.IsActive)
                                        }
                                    ).ToListAsync();

                foreach (var item in checkInResponse.BookingDetailCheckInDTO)
                {
                    item.BookedRoomRates = roomRates.Where(x => x.BookingId == item.BookingId).ToList();
                    item.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == item.GuestId) ?? new GuestDetails();
                }



                //payment details
                checkInResponse.PaymentDetails = await (from x in _context.PaymentDetails
                                                        join room in _context.RoomMaster on x.RoomId equals room.RoomId into roomT
                                                        from rm in roomT.DefaultIfEmpty()
                                                        where x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo
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
                                                            CreatedDate = x.CreatedDate,
                                                            UpdatedDate = x.UpdatedDate,
                                                            CompanyId = x.CompanyId,
                                                            RoomNo = rm != null ? rm.RoomNo : ""
                                                        }).ToListAsync();



                //payment summary
                var paymentSummary = new PaymentSummary();
                paymentSummary.TotalRoomAmount = checkInResponse.ReservationDetails.TotalRoomPayment;
                paymentSummary.TotalGstAmount = checkInResponse.ReservationDetails.TotalGst;
                paymentSummary.TotalAmount = checkInResponse.ReservationDetails.TotalAmount;
                paymentSummary.AgentPaid = checkInResponse.PaymentDetails.Where(x => x.PaymentStatus == Constants.Constants.AgentPayment).Sum(x => x.PaymentAmount);
                paymentSummary.AdvanceAmount = checkInResponse.PaymentDetails.Where(x => x.PaymentStatus == Constants.Constants.AdvancePayment).Sum(x => x.PaymentAmount);
                paymentSummary.ReceivedAmount = checkInResponse.PaymentDetails.Where(x => x.PaymentStatus == Constants.Constants.ReceivedPayment).Sum(x => x.PaymentAmount);
                var balance = paymentSummary.TotalAmount - (paymentSummary.AgentPaid + paymentSummary.AdvanceAmount + paymentSummary.ReceivedAmount);
                paymentSummary.BalanceAmount = balance > 0 ? balance : 0;
                paymentSummary.RefundAmount = balance < 0 ? Math.Abs(balance) : 0;

                checkInResponse.PaymentSummary = paymentSummary;



                return Ok(new { Code = 200, Message = "Data fetched successfully", data = checkInResponse });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

    }
}
