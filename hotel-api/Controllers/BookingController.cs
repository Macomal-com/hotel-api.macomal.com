using AutoMapper;
using hotel_api.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.DTO;
using Repository.Models;
using RepositoryModels.Repository;
using System.ComponentModel.Design;
using System.Data;
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
        public async Task<IActionResult> CheckRoomAvailaibility(string checkInDate, string checkInTime, string checkOutDate, string checkOutTime, string pageName = "")
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
                    var dataSet = new DataSet();
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
                            await connection.OpenAsync();

                            using (var adapter = new SqlDataAdapter(command))
                            {
                                adapter.Fill(dataSet);
                            }
                            await connection.CloseAsync();
                        }
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
                var bookingno = getbookingno.Prefix + "/" + getbookingno.Prefix1 + "/" + getbookingno.Prefix2 + getbookingno.Suffix + getbookingno.Number + getbookingno.LastNumber;

                var agentDetails = await _context.AgentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId).Select(x=> new
                {
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
        public async Task<IActionResult> ReservationRoomRate(int roomTypeId, DateTime checkInDate, DateTime checkOutDate,  string checkOutFormat, int noOfRooms, int noOfNights, string applicableService,string gstType, int hourId = 0)
        {
            try
            {
                var roomRateResponse = new RoomRateResponse();
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                if (roomTypeId == 0 || checkOutFormat == "")
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }

                var gstPercentage = await GetGstPercetage(applicableService);
                if (gstPercentage == null)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
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
                        roomRateResponse.BookingAmount = roomRates.RoomRate;
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
                                    RoomRate = roomRates.RoomRate,
                                    BookingDate = currentDate
                                });

                                roomRateResponse.BookingAmount = roomRateResponse.BookingAmount + roomRates.RoomRate;
                            }
                        }
                        else
                        {
                            roomRateResponse.BookedRoomRates.Add(new BookedRoomRate
                            {
                                RoomRate = customRoomRates.RoomRate,
                                BookingDate = currentDate
                            });
                            roomRateResponse.BookingAmount = roomRateResponse.BookingAmount + customRoomRates.RoomRate;
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
                roomRateResponse.AllRoomAmount = noOfRooms * roomRateResponse.BookingAmount;
                roomRateResponse.AllRoomGst = noOfRooms * roomRateResponse.GstAmount;
                roomRateResponse.TotalRoomAmount = roomRateResponse.AllRoomAmount + roomRateResponse.GstAmount;
                //calculate agent rates
                //if (agentId > 0)
                //{
                //    var agentDetails = await _context.AgentDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.AgentId == agentId);
                //    if (agentDetails == null)
                //    {
                //        return Ok(new { Code = 500, Message = "No agent found" });
                //    }
                //    else
                //    {
                //        CalculateAgentCommision(agentDetails, ref roomRateResponse);
                //    }
                //}
                return Ok(new { Code = 500, Message = "Room rate fetched successfully", data = roomRateResponse });

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

        public GstRangeMaster? GetApplicableGstRange(List<GstRangeMaster> gstRanges, decimal bookingAmount)
        {
            return gstRanges.FirstOrDefault(range => bookingAmount >= range.RangeStart && bookingAmount <= range.RangeEnd);
        }

        public void CalculateAgentCommision(AgentDetails agentDetails, ref RoomRateResponse roomRateResponse)
        {
            roomRateResponse.AgentGstType = agentDetails.GstType;
            if (agentDetails.GstType == Constants.Constants.WithGst)
            {
                //commission
                roomRateResponse.AgentCommissionPercentage = agentDetails.Commission;
                roomRateResponse.AgentCommisionAmount = Calculation.CalculateGst(roomRateResponse.TotalRoomAmount, roomRateResponse.AgentCommissionPercentage);
                //tcs
                roomRateResponse.TcsPercentage = agentDetails.Tcs;
                roomRateResponse.TcsAmount = Calculation.CalculateGst(roomRateResponse.TotalRoomAmount, roomRateResponse.TcsPercentage);
                //tds
                roomRateResponse.TdsPercentage = agentDetails.Tds;
                roomRateResponse.TdsAmount = Calculation.CalculateGst(roomRateResponse.TotalRoomAmount, roomRateResponse.TdsPercentage);
            }
            else
            {
                //commission
                roomRateResponse.AgentCommissionPercentage = agentDetails.Commission;
                roomRateResponse.AgentCommisionAmount = Calculation.CalculateGst(roomRateResponse.AllRoomAmount, roomRateResponse.AgentCommissionPercentage);
                //tcs
                roomRateResponse.TcsPercentage = agentDetails.Tcs;
                roomRateResponse.TcsAmount = Calculation.CalculateGst(roomRateResponse.AllRoomAmount, roomRateResponse.TcsPercentage);
                //tds
                roomRateResponse.TdsPercentage = agentDetails.Tds;
                roomRateResponse.TdsAmount = Calculation.CalculateGst(roomRateResponse.AllRoomAmount, roomRateResponse.TdsPercentage);
            }
        }
        


    }
}
