using AutoMapper;
using FluentValidation;
using hotel_api.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Repository.Models;
using RepositoryModels.Repository;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace hotel_api.Controllers
{
    [Route("api/Masters")]
    [ApiController]
    public class NewController(DbContextSql context, IMapper mapper) : ControllerBase
    {
        private readonly DbContextSql _context = context;
        private readonly IMapper _mapper = mapper;
        private static void SetMastersDefault(ICommonProperties model, int companyid, int userId)
        {
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            model.IsActive = true;
            model.CompanyId = companyid;
            model.UserId = userId;
        }

        //room category
        [HttpGet("GetRoomCategoryMaster")]
        public async Task<IActionResult> GetRoomCategoryMaster()
        {
            try
            {
                var data = await (from cat in _context.RoomCategoryMaster
                                  join bed in _context.BedTypeMaster on cat.BedTypeId equals bed.BedTypeId into bedcat
                                  from o in bedcat.DefaultIfEmpty()
                                  where cat.IsActive == true 
                                  select new
                                  {
                                      Id = cat.Id,
                                      Type = cat.Type,
                                      Description = cat.Description,
                                      MinPax = cat.MinPax,
                                      MaxPax = cat.MaxPax,
                                      DefaultPax = cat.DefaultPax,
                                      BedTypeId = cat.BedTypeId,
                                      BedType = o.BedType,
                                      ExtraBed = cat.ExtraBed,
                                      NoOfRooms = cat.NoOfRooms,
                                      PlanDetails = cat.PlanDetails
                                  }).ToListAsync();



                return Ok(new { Code = 200, Message = "Room Category fetched successfully", Data = data });
            }

            catch (Exception ex)
            {

                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetRoomCategoryById/{id}")]
        public async Task<IActionResult> GetRoomCategoryById(int id)
        {
            try
            {
                var data = await (from cat in _context.RoomCategoryMaster
                                  join bed in _context.BedTypeMaster on cat.BedTypeId equals bed.BedTypeId into bedcat
                                  from o in bedcat.DefaultIfEmpty()
                                  where cat.IsActive == true && cat.Id == id
                                  select new
                                  {
                                      Id = cat.Id,
                                      Type = cat.Type,
                                      Description = cat.Description,
                                      MinPax = cat.MinPax,
                                      MaxPax = cat.MaxPax,
                                      DefaultPax = cat.DefaultPax,
                                      BedTypeId = cat.BedTypeId,
                                      BedType= o.BedType,
                                      ExtraBed = cat.ExtraBed,
                                      NoOfRooms = cat.NoOfRooms,
                                      PlanDetails = cat.PlanDetails
                                  }).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Room Category not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Room Category fetched successfully", Data = data });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddRoomCategoryMaster")]
        public async Task<IActionResult> AddRoomCategoryMaster([FromBody] RoomCategoryMasterDTO roomCat)
        {
            if (roomCat == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<RoomCategoryMaster>(roomCat);
                var validator = new RoomCategoryValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, Message = errors });
                }

                SetMastersDefault(cm, companyId, userId);

                cm.BedTypeId = cm.BedTypeId == 0 ? null : cm.BedTypeId;

                await _context.RoomCategoryMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room Category created successfully" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPatch("PatchRoomCategoryMaster/{id}")]
        public async Task<IActionResult> PatchRoomCategoryMaster(int id, [FromBody] JsonPatchDocument<RoomCategoryMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var roomCat = await _context.RoomCategoryMaster.FindAsync(id);

                if (roomCat == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(roomCat, ModelState);
                if(roomCat.IsActive == true)
                {
                    var validator = new RoomCategoryValidator(_context);
                    var result = await validator.ValidateAsync(roomCat);
                    if (!result.IsValid)
                    {
                        var errors = result.Errors.Select(x => new
                        {
                            Error = x.ErrorMessage,
                            Field = x.PropertyName
                        }).ToList();
                        return Ok(new { Code = 202, Message = errors });
                    }
                    roomCat.BedTypeId = roomCat.BedTypeId == 0 ? null : roomCat.BedTypeId;
                }
               
                roomCat.UpdatedDate = DateTime.Now;
                if (!ModelState.IsValid)
                {
                    var errorMessages = ModelState
                                        .Where(x => x.Value.Errors.Any())
                                        .SelectMany(x => x.Value.Errors)
                                        .Select(x => x.ErrorMessage)
                                        .ToList();
                    return Ok(new { Code = 500, Message = errorMessages });
                }

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room Category updated successfully" });
            }
            catch(Exception ex)
            {

                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
           
        }


        [HttpGet("GetRoomMaster")]
        public async Task<IActionResult> GetRoomMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await (from roommaster in _context.RoomMaster
                                  join floor in _context.FloorMaster on roommaster.FloorId equals floor.FloorId into floorRoom
                                  from fr in floorRoom.DefaultIfEmpty()
                                  join building in _context.BuildingMaster on roommaster.BuildingId equals building.BuildingId into buildingRoom
                                  from fb in buildingRoom.DefaultIfEmpty()
                                  join property in _context.CompanyDetails on roommaster.PropertyId equals property.PropertyId
                                  join roomtype in _context.RoomCategoryMaster on roommaster.RoomTypeId equals roomtype.Id
                                  where roommaster.IsActive == true && roommaster.PropertyId == companyId && fr.IsActive == true && fb.IsActive == true && property.IsActive == true && roomtype.IsActive == true
                                  select new
                                  {
                                      RoomId = roommaster.RoomId,
                                      //roommaster.FloorId,
                                      fr.FloorNumber,
                                     // roommaster.BuildingId,
                                      fb.BuildingName,
                                     // roommaster.PropertyId,
                                      property.CompanyName,
                                      //roommaster.RoomTypeId,
                                      roomtype.Type,
                                      roommaster.RoomNo,
                                      roommaster.Description,


                                  }
                                   ).ToListAsync();

                

                return Ok(new { Code = 200, Message = "Rooms fetched successfully", Data = data });
            }
            
            catch (Exception ex)
            {

                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("AddRoomMaster")]
        public async Task<IActionResult> AddRoomMaster([FromBody] RoomMasterDTO room)
        {
            if (room == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<RoomMaster>(room);
                var validator = new RoomMasterValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, Message = errors });
                }

                SetMastersDefault(cm, companyId, userId);

                await _context.RoomMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchRoomMaster/{id}")]
        public async Task<IActionResult> PatchRoomMaster(int id, [FromBody] JsonPatchDocument<RoomMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var room = await _context.RoomMaster.FindAsync(id);

                if (room == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(room, ModelState);
                if(room.IsActive == true)
                {
                    var validator = new RoomMasterValidator(_context);
                    var result = await validator.ValidateAsync(room);
                    if (!result.IsValid)
                    {
                        var errors = result.Errors.Select(x => new
                        {
                            Error = x.ErrorMessage,
                            Field = x.PropertyName
                        }).ToList();
                        return Ok(new { Code = 202, Message = errors });
                    }
                }
                
                room.UpdatedDate = DateTime.Now;
                if (!ModelState.IsValid)
                {
                    var errorMessages = ModelState
                                        .Where(x => x.Value.Errors.Any())
                                        .SelectMany(x => x.Value.Errors)
                                        .Select(x => x.ErrorMessage)
                                        .ToList();
                    return Ok(new { Code = 500, Message = errorMessages });
                }

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room updated successfully" });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetRoomById/{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await (from roommaster in _context.RoomMaster
                                  join floor in _context.FloorMaster on roommaster.FloorId equals floor.FloorId into floorRoom
                                  from fr in floorRoom.DefaultIfEmpty()
                                  join building in _context.BuildingMaster on roommaster.BuildingId equals building.BuildingId into buildingRoom
                                  from fb in buildingRoom.DefaultIfEmpty()
                                  join property in _context.CompanyDetails on roommaster.PropertyId equals property.PropertyId
                                  join roomtype in _context.RoomCategoryMaster on roommaster.RoomTypeId equals roomtype.Id
                                  where roommaster.IsActive == true && roommaster.PropertyId == companyId && fr.IsActive == true && fb.IsActive == true && property.IsActive == true && roomtype.IsActive == true && roommaster.RoomId == id
                                  select new
                                  {
                                      RoomId = roommaster.RoomId,
                                      FloorId = roommaster.FloorId,
                                      FloorObj = roommaster.FloorId != null ? new
                                      {
                                          Label= fr.FloorNumber,
                                          Value = roommaster.FloorId
                                      } : null,

                                      BuildingId = roommaster.BuildingId,
                                      BuildingObj = roommaster.BuildingId !=null ? new
                                      {
                                          Label = fb.BuildingName,
                                          Value = roommaster.BuildingId

                                      } : null,

                                      PropertyId = roommaster.PropertyId,
                                      RoomTypeId = roommaster.RoomTypeId,
                                      RoomTypeObj =  new
                                      {
                                          Label = roommaster.RoomTypeId,
                                          Value = roomtype.Type
                                      } ,
                                      RoomNo = roommaster.RoomNo,
                                      Description = roommaster.Description,


                                  }
                                   ).FirstOrDefaultAsync();

                return data==null
                    ? NotFound(new { Code = 404, Message = "Room not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Room fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetRoomMasterFormLoad")]
        public async Task<IActionResult> GetRoomMasterFormLoad()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                //building list
                var buildingList = await _context.BuildingMaster.Where(x => x.IsActive == true && x.PropertyId == companyId).ToListAsync();

                //floor list
                var floorList = await _context.FloorMaster.Where(x => x.IsActive == true && x.PropertyId == companyId).ToListAsync();

                //room type list
                var roomTypeList = await _context.RoomCategoryMaster.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync() ;

                return Ok(new { Code = 200, Message = "Data fetched successfully", buildingList= buildingList, floorList = floorList, roomTypeList  = roomTypeList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage }); ;
            }
        }

        [HttpPost("AddRoomRateMaster")]
        public async Task<IActionResult> AddRoomRateMaster([FromBody] List<RoomRateMasterDTO> roomRateList)
        {
            var transaction = _context.Database.BeginTransaction();
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                if(roomRateList == null || roomRateList.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "Invalid Data" });
                }
                foreach(var item in roomRateList)
                {
                    var validator = new RoomRateValidator(_context);
                    var result = validator.Validate(item);
                    if (!result.IsValid)
                    {
                        var error = result.Errors.Select(x => new
                        {
                            Field = x.PropertyName,
                            Error = x.ErrorMessage
                        }).ToList();

                        return Ok(new { Code = 400, Message = error });
                    }
                }

                foreach(var item in roomRateList)
                {
                    //for standard
                    if(item.RateType == Constants.Constants.Standard || item.RateType == Constants.Constants.Hour)
                    {
                        //check any standard rate for given roomtype
                        var isStandardRateExists = await _context.RoomRateMaster.FirstOrDefaultAsync(x => x.RoomTypeId == item.RoomTypeId && x.IsActive == true && x.CompanyId == companyId);
                        if(isStandardRateExists == null)
                        {

                            var roomRateMaster = new RoomRateMaster();
                            roomRateMaster.RoomTypeId = item.RoomTypeId;
                            roomRateMaster.RoomRate = item.RoomRate;
                            roomRateMaster.Gst = item.Gst;
                            roomRateMaster.Discount = item.Discount;
                            roomRateMaster.IsActive = true;
                            roomRateMaster.CreatedDate = DateTime.Now;
                            roomRateMaster.UpdatedDate = DateTime.Now;
                            roomRateMaster.UserId = userId;
                            roomRateMaster.CompanyId = companyId;
                            roomRateMaster.GstTaxType = item.GstTaxType;
                            roomRateMaster.GstAmount = Calculation.CalculateGst(item.RoomRate, item.Gst);
                            roomRateMaster.HourId = item.HourId;
                            await _context.RoomRateMaster.AddAsync(roomRateMaster);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            isStandardRateExists.RoomRate = item.RoomRate;
                            isStandardRateExists.Gst = item.Gst;
                            isStandardRateExists.Discount = item.Discount;
                            isStandardRateExists.UpdatedDate = DateTime.Now;
                            isStandardRateExists.GstTaxType = item.GstTaxType;
                            isStandardRateExists.HourId = item.HourId;
                            isStandardRateExists.GstAmount = Calculation.CalculateGst(item.RoomRate, item.Gst);
                            _context.RoomRateMaster.Update(isStandardRateExists);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if(item.RateType == Constants.Constants.Custom)
                    {
                        var isCustomRateExists = await _context.RoomRateDateWise.FirstOrDefaultAsync(x => x.RoomTypeId == item.RoomTypeId && x.FromDate == item.FromDate && x.ToDate == item.ToDate && x.IsActive == true && x.RateType == Constants.Constants.Custom && x.CompanyId == companyId);
                        if (isCustomRateExists == null)
                        {
                            var roomrateDateWise = new RoomRateDateWise();
                            roomrateDateWise.RoomTypeId = item.RoomTypeId;
                            roomrateDateWise.RoomRate = item.RoomRate;
                            roomrateDateWise.Gst = item.Gst;
                            roomrateDateWise.Discount = item.Discount;
                            roomrateDateWise.FromDate = item.FromDate ?? Constants.Constants.DefaultDate;
                            roomrateDateWise.ToDate = item.ToDate ?? Constants.Constants.DefaultDate;
                            roomrateDateWise.RateType = Constants.Constants.Custom;
                            roomrateDateWise.WeekendDay = -1;
                            roomrateDateWise.IsActive = true;
                            roomrateDateWise.CreatedDate = DateTime.Now;
                            roomrateDateWise.UpdatedDate = DateTime.Now;
                            roomrateDateWise.UserId = userId;
                            roomrateDateWise.CompanyId = companyId;
                            roomrateDateWise.GstTaxType = item.GstTaxType;
                            roomrateDateWise.GstAmount = Calculation.CalculateGst(item.RoomRate, item.Gst);
                            await _context.RoomRateDateWise.AddAsync(roomrateDateWise);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            isCustomRateExists.RoomRate = item.RoomRate;
                            isCustomRateExists.Gst = item.Gst;
                            isCustomRateExists.Discount = item.Discount;
                            isCustomRateExists.UpdatedDate = DateTime.Now;
                            isCustomRateExists.GstAmount =  Calculation.CalculateGst(item.RoomRate, item.Gst);
                            isCustomRateExists.GstTaxType = item.GstTaxType;
                            _context.RoomRateDateWise.Update(isCustomRateExists);
                            await _context.SaveChangesAsync();
                        }
                    }

                    else if (item.RateType == Constants.Constants.Weekend)
                    {
                        DayOfWeek currentDay = GetDayName(item.WeekendDay);
                        List<DateTime> dayDates = GetDayDates(item.FromDate ?? Constants.Constants.DefaultDate, item.ToDate ?? Constants.Constants.DefaultDate
                            , currentDay);

                        foreach(var date in dayDates)
                        {
                            var isWeekendRateExists = await _context.RoomRateDateWise.FirstOrDefaultAsync(x => x.RoomTypeId == item.RoomTypeId && x.FromDate == date && x.ToDate == date && x.IsActive == true && x.RateType == Constants.Constants.Weekend && x.CompanyId == companyId);
                            if(isWeekendRateExists == null)
                            {
                                var roomrateDateWise = new RoomRateDateWise();
                                roomrateDateWise.RoomTypeId = item.RoomTypeId;
                                roomrateDateWise.RoomRate = item.RoomRate;
                                roomrateDateWise.Gst = item.Gst;
                                roomrateDateWise.Discount = item.Discount;
                                roomrateDateWise.FromDate = date;
                                roomrateDateWise.ToDate = date;
                                roomrateDateWise.RateType = Constants.Constants.Weekend;
                                roomrateDateWise.WeekendDay = Convert.ToInt16(currentDay);
                                roomrateDateWise.IsActive = true;
                                roomrateDateWise.CreatedDate = DateTime.Now;
                                roomrateDateWise.UpdatedDate = DateTime.Now;
                                roomrateDateWise.UserId = userId;
                                roomrateDateWise.CompanyId = companyId;
                                roomrateDateWise.GstTaxType = item.GstTaxType;
                                roomrateDateWise.GstAmount =  Calculation.CalculateGst(item.RoomRate, item.Gst);
                                await _context.RoomRateDateWise.AddAsync(roomrateDateWise);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                isWeekendRateExists.RoomRate = item.RoomRate;
                                isWeekendRateExists.Gst = item.Gst;
                                isWeekendRateExists.Discount = item.Discount;
                                isWeekendRateExists.UpdatedDate = DateTime.Now;
                                isWeekendRateExists.GstTaxType = item.GstTaxType;
                                isWeekendRateExists.GstAmount =  Calculation.CalculateGst(item.RoomRate, item.Gst);
                                _context.RoomRateDateWise.Update(isWeekendRateExists);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Room Rates saved successfully" });


            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = ex.Message });
            }
        }

        [HttpPut("UpdateRoomRate/{id}")]
        public async Task<IActionResult> UpdateRoomRate(int id, [FromBody] RoomRateMasterDTO roomRate)
        {
            var transaction = _context.Database.BeginTransaction();
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {

                var validator = new RoomRateValidator(_context);
                var result = validator.Validate(roomRate);
                if (!result.IsValid)
                {
                    var error = result.Errors.Select(x => new
                    {
                        Field = x.PropertyName,
                        Error = x.ErrorMessage
                    }).ToList();

                    return Ok(new { Code = 400, Message = error });
                }

                
                    //for standard
                    if (roomRate.RateType == Constants.Constants.Standard || roomRate.RateType == Constants.Constants.Hour)
                    {
                        //check any standard rate for given roomtype
                        var isStandardRateExists = await _context.RoomRateMaster.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);
                        if (isStandardRateExists == null)
                        {
                        return Ok(new { Code = 400, Message = "data not found" });
                        }
                        else
                        {
                            isStandardRateExists.RoomRate = roomRate.RoomRate;
                            isStandardRateExists.Gst = roomRate.Gst;
                            isStandardRateExists.Discount = roomRate.Discount;
                            isStandardRateExists.UpdatedDate = DateTime.Now;
                            isStandardRateExists.GstTaxType = roomRate.GstTaxType;
                        isStandardRateExists.HourId = roomRate.HourId;
                            _context.RoomRateMaster.Update(isStandardRateExists);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if (roomRate.RateType == Constants.Constants.Custom || roomRate.RateType == Constants.Constants.Weekend)
                    {
                        var isCustomRateExists = await _context.RoomRateDateWise.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);
                        if (isCustomRateExists == null)
                        {
                        return Ok(new { Code = 400, Message = "data not found" });
                    }
                        else
                        {
                            isCustomRateExists.RoomRate = roomRate.RoomRate;
                            isCustomRateExists.Gst = roomRate.Gst;
                            isCustomRateExists.Discount = roomRate.Discount;
                            isCustomRateExists.UpdatedDate = DateTime.Now;

                            isCustomRateExists.GstTaxType = roomRate.GstTaxType;
                            _context.RoomRateDateWise.Update(isCustomRateExists);
                            await _context.SaveChangesAsync();
                        }
                    }

                    
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Room Rates updated successfully" });


            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = ex.Message });
            }
        }

        private DayOfWeek GetDayName(int day)
        {
            switch (day)
            {
                case 1:
                    return DayOfWeek.Sunday;
                case 2:
                    return DayOfWeek.Monday;
                case 3:
                    return DayOfWeek.Tuesday;
                case 4:
                    return DayOfWeek.Wednesday;
                case 5:
                    return DayOfWeek.Thursday;
                case 6:
                    return DayOfWeek.Friday;
                case 7:
                    return DayOfWeek.Saturday;
                default:
                    return DayOfWeek.Sunday;

            }

        }

        private  List<DateTime> GetDayDates(DateTime startDate, DateTime endDate, DayOfWeek weekDay)
        {
            if (startDate == Constants.Constants.DefaultDate || endDate == Constants.Constants.DefaultDate)
            {
                return new List<DateTime>();
            }
            List<DateTime> dates = new List<DateTime>();
            // Find the first Monday in the range
            DateTime currentDate = startDate;

            while (currentDate.DayOfWeek != weekDay)
            {
                currentDate = currentDate.AddDays(1);
                if (currentDate > endDate)
                {
                    break;
                }
            }

            // Add all Mondays within the range
            while (currentDate <= endDate)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(7); // Move to next Monday
            }

            return dates;
        }
        [HttpGet("GetRoomRates")]
        public async Task<IActionResult> GetRoomRates()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                var roomRates = await (from cat in _context.RoomCategoryMaster
                                       join rate in _context.RoomRateMaster on cat.Id equals rate.RoomTypeId into roomrate
                                       from rrates in roomrate.DefaultIfEmpty()
                                       join hour in _context.HourMaster on rrates.HourId equals hour.Id into hourrate
                                       from hours in hourrate.DefaultIfEmpty()
                                       where cat.IsActive == true && rrates.IsActive == true && cat.CompanyId == companyId
                                       select new
                                       {
                                           Id = rrates.Id,
                                           RoomTypeId = cat.Id,
                                           RoomType = cat.Type,
                                           RoomRate = rrates.RoomRate,
                                           Gst = rrates.Gst,
                                           Discount = rrates.Discount,
                                           GstTaxType = rrates.GstTaxType ?? "",
                                           Hour = hours == null ? "" : hours.Hour.ToString()

                                       }).ToListAsync();

                return Ok(new { Code = 200, Message = "Room rated fetched successfully", data = roomRates });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetRoomRatesDateWise/{id}")]
        public async Task<IActionResult> GetRoomRatesDateWise(int id)
        {
            try
            {
                if(id == 0)
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }
                var isRateExists = await _context.RoomRateMaster.FirstOrDefaultAsync(x => x.IsActive == true && x.Id == id);
                if(isRateExists == null)
                {
                    return Ok(new { Code = 400, Message = "Data not found" });
                }
                var rates = await _context.RoomRateDateWise
    .Where(x => x.IsActive && x.RoomTypeId == isRateExists.RoomTypeId)
    .Select(x => new 
    {
        Id = x.Id,
        RoomTypeId = x.RoomTypeId,
        RoomRate = x.RoomRate,
        Gst = x.Gst,
        Discount = x.Discount,
        FromDate = x.FromDate.ToString("dd-MM-yyyy"),
        ToDate = x.ToDate.ToString("dd-MM-yyyy"),
        RateType = x.RateType,
        WeekendDay = x.WeekendDay,
        CreatedDate = x.CreatedDate,
        UpdatedDate = x.UpdatedDate,
        GstTaxType = x.GstTaxType
    })
    .ToListAsync();

                return Ok(new { Code = 200, Message = "Room rated fetched successfully", data = rates });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetRoomRatesById/{id}")]
        public async Task<IActionResult> GetRoomRatesById(int id, string rateType = "")
        {
            try
            {
                if (id == 0)
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }
                if(rateType == "All")
                {
                    var rates = await (from rate in _context.RoomRateMaster
                                       join cat in _context.RoomCategoryMaster on rate.RoomTypeId equals cat.Id
                                       join hour in _context.HourMaster on rate.HourId equals hour.Id
                                       where rate.IsActive == true && rate.Id == id
                                       select new
                                       {
                                           Id = rate.Id,
                                           RoomType = cat.Type,
                                           RoomTypeId = rate.RoomTypeId,
                                           RoomRate = rate.RoomRate,
                                           Gst = rate.Gst,
                                           GstTaxType = rate.GstTaxType,
                                           Discount = rate.Discount, 
                                           HourId = hour.Id,
                                           Hour = hour.Hour
                                       }).FirstOrDefaultAsync();
                    
                    if (rates == null)
                    {
                        return Ok(new { Code = 400, Message = "Data not found" });
                    }
                    return Ok(new { Code = 200, Message = "Room rated fetched successfully", data = rates });
                }
                else
                {
                    var rates = await (from rate in _context.RoomRateDateWise
                                       join cat in _context.RoomCategoryMaster on rate.RoomTypeId equals cat.Id
                                       where rate.IsActive == true && rate.Id == id
                                       select new
                                       {
                                           Id = rate.Id,
                                           RoomType = cat.Type,
                                           RoomTypeId = rate.RoomTypeId,
                                           RoomRate = rate.RoomRate,
                                           Gst = rate.Gst,
                                           GstTaxType = rate.GstTaxType,
                                           Discount = rate.Discount,
                                           FromDate = rate.FromDate.ToString("yyyy-MM-dd"),
                                           ToDate = rate.ToDate.ToString("yyyy-MM-dd"),
                                           RateType = rate.RateType,
                                           WeekendDay = rate.WeekendDay + 1
                                       }).FirstOrDefaultAsync();

                    if (rates == null)
                    {
                        return Ok(new { Code = 400, Message = "Data not found" });
                    }
                    return Ok(new { Code = 200, Message = "Room rated fetched successfully", data = rates });
                }
                    

                
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpDelete("DeleteRoomRate/{id}")]
        public async Task<IActionResult> DeleteRoomRatesById(int id, string rateType = "")
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                if(rateType == "All")
                {
                    var isRateExists = await _context.RoomRateMaster.FirstOrDefaultAsync(x => x.IsActive == true && x.Id == id);
                    if(isRateExists == null)
                    {
                        return Ok(new { Code = 400, Message = "Data not found" });
                    }
                    else
                    {
                        var roomRates = await _context.RoomRateDateWise.Where(x => x.RoomTypeId == isRateExists.RoomTypeId && x.IsActive == true).ToListAsync();

                        foreach(var item in roomRates)
                        {
                            item.IsActive = false;
                            _context.RoomRateDateWise.Update(item);
                           
                        }
                        isRateExists.IsActive = false;
                        _context.RoomRateMaster.Update(isRateExists);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                }
                else
                {
                    var roomRates = await _context.RoomRateDateWise.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);
                    if (roomRates == null)
                    {
                        return Ok(new { Code = 400, Message = "Data not found" });
                    }
                    roomRates.IsActive = false;
                    _context.RoomRateDateWise.Update(roomRates);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                return Ok(new { Code = 200, Message = "Room rate deleted siccessfully" });
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetDocumentNo")]
        public async Task<IActionResult> GetDocumentNo(string type)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                string financialYear = (HttpContext.Request.Headers["FinancialYear"]).ToString();

                var getbookingno = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Type == type &&x.FinancialYear == financialYear);
                if (getbookingno == null || getbookingno.Suffix == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found.", data = getbookingno });
                }
                getbookingno.Suffix = getbookingno.Prefix + "/" + getbookingno.Prefix1 + "/" + getbookingno.Prefix2 + getbookingno.Suffix + getbookingno.Number + getbookingno.LastNumber;

                return Ok(new { Code = 200, message = "Document Number Recieved Successfully.", data = getbookingno });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, message = Constants.Constants.ErrorMessage});
            }
        }

        [HttpGet("GetDocumentMasterList")]
        public async Task<IActionResult> GetDocumentMasterList()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);                
                string financialYear = (HttpContext.Request.Headers["FinancialYear"]).ToString();

                var documents = await _context.DocumentMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.FinancialYear == financialYear).ToListAsync();

                return Ok(new { Code = 200, Message = "Data found", data = documents });


            }
            catch (Exception)
            {
                return Ok(new { Code = 500, MEssage = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetDocumentById/{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                
                string financialYear = (HttpContext.Request.Headers["FinancialYear"]).ToString();


                var documents = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.FinancialYear == financialYear && x.DocId == id);

                return Ok(new { Code = 200, Message = "Data found", data = documents });


            }
            catch (Exception)
            {
                return Ok(new { Code = 500, MEssage = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddDocumentMaster")]
        public async Task<IActionResult> AddDocumentMaster([FromBody] DocumentMasterDTO documentMasterDTO)
        {
            var currentDate = DateTime.Now;
            try
            {
                if(documentMasterDTO == null)
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }
                var documentMaster = _mapper.Map<DocumentMaster>(documentMasterDTO);
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string financialYear = (HttpContext.Request.Headers["FinancialYear"]).ToString();
                documentMaster.CompanyId = companyId;
                documentMaster.FinancialYear = financialYear;
                documentMaster.CreatedBy = userId;
                documentMaster.IsActive = true;
                documentMaster.CreatedDate = currentDate;
                documentMaster.UpdatedDate = currentDate;
                documentMaster.LastNumber = 1; 
                var validator = new DocumentMasterValidator(_context);
                var result = await validator.ValidateAsync(documentMaster);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, Message = errors });
                }

                await _context.DocumentMaster.AddAsync(documentMaster);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Document created successfully" });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, MEssage = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchDocumentMaster/{id}")]
        public async Task<IActionResult> PatchDocumentMaster(int id, [FromBody] JsonPatchDocument<DocumentMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var document = await _context.DocumentMaster.FindAsync(id);

                if (document == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(document, ModelState);
                if (document.IsActive == true)
                {
                    var validator = new DocumentMasterValidator(_context);
                    var result = await validator.ValidateAsync(document);
                    if (!result.IsValid)
                    {
                        var errors = result.Errors.Select(x => new
                        {
                            Error = x.ErrorMessage,
                            Field = x.PropertyName
                        }).ToList();
                        return Ok(new { Code = 202, Message = errors });
                    }
                }

                document.UpdatedDate = DateTime.Now;
                if (!ModelState.IsValid)
                {
                    var errorMessages = ModelState
                                        .Where(x => x.Value.Errors.Any())
                                        .SelectMany(x => x.Value.Errors)
                                        .Select(x => x.ErrorMessage)
                                        .ToList();
                    return Ok(new { Code = 500, Message = errorMessages });
                }

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetAgentList")]
        public async Task<IActionResult> GetAgentList()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                

                var agents = await _context.AgentDetails.Where(x => x.IsActive == true && x.CompanyId == companyId).ToListAsync();

                return Ok(new { Code = 200, Message = "Data found", data = agents });


            }
            catch (Exception)
            {
                return Ok(new { Code = 500, MEssage = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetAgentListById/{id}")]
        public async Task<IActionResult> GetAgentListById(int id)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);


                var agents = await _context.AgentDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.CompanyId == companyId && x.AgentId == id);

                return Ok(new { Code = 200, Message = "Data found", data = agents });


            }
            catch (Exception)
            {
                return Ok(new { Code = 500, MEssage = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddAgent")]
        public async Task<IActionResult> AddAgent([FromForm] AgentDetailsDTO agentDetailsDTO, IFormFile? file)
        {
            try
            {
                if(agentDetailsDTO == null)
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                
                var agentDetails = _mapper.Map<AgentDetails>(agentDetailsDTO);
                

                var validator = new AgentDetailValidator(_context);
                var result = await validator.ValidateAsync(agentDetails);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    
                    return Ok(new { Code = 202, Message = errors });

                    
                }

                SetMastersDefault(agentDetails, companyId, userId);
                if (file != null)
                {
                    
                    agentDetails.ContractFile = await Constants.Constants.AddFile(file);

                }
                

                await _context.AgentDetails.AddAsync(agentDetails);
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Agent created successfully" });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPatch("PatchAgentMaster/{id}")]
        public async Task<IActionResult> PatchAgentMaster(int id, [FromForm] string patchDoc, IFormFile? file)
        {
            try
            {
                if (patchDoc == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }
                var patchDocument = JsonConvert.DeserializeObject<JsonPatchDocument<AgentDetails>>(patchDoc);
                var agent = await _context.AgentDetails.FindAsync(id);

                if (agent == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(agent, ModelState);
                if (agent.IsActive == true)
                {
                    var validator = new AgentDetailValidator(_context);
                    var result = await validator.ValidateAsync(agent);
                    if (!result.IsValid)
                    {
                        var errors = result.Errors.Select(x => new
                        {
                            Error = x.ErrorMessage,
                            Field = x.PropertyName
                        }).ToList();
                        return Ok(new { Code = 202, Message = errors });
                    }
                }

                agent.UpdatedDate = DateTime.Now;
                if (file != null)
                {
                    agent.ContractFile = await Constants.Constants.AddFile(file);

                }

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Agent updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

    }
}
