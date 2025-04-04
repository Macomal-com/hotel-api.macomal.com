using AutoMapper;
using hotel_api.Constants;
using Microsoft.AspNetCore.Http;
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

        private static void SetMastersDefault(ICommonProperties model, int companyid, int userId, DateTime currentDate)
        {
            model.CreatedDate = currentDate;
            model.UpdatedDate = currentDate;
            model.IsActive = true;
            model.CompanyId = companyid;
            model.UserId = userId;
        }

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

                var getbookingno = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Type == type && x.FinancialYear == financialYear);
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
                    }
                }
                else if(checkOutFormat == Constants.Constants.Hour24Format || checkOutFormat == Constants.Constants.NightFormat)
                {

                    DateTime currentDate = checkInDate;
                    while (noOfNights > 0)
                    {
                        var customRoomRates = await _context.RoomRateDateWise.Where(x => x.IsActive == true && x.CompanyId == companyId && (x.FromDate >= currentDate && x.ToDate <= checkOutDate)).OrderByDescending(x => x.RatePriority).FirstOrDefaultAsync();
                        if(customRoomRates == null)
                        {
                            var roomRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomTypeId).FirstOrDefaultAsync();
                            if (roomRates == null)
                            {
                                return Ok(new { Code = 200, Message = "No Room Rates found" });
                            }
                            else
                            {
                                roomRateResponse.BookedRoomRates.Add(new BookedRoomRate
                                {
                                    RoomRate = Calculation.RoundOffDecimal(roomRates.RoomRate),
                                    BookingDate = currentDate
                                });

                                roomRateResponse.BookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + roomRates.RoomRate);
                            }
                        }
                        else
                        {
                            roomRateResponse.BookedRoomRates.Add(new BookedRoomRate
                            {
                                RoomRate = Calculation.RoundOffDecimal(customRoomRates.RoomRate),
                                BookingDate = currentDate
                            });
                            roomRateResponse.BookingAmount = Calculation.RoundOffDecimal(roomRateResponse.BookingAmount + customRoomRates.RoomRate);
                        }
                        noOfNights--;
                        currentDate = currentDate.AddDays(1);
                    }
                }
                
                //calculate gst
                if (gstPercentage.GstType == Constants.Constants.MultipleGst)
                {
                    var gstRangeMaster = GetApplicableGstRange(gstPercentage.ranges, roomRateResponse.AllRoomAmount);
                    if (gstRangeMaster == null)
                    {
                        roomRateResponse.GstPercentage = gstPercentage.TaxPercentage;
                        (roomRateResponse.BookingAmount, roomRateResponse.GstAmount) = Calculation.CalculateGst(roomRateResponse.BookingAmount, roomRateResponse.GstPercentage, gstType);
                    }
                    else
                    {
                        roomRateResponse.GstPercentage = gstRangeMaster.TaxPercentage;
                        (roomRateResponse.BookingAmount, roomRateResponse.GstAmount) = Calculation.CalculateGst(roomRateResponse.BookingAmount, roomRateResponse.GstPercentage, gstType);
                    }
                }
                else if (gstPercentage.GstType == Constants.Constants.SingleGst)
                {
                    roomRateResponse.GstPercentage = gstPercentage.TaxPercentage;
                    (roomRateResponse.BookingAmount, roomRateResponse.GstAmount) = Calculation.CalculateGst(roomRateResponse.BookingAmount, roomRateResponse.GstPercentage, gstType);
                }

                //total amount
                roomRateResponse.AllRoomAmount = Calculation.RoundOffDecimal(noOfRooms * roomRateResponse.BookingAmount);
                roomRateResponse.AllRoomGst = Calculation.RoundOffDecimal(noOfRooms * roomRateResponse.GstAmount);
                roomRateResponse.TotalAmount = Calculation.RoundOffDecimal(roomRateResponse.AllRoomAmount + roomRateResponse.GstAmount);
                
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


        [HttpPost]
        public async Task<IActionResult> SaveReservation([FromForm] ReservationRequest request, IFormFile? GuestImage, string reservationNo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var currentDate = DateTime.Now;
            try
            {
                if (request == null || request.GuestDetails == null || request.BookingDetails == null || request.BookingDetails.Count == 0 || request.PaymentDetails == null)
                {
                    return Ok(new { code = 400, message = "Invalid data.", data = new object { } });
                }
                if (string.IsNullOrWhiteSpace(request.GuestDetails.GuestName) || string.IsNullOrWhiteSpace(request.GuestDetails.PhoneNumber))
                {
                    return Ok(new { code = 400, message = "Guest name and phone number is required.", data = new object { } });

                }

                //CHECK ROOM ALREADY BOOKING OR NOT
                foreach (var item in request.BookingDetails)
                {
                    //room not assigned
                    if(item.RoomsList.Count == 0)
                    {
                        DataSet dataSet = await GetRoomAvailability(item.CheckInDate, item.CheckInTime, item.CheckOutDate, item.CheckOutTime, "cleanroom", item.RoomTypeId, 0);
                        if(dataSet == null)
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

                            if(AvailableRooms < item.NoOfRooms)
                            {
                                return Ok(new { Code = 400, Message = "Required No of rooms are not available for " + item.RoomCategoryName });
                            }                          
                        }
                    }
                    else
                    {
                        foreach(var room in item.RoomsList)
                        {
                            DataSet dataSet = await GetRoomAvailability(item.CheckInDate, item.CheckInTime, item.CheckOutDate, item.CheckOutTime, "checkbyroomid", item.RoomTypeId, item.RoomId);
                            if (dataSet == null)
                            {
                                await transaction.RollbackAsync();
                                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                            }
                            else
                            {
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
                                    if (rows[0]["RoomStatus"].ToString() != Constants.Constants.Clean)
                                    {
                                        return Ok(new { code = 400, message = "Room " + rows[0]["RoomNo"] + " is already reserved with Reservation No " + rows[0]["ReservationNo"], data = new object { } });
                                    }
                                }

                            }
                        }
                    }



                }


                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);


                //Insert Guest Details
                var guest = await _context.GuestDetails.FirstOrDefaultAsync(x => x.GuestID == request.GuestDetails.GuestID);
                
                if (guest == null)
                {
                    SetMastersDefault(request.GuestDetails, companyId, userId, currentDate);
                    if (GuestImage != null)
                    {
                        request.GuestDetails.GuestImage = await Constants.Constants.AddFile(GuestImage);
                    }
                    await _context.GuestDetails.AddAsync(request.GuestDetails);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    guest.GuestName = request.GuestDetails.GuestName;
                    guest.Nationality = request.GuestDetails.Nationality;
                    guest.StateName = request.GuestDetails.StateName;
                    guest.Address = request.GuestDetails.Address;
                    guest.City = request.GuestDetails.City;
                    guest.PhoneNumber = request.GuestDetails.PhoneNumber;
                    guest.Email = request.GuestDetails.Email;
                    if (GuestImage != null)
                    {
                        guest.GuestImage = await Constants.Constants.AddFile(GuestImage);
                    }
                    guest.Other1 = request.GuestDetails.Other1;
                    guest.Other2 = request.GuestDetails.Other2;
                    guest.UpdatedDate = currentDate;
                    _context.GuestDetails.Update(guest);
                    await _context.SaveChangesAsync();
                }

                //save booking
                int roomCount = 1;
                foreach(var item in request.BookingDetails)
                {
                    //not assigned room
                    if(item.RoomsList.Count == 0)
                    {
                        for(int i = 1; i <= item.NoOfRooms; i++)
                        {
                            var bookingDetail = item;
                            bookingDetail.RoomId = 0;
                            bookingDetail.RoomCount = roomCount;
                            bookingDetail.PrimaryGuestId = request.GuestDetails.GuestID;
                            bookingDetail.GuestId = request.GuestDetails.GuestID;
                            bookingDetail.CheckInDateTime = DateTime.ParseExact(bookingDetail.CheckInDate + " " + bookingDetail.CheckInTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            bookingDetail.CheckOutDateTime = DateTime.ParseExact(bookingDetail.CheckOutDate + " " + bookingDetail.CheckOutTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            SetMastersDefault(bookingDetail, companyId, userId, currentDate);
                            await _context.BookingDetail.AddAsync(bookingDetail);
                            await _context.SaveChangesAsync();
                            roomCount++;

                            RoomAvailability roomAvailability = new RoomAvailability();
                            SetMastersDefault(roomAvailability, companyId, userId, currentDate);
                            roomAvailability.CheckInDate = bookingDetail.CheckInDate;
                            roomAvailability.CheckOutDate = bookingDetail.CheckOutDate;
                            roomAvailability.CheckInTime = bookingDetail.CheckInTime;
                            roomAvailability.CheckOutTime = bookingDetail.CheckOutTime;
                            roomAvailability.CheckInDateTime = bookingDetail.CheckInDateTime;
                            roomAvailability.CheckOutDateTime = bookingDetail.CheckOutDateTime;
                            roomAvailability.ReservationNo = bookingDetail.ReservationNo;
                            roomAvailability.BookingId = bookingDetail.BookingId;
                            roomAvailability.RoomId = bookingDetail.RoomId;
                            roomAvailability.RoomStatus = bookingDetail.Status;
                            roomAvailability.RoomTypeId = bookingDetail.RoomTypeId;
                            await _context.RoomAvailability.AddAsync(roomAvailability);
                            await _context.SaveChangesAsync();


                        }
                    }
                    else
                    {
                        foreach(var room in item.RoomsList)
                        {
                            var bookingDetail = item;
                            bookingDetail.RoomId = room.RoomId;
                            bookingDetail.RoomCount = roomCount;
                            bookingDetail.PrimaryGuestId = request.GuestDetails.GuestID;
                            bookingDetail.GuestId = request.GuestDetails.GuestID;
                            SetMastersDefault(bookingDetail, companyId, userId, currentDate);
                            await _context.BookingDetail.AddAsync(bookingDetail);
                            await _context.SaveChangesAsync();

                            RoomAvailability roomAvailability = new RoomAvailability();
                            SetMastersDefault(roomAvailability, companyId, userId, currentDate);
                            roomAvailability.CheckInDate = bookingDetail.CheckInDate;
                            roomAvailability.CheckOutDate = bookingDetail.CheckOutDate;
                            roomAvailability.CheckInTime = bookingDetail.CheckInTime;
                            roomAvailability.CheckOutTime = bookingDetail.CheckOutTime;
                            roomAvailability.CheckInDateTime = bookingDetail.CheckInDateTime;
                            roomAvailability.CheckOutDateTime = bookingDetail.CheckOutDateTime;
                            roomAvailability.ReservationNo = bookingDetail.ReservationNo;
                            roomAvailability.BookingId = bookingDetail.BookingId;
                            roomAvailability.RoomId = bookingDetail.RoomId;
                            roomAvailability.RoomStatus = bookingDetail.Status;
                            roomAvailability.RoomTypeId = bookingDetail.RoomTypeId;
                            await _context.RoomAvailability.AddAsync(roomAvailability);
                            await _context.SaveChangesAsync();

                            roomCount++;
                        }
                    }
                }
                
                //payment
                if(request.PaymentDetails.PaymentAmount > 0)
                {
                    SetMastersDefault(request.PaymentDetails, companyId, userId, currentDate);
                    request.PaymentDetails.BookingId = 0;
                    request.PaymentDetails.ReservationNo = reservationNo;
                    request.PaymentDetails.PaymentFormat = Constants.Constants.ReservationWisePayment;       
                    await _context.PaymentDetails.AddAsync(request.PaymentDetails);
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
            catch (Exception)
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

    }
}
