using AutoMapper;
using hotel_api.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Repository.DTO;
using Repository.Models;
using Repository.RequestDTO;
using RepositoryModels.Repository;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace hotel_api.Controllers
{

    

    [Route("api/[controller]")]
    [ApiController]
    public class HouseKeepingController : ControllerBase
    {
        private readonly DbContextSql _context;
        private readonly IMapper _mapper;
        private int companyId;
        private string financialYear = string.Empty;
        private int userId;
        public HouseKeepingController(DbContextSql contextSql, IMapper map, IHttpContextAccessor httpContextAccessor)
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

        [HttpGet("GetHouseKeepingFormData")]
        public async Task<IActionResult> GetHouseKeepingFormData()
        {
            try
            {
                HousekeepingFormResponse response = new HousekeepingFormResponse();

                response.AllServicesStatus = new List<string>
                {
                    "Clean",
                    "Dirty",
                    "Under Repair",
                    "Management Block",
                    "DND",
                    "CheckIn"
                };

                response.ServicesStatus = new List<object>
                {
                    new { Status = "Clean", Colour = "#e49273" , IsEnable = true},
                    new { Status = "Dirty", Colour = "#8C8C8C", IsEnable = true},
                    new { Status = "Under Repair", Colour = "#FFD393", IsEnable = true},
                    new { Status = "Management Block", Colour = "#ecd230", IsEnable = true},
                    new { Status = "DND", Colour = "#7d2525", IsEnable = true},
                    new { Status = "Roll Back", Colour = "#ef5454", IsEnable = false},
                };

                
                response.RoomMaster = await _context.RoomMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync();

                response.RoomCategoryMaster = await _context.RoomCategoryMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync();

                response.Staff = await _context.StaffManagementMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync();

                return Ok(new { Code = 200, Message = "Data fetch successfully", data = response });


            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetRoomsForHouseKeeping")]
        public async Task<IActionResult> GetRoomsForHouseKeeping(DateTime startDate, DateTime endDate,int roomId, int roomTypeId, string? roomStatus = "")
        {
            try
            {
                
                
                DataSet dataSet = null;
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("housekeeping_availability", connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@userCheckInDateTime", startDate);
                        command.Parameters.AddWithValue("@userCheckOutDateTime", endDate);                        
                        command.Parameters.AddWithValue("@companyId", companyId);
                        command.Parameters.AddWithValue("@roomStatus", roomStatus);
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

                return Ok(new { Code = 200, Message = "Data fetched", data = rows });
            }
            catch (Exception ex)
            {
                
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("SaveHouseKeeping")]
        public async Task<IActionResult> SaveHouseKeeping([FromBody] HousekeepingRequestDTO housekeepingRequest)
        {

            var CleanAllowedStatus = new List<string> { "Dirty", "Under Repair", "Management Block", "Other" };
            var DirtyAllowedStatus = new List<string> { "Clean" };
            var CheckInAllowedStatus = new List<string> { "DND" };
            var AllowedStatus = new List<string> { "Roll Back" };
            var currentDate = DateTime.Now;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //check valid status
                foreach(var item in housekeepingRequest.HouseKeepingRooms)
                {
                    HouseKeeping houseKeeping = new HouseKeeping();
                    Constants.Constants.SetMastersDefault(houseKeeping, companyId, userId, currentDate);
                    if (item.RoomStatus == "Clean")
                    {
                        if (!CleanAllowedStatus.Contains(housekeepingRequest.ServiceStatus))
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = $"No service should be performed for status {housekeepingRequest.ServiceStatus} and room no {item.RoomNo}." });
                        }
                    }
                    else if(item.RoomStatus == "Dirty")
                    {
                        if (!DirtyAllowedStatus.Contains(housekeepingRequest.ServiceStatus))
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = $"No service should be performed for status {housekeepingRequest.ServiceStatus} and room no {item.RoomNo}." });
                        }
                    }
                    else if (item.RoomStatus == "CheckIn")
                    {
                        if (!CheckInAllowedStatus.Contains(housekeepingRequest.ServiceStatus))
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = $"No service should be performed for status {housekeepingRequest.ServiceStatus} and room no {item.RoomNo}." });
                        }
                    }
                    else
                    {
                        if (!AllowedStatus.Contains(housekeepingRequest.ServiceStatus))
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 400, Message = $"No service should be performed for status {housekeepingRequest.ServiceStatus} and room no {item.RoomNo}." });
                        }
                    }

                    if(item.RoomAvailaibilityId > 0)
                    {
                        var roomAvailability = await _context.RoomAvailability.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == item.RoomAvailaibilityId);
                        if(roomAvailability == null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 500, Message = "Room availability not found" });
                        }
                        else
                        {
                            if(housekeepingRequest.ServiceStatus == "Clean")
                            {
                                //room status = dirty, under repair, management block
                                _context.RoomAvailability.Remove(roomAvailability);
                            }
                            else if(housekeepingRequest.ServiceStatus == "Dirty")
                            {
                                //roomstatus = clean
                                roomAvailability.RoomStatus = "Dirty";
                            }
                            else if(houseKeeping.ServiceStatus == "Roll Back")
                            {
                                roomAvailability.ServiceStatus = "";                                
                            }
                            else if(houseKeeping.ServiceStatus == "DND")
                            {
                                roomAvailability.ServiceStatus = houseKeeping.ServiceStatus;
                            }
                            else
                            {
                                roomAvailability.ServiceStatus = houseKeeping.ServiceStatus;
                            }
                            
                        }

                       


                    }
                    else
                    {
                        RoomAvailability room = new RoomAvailability();
                        Constants.Constants.SetMastersDefault(room, companyId, userId, currentDate);
                        (room.CheckInDate, room.CheckInTime) = DateTimeMethod.GetDateTime(housekeepingRequest.StartDate);
                        (room.CheckOutDate, room.CheckOutTime) = DateTimeMethod.GetDateTime(housekeepingRequest.EndDate);
                        room.CheckInDateTime = housekeepingRequest.StartDate;
                        room.CheckOutDateTime = housekeepingRequest.EndDate;
                        room.RoomId = item.RoomId;
                        room.RoomTypeId = item.RoomTypeId;
                        room.RoomStatus = housekeepingRequest.ServiceStatus;

                        await _context.RoomAvailability.AddAsync(room);
                    }

                    houseKeeping.RoomTypeId = item.RoomTypeId;
                    houseKeeping.RoomId = item.RoomId;
                    houseKeeping.RoomStatus = item.RoomStatus;
                    houseKeeping.ServiceStatus = housekeepingRequest.ServiceStatus;
                    (houseKeeping.ServiceDate, houseKeeping.ServiceTime) = DateTimeMethod.GetDateOnlyAndTime(currentDate);
                    houseKeeping.ServiceDateTime = currentDate;
                    houseKeeping.ServiceBy = housekeepingRequest.ServiceBy;
                    houseKeeping.Remarks = housekeepingRequest.Remarks;

                    await _context.HouseKeeping.AddAsync(houseKeeping);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Housekeeping updated successfully" });
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();

                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }
    }
}
