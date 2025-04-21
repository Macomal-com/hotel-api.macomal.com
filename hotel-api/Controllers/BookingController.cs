using AutoMapper;
using Azure.Core;
using hotel_api.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.DTO;
using Repository.Models;
using Repository.RequestDTO;
using RepositoryModels.Repository;
using System.ComponentModel.Design;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;

namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController(DbContextSql context, IMapper mapper) : ControllerBase
    {
        private readonly DbContextSql _context = context;
        private readonly IMapper _mapper = mapper;

        
        [HttpGet("CheckRoomAvailaibility")]
        public async Task<IActionResult> CheckRoomAvailaibility(DateTime checkInDate, string checkInTime, DateTime checkOutDate, string checkOutTime, string pageName = "", int roomTypeId = 0)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                if (checkInDate == null || checkOutDate == null)
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
                    return Ok(new { Code = 200, message = "Room retrieved successfully.", data = result , AvailableRooms  = result.Count});
                }
                else
                {
                    DataSet dataSet = await GetRoomAvailability(checkInDate, checkInTime, checkOutDate, checkOutTime, pageName, roomTypeId);
                    if(dataSet == null)
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
        public async Task<IActionResult> GetReservationFormData(string type)
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

                var agentDetails = await _context.AgentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId).Select(x=> new
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
        public async Task<IActionResult> ReservationRoomRate(int roomTypeId, DateTime checkInDate, DateTime checkOutDate,  string checkOutFormat, int noOfRooms, int noOfNights, string gstType, int hourId = 0)
        {
            try
            {
                var roomRateResponse = new RoomRateResponse();
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                if (roomTypeId == 0 || checkOutFormat == "")
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }

                var gstPercentage = await GetGstPercetage(Constants.Constants.Reservation);
                if (gstPercentage == null)
                {
                    return Ok(new { Code = 500, Message = "Gst percentage not found for reservation" });
                }
                //if checkout format is sameday
                if (checkOutFormat == Constants.Constants.SameDayFormat)
                {
                    var roomRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId && x.HourId == hourId).FirstOrDefaultAsync();
                    if (roomRates == null)
                    {
                        return Ok(new { Code = 200, Message = "No Room Rates found" });
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

                        var customRoomRates = await _context.RoomRateDateWise.Where(x => x.IsActive == true && x.CompanyId == companyId && (x.FromDate <= currentDate && x.ToDate >= currentDate)).OrderByDescending(x => x.RatePriority).FirstOrDefaultAsync();
                        if(customRoomRates == null)
                        {
                           //fetch standard room rates
                            var roomRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId).FirstOrDefaultAsync();
                            if (roomRates == null)
                            {
                                return Ok(new { Code = 200, Message = "No Room Rates found" });
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
                
                return Ok(new { Code = 200, Message = "Room rate fetched successfully", data = roomRateResponse });

            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        private async Task<GstMaster> GetGstPercetage(string service)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            var gstPercentage = await _context.GstMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ApplicableServices == service).FirstOrDefaultAsync();

            if(gstPercentage == null)
            {
                return null;
            }
            else
            {
                if(gstPercentage.GstType == Constants.Constants.MultipleGst)
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


        [HttpGet("CalculateAgentCommision")]
        public async Task<IActionResult> CalculateAgentCommision(int agentId, decimal bookingAmount, decimal totalAmountWithGst)
        {
            try
            {
                var agentCommissionResponse = new AgentCommissionResponse();
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);

                if (agentId == 0)
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }
                var agentDetails = await _context.AgentDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.AgentId == agentId);
                if (agentDetails == null)
                {
                    return Ok(new { Code = 500, Message = "No agent found" });
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
                return Ok(new { Code = 200, Message = "Agent rate fetched successfully", data = agentCommissionResponse });


            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }


        }

        private async Task<DataSet> GetRoomAvailability(DateTime checkInDate, string checkInTime, DateTime checkOutDate, string checkOutTime, string pageName = "", int roomTypeId = 0, int roomId = 0)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
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


        [HttpPost("SaveReservation")]
        public async Task<IActionResult> SaveReservation([FromBody] ReservationRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var currentDate = DateTime.Now;
            try
            {
                if (request == null || request.GuestDetailsDTO == null || request.BookingDetailsDTO == null || request.BookingDetailsDTO.Count == 0 || request.PaymentDetailsDTO == null || request.ReservationDetailsDTO == null || request.AgentPaymentDetailsDTO ==null)
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
                                return Ok(new { Code = 400, Message = "Room not found" });
                            }
                            else
                            {
                                if (rows[0]["roomStatus"].ToString() != Constants.Constants.Clean)
                                {
                                    return Ok(new { code = 400, message = "Room " + rows[0]["RoomNo"] + " is already reserved with Reservation No " + rows[0]["ReservationNo"], data = new object { } });
                                }
                            }


                        }
                    }


                }

                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);


                //Insert Guest Details
                var guest = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == request.GuestDetailsDTO.GuestId);
                
                if (guest == null)
                {
                    guest = _mapper.Map<GuestDetails>(request.GuestDetailsDTO);
                    Constants.Constants.SetMastersDefault(guest, companyId, userId, currentDate);
                    //if (GuestImage != null)
                    //{
                    //    request.GuestDetailsDTO.GuestImage = await Constants.Constants.AddFile(GuestImage);
                    //}
                    await _context.GuestDetails.AddAsync(guest);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _mapper.Map(request.GuestDetailsDTO, guest);                    
                    //if (GuestImage != null)
                    //{
                    //    guest.GuestImage = await Constants.Constants.AddFile(GuestImage);
                    //}
                    
                    guest.UpdatedDate = currentDate;
                    _context.GuestDetails.Update(guest);
                    await _context.SaveChangesAsync();
                }

                //save reservation details
                var reservationDetails = _mapper.Map<ReservationDetails>(request.ReservationDetailsDTO);
                reservationDetails.PrimaryGuestId = guest.GuestId;
                Constants.Constants.SetMastersDefault(reservationDetails, companyId, userId, currentDate);

                (reservationDetails.TotalRoomPayment, reservationDetails.TotalGst, reservationDetails.TotalAmount) = Calculation.CalculateTotalRoomAmount(request.BookingDetailsDTO);

                if(reservationDetails.AgentId > 0)
                {
                    var gstPercentage = await GetGstPercetage(Constants.Constants.Agent);
                    if(gstPercentage == null)
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
                await _context.SaveChangesAsync();

                //save booking
                int roomCount = 1;
                foreach(var item in request.BookingDetailsDTO)
                {
                    foreach(var room in item.AssignedRooms)
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
                        bookingDetails.GuestId = room.RoomId > 0 ? guest.GuestId : 0;
                        bookingDetails.RoomId = room.RoomId;
                        bookingDetails.RoomCount = request.BookingDetailsDTO.Count == 1 && item.AssignedRooms.Count == 1 ? 0 : roomCount;
                        bookingDetails.BookingSource = request.ReservationDetailsDTO.BookingSource;
                        bookingDetails.TotalAmount = Constants.Calculation.BookingTotalAmount(bookingDetails);
                        roomCount++;

                        await _context.BookingDetail.AddAsync(bookingDetails);
                        await _context.SaveChangesAsync();

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
                        await _context.SaveChangesAsync();

                        //room rates
                        foreach (var rates in room.roomRates)
                        {
                            rates.BookingId = bookingDetails.BookingId;
                            rates.RoomId = room.RoomId;
                            rates.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                            Constants.Constants.SetMastersDefault(rates, companyId, userId, currentDate);

                            await _context.BookedRoomRates.AddAsync(rates);
                            await _context.SaveChangesAsync();
                        }


                    }


                }

               //paid to agent
                if(request.AgentPaymentDetailsDTO.PaymentAmount > 0)
                {
                    var paymentDetails = _mapper.Map<PaymentDetails>(request.AgentPaymentDetailsDTO);
                    Constants.Constants.SetMastersDefault(paymentDetails, companyId, userId, currentDate);
                    paymentDetails.BookingId = 0;
                    paymentDetails.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                    paymentDetails.PaymentLeft = request.PaymentDetailsDTO.PaymentAmount;
                    await _context.PaymentDetails.AddAsync(paymentDetails);
                    await _context.SaveChangesAsync();
                }
                
                //payment
                if(request.PaymentDetailsDTO.PaymentAmount > 0)
                {
                    var paymentDetails = _mapper.Map<PaymentDetails>(request.PaymentDetailsDTO);
                    Constants.Constants.SetMastersDefault(paymentDetails, companyId, userId, currentDate);
                    paymentDetails.BookingId = 0;
                    paymentDetails.ReservationNo = request.ReservationDetailsDTO.ReservationNo;
                    paymentDetails.PaymentLeft = request.PaymentDetailsDTO.PaymentAmount;
                    await _context.PaymentDetails.AddAsync(paymentDetails);
                    await _context.SaveChangesAsync();
                }

                var response = await UpdateDocumentNo(Constants.Constants.Reservation);
                if(response == null)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }

                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Reservation created successfully" });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        
        private async Task<string> UpdateDocumentNo(string BookingType)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                var getbookingno = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.Type == BookingType && x.CompanyId == companyId && x.IsActive == true);
                if (getbookingno == null)
                {
                    return null;
                }
                getbookingno.LastNumber = getbookingno.LastNumber + 1;
                await _context.SaveChangesAsync();
                return "success";
            }
            catch (Exception)
            {
                return null;
            }
        }


        [HttpGet("GetGuestList")]
        public async Task<IActionResult> GetGuestList(string status = "", string pageName = "")
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                
                    if(pageName == "addPayment")
                    {
                        var bookings = await (from booking in _context.BookingDetail
                                              join guest in _context.GuestDetails on booking.PrimaryGuestId equals guest.GuestId
                                              join rguest in _context.GuestDetails on booking.GuestId equals rguest.GuestId into roomguest
                                              from bookingguest in roomguest.DefaultIfEmpty()
                                              join r in _context.RoomMaster on new { booking.RoomId, CompanyId = companyId }
                                                equals new { RoomId = r.RoomId, r.CompanyId } into rooms
                                              from room in rooms.DefaultIfEmpty()
                                              where booking.CompanyId == companyId && booking.IsActive == true && guest.CompanyId == companyId

                                              && booking.Status == "Pending" || booking.Status == "Confirmed" || booking.Status == "CheckIn"
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
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetBookings")]
        public async Task<IActionResult> GetBookings(string reservationNo, int guestId)
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
                if(checkInResponse.ReservationDetails == null)
                {
                    return Ok(new { Code = 400, Message = $"No details for {reservationNo} reservation" });
                }

                checkInResponse.GuestDetails = await _context.GuestDetails.Where(x => x.CompanyId == companyId && x.IsActive && x.GuestId == guestId).FirstOrDefaultAsync();
                if(checkInResponse.GuestDetails == null)
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
                                            RoomTypeId = booking.RoomTypeId,
                                            RoomCategoryName = category.Type,
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
                                            ReservationDate =booking.ReservationDate.ToString("yyyy-MM-dd"),
                                            ReservationTime = booking.ReservationTime,
                                            ReservationDateTime = booking.ReservationDateTime, 
                                            Pax = booking.Pax,
                                            IsSameGuest = booking.PrimaryGuestId == booking.GuestId ? true : false,
                                            OriginalReservationDateTime = booking.ReservationDateTime,
                                            OriginalReservationDate = booking.ReservationDate.ToString("yyyy-MM-dd"),
                                            OriginalReservationTime = booking.ReservationTime,
                                            OriginalCheckInDate = booking.CheckInDate.ToString("yyyy-MM-dd"),
                                            OriginalCheckInTime = booking.CheckInTime,
                                            OriginalCheckOutDate = booking.CheckOutDate.ToString("yyyy-MM-dd"),
                                            OriginalCheckOutTime = booking.CheckOutTime,
                                            
                                        } // project the entity to map later
                                    ).ToListAsync();


                foreach(var item in checkInResponse.BookingDetailCheckInDTO)
                {
                    item.BookedRoomRates = roomRates.Where(x => x.BookingId == item.BookingId).ToList();
                    item.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestId == item.GuestId) ?? new GuestDetails();
                }

                //payment details
                checkInResponse.PaymentDetails = await _context.PaymentDetails.Select(x=> new PaymentDetails 
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

                }).Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo).ToListAsync();

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



                return Ok(new { Code =200, Message = "Data fetched successfully" , data = checkInResponse});
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetPendingReservations")]
        public async Task<IActionResult> GetPendingReservations(string status)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            DataSet? dataSet = null;
            string spName = "";
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    if(status == "Pending")
                    {
                        spName = "PendingReservations";
                    }
                    else if(status == "CheckIn")
                    {
                        spName = "CheckedInReservations";
                    }
                    else if(status == "PaymentList")
                    {
                        spName = "paymentList";
                    }
                        using (var command = new SqlCommand(spName, connection))
                        {
                            command.CommandTimeout = 120;
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@companyId", companyId);
                            await connection.OpenAsync();

                            using (var adapter = new SqlDataAdapter(command))
                            {
                                dataSet = new DataSet();
                                adapter.Fill(dataSet);
                            }
                            await connection.CloseAsync();
                        }
                }
                return Ok(new { Code = 200, Message = "Reservation Fetched Successfully", data = dataSet });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("ApproveReservation")]
        public async Task<IActionResult> ApproveReservation(string status, int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
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
                if (booking == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }
                if (status == Constants.Constants.Confirmed)
                {
                    foreach (var item in booking)
                    {
                        item.Status = Constants.Constants.Confirmed;
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

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Code = 200, Message = "Reservation Updated Successfully" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("UpdateRoomDetail")]
        public async Task<IActionResult> UpdateRoomDetail([FromBody] List<BookingDetailCheckInDTO> bookingList, string reservationNo)
        {

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                using var transaction = await _context.Database.BeginTransactionAsync();
                var currentDate = DateTime.Now;
                if (bookingList.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "No data found" });
                }

                foreach (var item in bookingList)
                {
                    //guest details
                    if (item.GuestDetails.GuestId == 0)
                    {
                        Constants.Constants.SetMastersDefault(item.GuestDetails, companyId, userId, currentDate);
                        await _context.GuestDetails.AddAsync(item.GuestDetails);
                        await _context.SaveChangesAsync();
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
                            _context.GuestDetails.Update(guestdetails);
                            await _context.SaveChangesAsync();
                        }
                    }

                    //  booked room rate
                    var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();

                    if(roomRates.Count > 0)
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

                        foreach(var rates in item.BookedRoomRates)
                        {
                            rates.BookingId = bookingDetails.BookingId;
                            rates.RoomId = bookingDetails.RoomId;
                            rates.ReservationNo = reservationNo;
                            Constants.Constants.SetMastersDefault(rates, companyId, userId, currentDate);

                            await _context.BookedRoomRates.AddAsync(rates);
                            await _context.SaveChangesAsync();

                        }

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
                        bookingDetails.UpdatedDate = currentDate;
                        bookingDetails.GuestId = item.GuestDetails.GuestId;
                        _context.BookingDetail.Update(bookingDetails);
                        await _context.SaveChangesAsync();

                        // room avaialability
                        var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId);
                        if(roomAvailability == null)
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
                            await _context.SaveChangesAsync();
                        }

                        foreach (var rates in item.BookedRoomRates)
                        {
                            rates.Id = 0;
                            rates.BookingId = bookingDetails.BookingId;
                            rates.RoomId = bookingDetails.RoomId;
                            rates.ReservationNo = reservationNo;
                            Constants.Constants.SetMastersDefault(rates, companyId, userId, currentDate);

                            await _context.BookedRoomRates.AddAsync(rates);
                            await _context.SaveChangesAsync();

                        }
                    }


                    
                    
                }

                var reservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);
                if(reservationDetails == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 500, Message = "Invalid reservation" });
                }
                else
                {
                    List<BookingDetailDTO> bookings = await _context.BookingDetail
                                                        .Where(x => x.IsActive && x.CompanyId == companyId && x.ReservationNo == reservationNo)
                                                        .Select(x => _mapper.Map<BookingDetailDTO>(x))
                                                        .ToListAsync();

                    (reservationDetails.TotalRoomPayment, reservationDetails.TotalGst, reservationDetails.TotalAmount) = Calculation.CalculateTotalRoomAmount(bookings);

                    reservationDetails.UpdatedDate = currentDate;
                    _context.ReservationDetails.Update(reservationDetails);

                    await _context.SaveChangesAsync();
                }

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
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                var currentDate = DateTime.Now;
                if (paymentDetails.PaymentId > 0) 
                {
                    var payment = await _context.PaymentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId && x.PaymentId == paymentDetails.PaymentId).FirstOrDefaultAsync();
                    if(payment == null)
                    {
                        return Ok(new { Code = 400, Message = "Invalid data" });
                    }
                    else
                    {
                        if(paymentDetails.IsActive == false)
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
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                var currentDate = DateTime.Now;
                if (rooms.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "No rooms found for checkin" });
                }
                
                foreach(var item in rooms)
                {
                    var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive && x.CompanyId == companyId && x.BookingId == item);
                    if (booking == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                    }
                    else
                    {
                        booking.Status = Constants.Constants.CheckIn;
                        booking.UpdatedDate = currentDate;
                        _context.BookingDetail.Update(booking);
                        await _context.SaveChangesAsync();




                        var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId && x.RoomStatus == Constants.Constants.Confirmed);
                        if (roomAvailability == null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = "Room availability not found" });
                        }
                        
                        roomAvailability.RoomStatus = Constants.Constants.CheckIn;
                        roomAvailability.UpdatedDate = currentDate;
                        _context.RoomAvailability.Update(roomAvailability);
                        await _context.SaveChangesAsync();
                    }
                    await transaction.CommitAsync();
                }
                return Ok(new { Code = 200, Message = "Rooms Check-In successfully" });
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetCheckoutBookings")]
        public async Task<IActionResult> GetCheckoutBookings(string reservationNo, string guestId)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();
                var response = new CheckOutResponse();
                response.ReservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);

              
                if (response.ReservationDetails == null)
                {
                    return Ok(new { Code = 500, Message = "Reservation details not found" });
                }

                var getbookingno = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Type == Constants.Constants.DocumentInvoice && x.FinancialYear == financialYear);

                if (getbookingno == null || getbookingno.Suffix == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found.", data = getbookingno });
                }
                response.InvoiceNo = getbookingno.Prefix + getbookingno.Separator + getbookingno.Prefix1 + getbookingno.Separator + getbookingno.Prefix2 + getbookingno.Suffix + getbookingno.Number + getbookingno.LastNumber;

                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.GuestId == response.ReservationDetails.PrimaryGuestId);

                response.BookingDetails = await (from booking in _context.BookingDetail
                                                 join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                                 join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
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
                                                     InvoiceName = response.GuestDetails.GuestName,
                                                     BillTo = "",
                                                     InvoiceNo = response.InvoiceNo,
                                                     InvoiceDate = DateTime.Now,
                                                     TotalAmount = booking.TotalAmount,
                                                     BookedRoomRates =  _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList()
                                                 }).ToListAsync();

               
                if (response.BookingDetails.Count == 0)
                {
                    return Ok(new { Code = 500, Message = "Bookings not found" });
                }

                response.PaymentSummary = await CalculateSummary(response.ReservationDetails, response.BookingDetails);

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
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                var response = new CheckOutResponse();
                if (request.BookingIds.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "Invalid data", data = response });
                }

                var bookings = await  (from booking in _context.BookingDetail
                                             join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
                                             join roomType in _context.RoomCategoryMaster on booking.RoomTypeId equals roomType.Id
                                             where booking.IsActive == true && booking.CompanyId == companyId && request.BookingIds.Contains(booking.BookingId)
                                             orderby booking.TotalAmount
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
                                                 RoomTypeName = roomType.Type,
                                                 RoomNo = rooms.RoomNo,
                                                 InitialCheckOutDate = booking.InitialCheckOutDate,
                                                 InitialCheckOutTime = booking.InitialCheckOutTime,
                                                 InitialCheckOutDateTime = booking.InitialCheckOutDateTime
                                             }).ToListAsync();

                //var bookings = await _context.BookingDetail.Where(x => x.IsActive == true && x.CompanyId == companyId && request.BookingIds.Contains(x.BookingId)).ToListAsync();

                var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && request.BookingIds.Contains(x.BookingId)).ToListAsync();

                foreach (var item in bookings) 
                {
                    item.CheckOutDate = request.EarlyCheckOutDate;
                    item.NoOfNights = Constants.Calculation.CalculateNights(item.ReservationDate, item.CheckOutDate);
                    item.BookingAmount = 0;
                    item.GstAmount = 0;
                    item.TotalBookingAmount = 0;
                    item.TotalAmount = 0;
                    
                    var eachRoomRate = roomRates.Where(x => x.BookingId == item.BookingId && x.BookingDate <= request.EarlyCheckOutDate).OrderBy(x => x.BookingDate).ToList();
                    item.BookedRoomRates = eachRoomRate;

                    foreach (var rate in eachRoomRate)
                    {
                        item.BookingAmount = item.BookingAmount + rate.RoomRate;
                        item.GstAmount = item.GstAmount + rate.GstAmount;
                        item.TotalBookingAmount = item.TotalBookingAmount + rate.TotalRoomRate;
                        
                    }
                    item.TotalAmount = Constants.Calculation.BookingTotalAmount(item);
                }
                PaymentSummary paymentSummary = await CalculateSummary(request.ReservationDetails, bookings);

                response.BookingDetails = bookings;
                response.PaymentSummary = paymentSummary;

                return Ok(new { Code = 200, Message = "Rates fetched successfully", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("UpdateRoomsCheckOut")]
        public async Task<IActionResult> UpdateRoomsCheckOut([FromBody] CheckOutRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();
                var currentTime = DateTime.Now;

                if(request.bookingDetails.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "No rooms selected for checkout" });
                }
                if(request.ReservationNo == "")
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Reservation No not found" });
                }

                //find payments
                var payments = await _context.PaymentDetails.Where(x => x.IsActive == true && x.IsReceived == false && x.PaymentLeft > 0 && x.CompanyId == companyId && x.ReservationNo == request.ReservationNo).ToListAsync();

                foreach(var item in request.bookingDetails)
                {
                    var roomRates = await _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId).ToListAsync();
                    item.BookingAmount = 0;
                    item.GstAmount = 0;
                    item.TotalBookingAmount = 0;
                    item.TotalAmount = 0;


                    foreach (var rate in roomRates)
                    {
                        if (rate.BookingDate > item.CheckOutDate)
                        {
                            rate.IsActive = false;
                            rate.UpdatedDate = currentTime;
                            _context.BookedRoomRates.Update(rate);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            item.BookingAmount = item.BookingAmount + rate.RoomRate;
                            item.GstAmount = item.GstAmount + rate.GstAmount;
                            item.TotalBookingAmount = item.TotalBookingAmount + rate.TotalRoomRate;
                        }
                        item.TotalAmount = Constants.Calculation.BookingTotalAmount(item);

                    }
                }

                CalculateInvoice(request.bookingDetails, payments);
                foreach (var item in request.bookingDetails)
                {
                    var booking = await _context.BookingDetail.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == item.BookingId && x.Status == Constants.Constants.CheckIn);
                    if(booking == null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 400, Message = "Booking not found" });
                    }


                    

                    var balance = CalculateBalanceBooking(item);

                    booking.CheckOutDate = item.CheckOutDate;
                    booking.CheckOutTime = item.CheckOutTime;
                    booking.CheckOutDateTime = Constants.Calculation.ConvertToDateTime(booking.CheckOutDate, booking.CheckOutTime);
                    booking.NoOfNights = Constants.Calculation.CalculateNights(booking.ReservationDate, booking.CheckOutDate);
                    booking.Status = Constants.Constants.CheckOut;
                    booking.UpdatedDate = currentTime;
                    booking.InitialBalanceAmount = balance;
                    booking.BalanceAmount = balance;
                    booking.AdvanceAmount = item.AdvanceAmount;
                    booking.AgentAdvanceAmount = item.AgentAdvanceAmount;
                    booking.ReceivedAmount = item.ReceivedAmount;
                    booking.InvoiceNo = item.InvoiceNo;
                    booking.InvoiceDate = item.InvoiceDate;
                    booking.InvoiceName = item.InvoiceName;
                    booking.BookingAmount = item.BookingAmount;
                    booking.GstAmount = item.GstAmount;
                    booking.TotalBookingAmount = item.TotalBookingAmount;
                    booking.TotalAmount = item.TotalAmount;
                    booking.BillTo = item.BillTo;
                    
                    _context.BookingDetail.Update(booking);
                    await _context.SaveChangesAsync();

                   
                        

                    var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId && x.RoomStatus == Constants.Constants.CheckIn);
                    if(roomAvailability == null)
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
                    await _context.SaveChangesAsync();
                }

                

                foreach (var pay in payments)
                {
                    pay.UpdatedDate = currentTime;
                    _context.PaymentDetails.Update(pay);

                    foreach(var invoice in pay.InvoiceHistories)
                    {
                        Constants.Constants.SetMastersDefault(invoice, companyId, userId, currentTime);
                        await _context.InvoiceHistory.AddAsync(invoice);
                        await _context.SaveChangesAsync();
                    }
                }

                string result = await UpdateDocumentNo(Constants.Constants.DocumentInvoice);
                if(result == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, Message = "Error while updating document" });
                }

                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Bookings Checkout successfully" });
            }
            catch(Exception ex)
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
                var response = new CheckInRoomData();
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
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

                if(response.BookingDetail == null)
                {
                    return Ok(new { Code = 400, Message = "No booking found" });
                }
                return Ok(new { Code = 200, Message = "Data fetched", data = response });
                
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        private async Task<PaymentSummary> CalculateSummary(ReservationDetails reservationDetails, List<BookingDetail> bookings)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            var summary = new PaymentSummary();
            

            foreach (var item in bookings)
            {
                summary.TotalRoomAmount = summary.TotalRoomAmount + item.BookingAmount;
                summary.TotalGstAmount = summary.TotalGstAmount + item.GstAmount;
                summary.TotalAmount = summary.TotalAmount + item.TotalBookingAmount;
            }
            summary.AgentServiceCharge = reservationDetails.AgentServiceCharge;
            summary.AgentServiceGst = reservationDetails.AgentServiceGstAmount;
            summary.AgentServiceTotal = reservationDetails.AgentTotalServiceCharge;

            summary.TotalPayable = summary.TotalAmount + summary.AgentServiceTotal;

            var payments = await _context.PaymentDetails.Where(x => x.IsActive == true && x.IsReceived == false && x.CompanyId == companyId && x.ReservationNo == reservationDetails.ReservationNo).OrderBy(x=>x.PaymentId).ToListAsync();

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
                        if (bookings.Select(x=>x.BookingId).Contains(pay.PaymentId))
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

            //calculate invoice    

            return summary;
        }

        private void CalculateInvoice(List<BookingDetail> bookings, List<PaymentDetails> payments)
        {
            //set room payment if room wise payment
            foreach (var booking in bookings)
            {
                foreach (var pay in payments)
                {
                    if (pay.RoomId == booking.RoomId && pay.BookingId == booking.BookingId )
                    {
                        pay.IsReceived = true;
                        pay.PaymentLeft = 0;
                        decimal balance = CalculateBalanceBooking(booking);
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
            (agentAdvance, advanceAmount, receivedAmount) = CalculatePayment(payments);

            //agent advance allocation
            if (agentAdvance > 0)
            {
                int roomCounts = GetBalanceRoomCount(bookings);
                List<decimal> equallyDivideArr = EquallyDivideAmount(agentAdvance, roomCounts);
                int divideArrIndex = 0;
               

                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateBalanceBooking(bookings[i]);
                    if(balance > 0)
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
                        while (paymentIndex != payments.Count && currentBalance!=0)
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
                int roomCounts = GetBalanceRoomCount(bookings);
                List<decimal> equallyDivideArr = EquallyDivideAmount(advanceAmount, roomCounts);
                int divideArrIndex = 0;
                

                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateBalanceBooking(bookings[i]);
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
                int roomCounts = GetBalanceRoomCount(bookings);
                List<decimal> equallyDivideArr = EquallyDivideAmount(receivedAmount, roomCounts);
                int divideArrIndex = 0;
               

                for (int i = 0; i < bookings.Count; i++)
                {
                    int paymentIndex = 0;
                    decimal balance = CalculateBalanceBooking(bookings[i]);
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
        }

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
            return (payments[paymentIndex].PaymentStatus != Constants.Constants.AdvancePayment && payments[paymentIndex].PaymentStatus != Constants.Constants.AgentPayment && payments[paymentIndex].IsReceived == false && payments[paymentIndex].PaymentLeft > 0);
        }
       private int GetBalanceRoomCount(List<BookingDetail> bookings)
        {
            int count = 0;
            foreach(var item in bookings)
            {
                decimal balance = CalculateBalanceBooking(item);
                if(balance > 0)
                {
                    count++;
                }
            }
            return count;
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
            decimal balanceDiv = Constants.Calculation.RoundOffDecimal(totalDivide - amount);
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

        private decimal CalculateBalanceBooking(BookingDetail booking)
        {
            return (booking.TotalAmount - (booking.AgentAdvanceAmount + booking.AdvanceAmount + booking.ReceivedAmount));
        }

        private (decimal agentAdvance, decimal advance, decimal adjustedAmount) CalculatePayment(List<PaymentDetails> paymentDetails)
        {
            decimal agentAdvance = 0;
            decimal advance = 0;
            decimal received = 0;
            foreach(var pay in paymentDetails)
            {
                if(pay.PaymentStatus == Constants.Constants.AdvancePayment && pay.IsReceived == false)
                {
                    advance += pay.PaymentLeft;
                }
                else if(pay.PaymentStatus == Constants.Constants.ReceivedPayment && pay.IsReceived == false)
                {
                    received += pay.PaymentLeft;
                }
                else if(pay.PaymentStatus == Constants.Constants.AgentPayment && pay.IsReceived == false)
                {
                    agentAdvance += pay.PaymentLeft;
                }
            }
            return (agentAdvance,advance, received);
        }

        [HttpGet("GetPayments")]
        public async Task<IActionResult> GetPayments(int roomId, string reservationNo, string filter, int guestId)
        {
            try
            {
                var response = new AddPaymentResponse();
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();
                response.ReservationDetails = await _context.ReservationDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo);


                if (response.ReservationDetails == null)
                {
                    return Ok(new { Code = 500, Message = "Reservation details not found" });
                }

                var statusList = new List<string> { Constants.Constants.Pending, Constants.Constants.Confirmed, Constants.Constants.CheckIn};
                response.GuestDetails = await _context.GuestDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.GuestId == guestId);

                response.BookingDetails = await (from booking in _context.BookingDetail
                                                 join rooms in _context.RoomMaster on booking.RoomId equals rooms.RoomId
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
                                                     InvoiceDate = DateTime.Now,
                                                     TotalAmount = booking.TotalAmount,
                                                     BookedRoomRates = _context.BookedRoomRates.Where(x => x.IsActive == true && x.CompanyId == companyId && x.BookingId == booking.BookingId).ToList()
                                                 }).ToListAsync();

                response.PaymentDetails = await _context.PaymentDetails.Select(x => new PaymentDetails
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
                    PaymentLeft = x.PaymentLeft
                }).Where(x => x.IsActive == true && x.CompanyId == companyId && x.ReservationNo == reservationNo).ToListAsync();
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
    }
}
