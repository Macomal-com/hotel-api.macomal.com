using AutoMapper;
using FluentValidation;
using hotel_api.Constants;
using hotel_api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Repository.Models;
using Repository.RequestDTO;
using RepositoryModels.Repository;
using System.ComponentModel.Design;
using System.Data;
using System.Reflection.Metadata;
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
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                var data = await (from cat in _context.RoomCategoryMaster
                                  join bed in _context.BedTypeMaster on cat.BedTypeId equals bed.BedTypeId into bedcat
                                  from o in bedcat.DefaultIfEmpty()
                                  where cat.IsActive == true  && cat.CompanyId == companyId
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
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                var data = await (from cat in _context.RoomCategoryMaster
                                  join bed in _context.BedTypeMaster on cat.BedTypeId equals bed.BedTypeId into bedcat
                from o in bedcat.DefaultIfEmpty()
                                  where cat.IsActive == true && cat.Id == id && cat.CompanyId == companyId
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
            var transaction = await _context.Database.BeginTransactionAsync();
            if (roomCat == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<RoomCategoryMaster>(roomCat);
                SetMastersDefault(cm, companyId, userId);
                var validator = new RoomCategoryValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }


                cm.BedTypeId = cm.BedTypeId == 0 ? null : cm.BedTypeId;
                cm.ColorCode = ColourCodeService.GetUniqueColor(_context, cm.CompanyId);
                await _context.RoomCategoryMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                var roomRate = new RoomRateMaster
                {
                    RoomTypeId = cm.Id,
                    RoomRate = 0,
                    Gst = 0,
                    Discount = 0,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UserId = userId,
                    CompanyId = companyId,
                    GstTaxType = "",
                    HourId = 0,
                    GstAmount = 0,
                    RatePriority = 0
                };
                _context.RoomRateMaster.Add(roomRate);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Room Type created successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("EditRoomCategoryMaster")]
        public async Task<IActionResult> PatchRoomCategoryMaster([FromBody] RoomCategoryMaster model)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 400, message = "Invalid request. Data is null.", data = new object() });
                }

                var roomCat = await _context.RoomCategoryMaster.FindAsync(model.Id);

                if (roomCat == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }
                var validator = new RoomCategoryValidator(_context);
                if (!model.ChangeDetails)
                {
                    // Create new with updated data (from patched `roomCat`)
                    roomCat.IsActive = false;
                    roomCat.UpdatedDate = DateTime.Now;
                    _context.RoomCategoryMaster.Update(roomCat);
                    await _context.SaveChangesAsync();

                    var newRoomCat = new RoomCategoryMaster
                    {
                        Type = model.Type,
                        Description = model.Description,
                        MinPax = model.MinPax,
                        MaxPax = model.MaxPax,
                        BedTypeId = model.BedTypeId,
                        NoOfRooms = model.NoOfRooms,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UserId = roomCat.UserId,
                        CompanyId = roomCat.CompanyId,
                        DefaultPax = model.DefaultPax,
                        ExtraBed = model.ExtraBed,
                        ColorCode = ColourCodeService.GetUniqueColor(_context, roomCat.CompanyId),
                        RefRoomTypeId = roomCat.Id
                    };
                    var result = await validator.ValidateAsync(newRoomCat);

                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }
                    await _context.RoomCategoryMaster.AddAsync(newRoomCat);
                    await _context.SaveChangesAsync();

                    //update room master
                    var rooms = await _context.RoomMaster.Where(x => x.IsActive == true && x.RoomTypeId == roomCat.Id && x.CompanyId == companyId).ToListAsync();

                    foreach(var room in rooms)
                    {
                        room.RoomTypeId = newRoomCat.Id;
                        _context.RoomMaster.Update(room);
                    }

                    var roomRates = await _context.RoomRateMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomCat.Id).ToListAsync();
                    foreach (var rate in roomRates)
                    {
                        rate.RoomTypeId = newRoomCat.Id;
                        _context.RoomRateMaster.Update(rate);
                    }

                    var roomRatesDateWise = await _context.RoomRateDateWise.Where(x => x.IsActive == true && x.CompanyId == companyId && x.RoomTypeId == roomCat.Id).ToListAsync();
                    foreach (var rate in roomRatesDateWise)
                    {
                        rate.RoomTypeId = newRoomCat.Id;
                        _context.RoomRateDateWise.Update(rate);
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { Code = 200, Message = "Room Type updated successfully" });
                }
                
                else
                {
                    roomCat.Type = model.Type;
                    roomCat.Description = model.Description;
                    roomCat.MinPax = model.MinPax;
                    roomCat.MaxPax = model.MaxPax;
                    roomCat.NoOfRooms = model.NoOfRooms;
                    roomCat.IsActive = true;
                    roomCat.UserId = roomCat.UserId;
                    roomCat.CompanyId = roomCat.CompanyId;
                    roomCat.DefaultPax = model.DefaultPax;
                    roomCat.ExtraBed = model.ExtraBed;
                    roomCat.UpdatedDate = DateTime.Now;
                    roomCat.BedTypeId = model.BedTypeId == 0 ? null : model.BedTypeId;

                    var result = await validator.ValidateAsync(roomCat);

                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    _context.RoomCategoryMaster.Update(roomCat);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { Code = 200, Message = "Room Type updated successfully" });
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("DeleteRoomCategoryMaster/{id}")]
        public async Task<IActionResult> DeleteRoomCategoryMaster(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

            if (id == 0)
            {
                return Ok(new { Code = 400, message = "Invalid Id.", data = new object() });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingProduct = await _context.RoomCategoryMaster
                    .FirstOrDefaultAsync(u => u.Id == id && u.IsActive == true);

                if (existingProduct == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }
                var validator = new RoomCategoryDeleteValidator(_context);
                var result = await validator.ValidateAsync(existingProduct);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                // Update existing fields
                existingProduct.IsActive = false;

                _context.RoomCategoryMaster.Update(existingProduct);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { Code = 200, message = "Room Type deleted successfully", data = new object() });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Optional: log the exception
                return StatusCode(500, new { Code = 500, message = "An error occurred", data = new object() });
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
                                  
                                  join property in _context.CompanyDetails on roommaster.PropertyId equals property.PropertyId
                                  join roomtype in _context.RoomCategoryMaster on roommaster.RoomTypeId equals roomtype.Id
                                  join floor in _context.FloorMaster on roommaster.FloorId equals floor.FloorId into floorRoom
                                  from fr in floorRoom.DefaultIfEmpty()
                                  join building in _context.BuildingMaster on roommaster.BuildingId equals building.BuildingId into buildingRoom
                                  from fb in buildingRoom.DefaultIfEmpty()
                                  where roommaster.IsActive == true && roommaster.PropertyId == companyId  && property.IsActive == true && roomtype.IsActive == true
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
                SetMastersDefault(cm, companyId, userId);
                var validator = new RoomMasterValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                cm.BuildingId = cm.BuildingId == 0 ? null : cm.BuildingId;
                cm.FloorId = cm.FloorId == 0 ? null : cm.FloorId;
                await _context.RoomMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("EditRoomMaster")]
        public async Task<IActionResult> EditRoomMaster([FromBody] RoomMaster model)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (model == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 500, Message = "Invalid Data" });
                }

                var room = await _context.RoomMaster.FindAsync(model.RoomId);

                if (room == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }
                var validator = new RoomMasterValidator(_context);
                if (!model.ChangeDetails)
                {
                    // Create new with updated data (from patched `roomCat`)
                    room.IsActive = false;
                    room.UpdatedDate = DateTime.Now;
                    _context.RoomMaster.Update(room);
                    await _context.SaveChangesAsync();

                    var newRoom = new RoomMaster
                    {
                        FloorId = model.FloorId,
                        BuildingId = model.BuildingId,
                        PropertyId = model.PropertyId,
                        RoomNo = model.RoomNo,
                        RoomTypeId = model.RoomTypeId,
                        Description = model.Description,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UserId = userId,
                        CompanyId = companyId,
                    };
                    var result = await validator.ValidateAsync(newRoom);

                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }
                    await _context.RoomMaster.AddAsync(newRoom);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { Code = 200, Message = "Room updated successfully" });
                }

                else
                {
                    room.FloorId = model.FloorId;
                    room.BuildingId = model.BuildingId;
                    room.PropertyId = model.PropertyId;
                    room.RoomNo = model.RoomNo;
                    room.RoomTypeId = model.RoomTypeId;
                    room.Description = model.Description;
                    room.UpdatedDate = DateTime.Now;

                    var result = await validator.ValidateAsync(room);

                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    _context.RoomMaster.Update(room);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { Code = 200, Message = "Room updated successfully" });
                }
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("DeleteRoomMaster/{id}")]
        public async Task<IActionResult> DeleteRoomMaster(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

            if (id == 0)
            {
                return Ok(new { Code = 400, message = "Invalid Id.", data = new object() });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingProduct = await _context.RoomMaster
                    .FirstOrDefaultAsync(u => u.RoomId == id && u.IsActive == true);

                if (existingProduct == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }
                
                if (existingProduct == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 404, message = "Room does not exist.", data = new object() });
                }
                var validator = new RoomMasterValidator(_context);
                var result = await validator.ValidateAsync(existingProduct);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                // Update existing fields
                existingProduct.IsActive = false;

                _context.RoomMaster.Update(existingProduct);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { Code = 200, message = "Service deleted successfully", data = new object() });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Optional: log the exception
                return StatusCode(500, new { Code = 500, message = "An error occurred", data = new object() });
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
                                  where roommaster.IsActive == true && roommaster.PropertyId == companyId && (fr == null || fr.IsActive == true) && (fb == null || fb.IsActive == true) && property.IsActive == true && roomtype.IsActive == true && roommaster.RoomId == id
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
                                      RoomType = roomtype.Type,
                                      RoomTypeObj =  new
                                      {
                                          Label = roomtype.Type,
                                          Value = roommaster.RoomTypeId
                                      } ,
                                      RoomNo = roommaster.RoomNo,
                                      Description = roommaster.Description,


                                  }).FirstOrDefaultAsync();

                return data==null
                    ? Ok(new { Code = 404, Message = "Room not found", Data = Array.Empty<object>() })
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

        [HttpPost("CheckRoomRatesExists")]
        public async Task<IActionResult> CheckRoomRatesExists([FromBody] List<RoomRateMasterDTO> roomRateList)
        {
            try
            {
                bool flag = false;
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                //check rates already exists on the date on || not
                foreach (var item in roomRateList)
                {
                    if (item.RateType == Constants.Constants.Custom )
                    {
                        var isRateExists = await _context.RoomRateDateWise.Where(x => x.RoomTypeId == item.RoomTypeId && x.IsActive == true && (x.RateType == Constants.Constants.Custom || x.RateType == Constants.Constants.Weekend) && x.CompanyId == companyId && ((item.FromDate >= x.FromDate && item.FromDate <= x.ToDate) ||
                            (item.ToDate >= x.FromDate && item.ToDate <= x.ToDate) ||
                            (x.FromDate >= item.FromDate && x.FromDate <= item.ToDate) ||
                            (x.ToDate >= item.FromDate && x.ToDate <= item.ToDate))).OrderBy(x=>x.FromDate).ToListAsync();
                        if(isRateExists.Count == 0)
                        {
                            return Ok(new { Code = 404, Message = "No rates found" });
                        }
                        else
                        {
                            return Ok(new { Code = 200, Message = "Rate already exists", data = isRateExists });
                        }
                        
                    }
                    else if(item.RateType == Constants.Constants.Weekend)
                    {
                        DayOfWeek currentDay = GetDayName(item.WeekendDay);
                        List<DateOnly> dayDates = GetDayDates(item.FromDate ?? Constants.Constants.DefaultDate, item.ToDate ?? Constants.Constants.DefaultDate
                            , currentDay);
                        foreach(var date in dayDates)
                        {
                            var isRateExists = await _context.RoomRateDateWise.Where(x => x.RoomTypeId == item.RoomTypeId && x.IsActive == true && (x.RateType == Constants.Constants.Custom || x.RateType == Constants.Constants.Weekend) && x.CompanyId == companyId && ((item.FromDate >= date || item.FromDate <= date))).OrderBy(x=>x.FromDate).ToListAsync();
                            if (isRateExists.Count == 0)
                            {
                                return Ok(new { Code = 404, Message = "No rates found" });
                            }
                            else
                            {
                                return Ok(new { Code = 200, Message = "Rate already exists", data = isRateExists });
                            }
                        }
                        
                    }
                    
                }
                return Ok(new { Code = 404, Message = "No rates found" });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("AddRoomRateMaster")]
        public async Task<IActionResult> AddRoomRateMaster([FromBody] List<RoomRateMasterDTO> roomRateList)
        {
            var transaction = _context.Database.BeginTransaction();
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            var currentDate = DateTime.Now;
            try
            {
                if(roomRateList == null || roomRateList.Count == 0)
                {
                    return Ok(new { Code = 400, Message = "Please enter room rates" });
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
                    if(item.RateType == Constants.Constants.Standard)
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
                            roomRateMaster.CreatedDate = currentDate;
                            roomRateMaster.UpdatedDate = currentDate;
                            roomRateMaster.UserId = userId;
                            roomRateMaster.CompanyId = companyId;
                            roomRateMaster.GstTaxType = item.GstTaxType;
                            roomRateMaster.GstAmount = 0;
                            roomRateMaster.HourId = item.HourId;
                            roomRateMaster.RatePriority = Constants.Constants.LowPrority;
                            await _context.RoomRateMaster.AddAsync(roomRateMaster);
                           
                        }
                        else
                        {
                            isStandardRateExists.RoomRate = item.RoomRate;
                            isStandardRateExists.Gst = item.Gst;
                            isStandardRateExists.Discount = item.Discount;
                            isStandardRateExists.UpdatedDate = currentDate;
                            isStandardRateExists.GstTaxType = item.GstTaxType;
                            isStandardRateExists.HourId = item.HourId;
                            isStandardRateExists.GstAmount = 0;
                            _context.RoomRateMaster.Update(isStandardRateExists);
                            
                        }
                    }
                    else if(item.RateType == Constants.Constants.Hour)
                    {
                        var isStandardRateExists = await _context.RoomRateMaster.FirstOrDefaultAsync(x => x.RoomTypeId == item.RoomTypeId && x.IsActive == true && x.CompanyId == companyId && x.HourId == item.HourId);
                        if (isStandardRateExists == null)
                        {

                            var roomRateMaster = new RoomRateMaster();
                            roomRateMaster.RoomTypeId = item.RoomTypeId;
                            roomRateMaster.RoomRate = item.RoomRate;
                            roomRateMaster.Gst = item.Gst;
                            roomRateMaster.Discount = item.Discount;
                            roomRateMaster.IsActive = true;
                            roomRateMaster.CreatedDate = currentDate;
                            roomRateMaster.UpdatedDate = currentDate;
                            roomRateMaster.UserId = userId;
                            roomRateMaster.CompanyId = companyId;
                            roomRateMaster.GstTaxType = item.GstTaxType;
                            roomRateMaster.GstAmount = 0;
                            roomRateMaster.HourId = item.HourId;
                            roomRateMaster.RatePriority =  Constants.Constants.LowPrority;
                            await _context.RoomRateMaster.AddAsync(roomRateMaster);
                           
                        }
                        else
                        {
                            isStandardRateExists.RoomRate = item.RoomRate;
                            isStandardRateExists.Gst = item.Gst;
                            isStandardRateExists.Discount = item.Discount;
                            isStandardRateExists.UpdatedDate = currentDate;
                            isStandardRateExists.GstTaxType = item.GstTaxType;
                            isStandardRateExists.HourId = item.HourId;
                            isStandardRateExists.GstAmount = 0;
                            _context.RoomRateMaster.Update(isStandardRateExists);
                           
                        }
                    }
                    else if (item.RateType == Constants.Constants.Custom)
                    {

                        var isRateExists = await _context.RoomRateDateWise.Where(x => x.RoomTypeId == item.RoomTypeId && x.IsActive == true && (x.RateType == Constants.Constants.Custom || x.RateType == Constants.Constants.Weekend) && x.CompanyId == companyId && ((item.FromDate >= x.FromDate && item.FromDate <= x.ToDate) ||
                           (item.ToDate >= x.FromDate && item.ToDate <= x.ToDate) ||
                           (x.FromDate >= item.FromDate && x.FromDate <= item.ToDate) ||
                           (x.ToDate >= item.FromDate && x.ToDate <= item.ToDate))).ToListAsync();
                        if (isRateExists.Count > 0) 
                        { 
                            foreach(var data in isRateExists)
                            {
                                data.IsActive = false;
                                data.UpdatedDate = currentDate;
                                _context.RoomRateDateWise.Update(data);
                            }
                        }

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
                            roomrateDateWise.CreatedDate = currentDate;
                            roomrateDateWise.UpdatedDate = currentDate;
                            roomrateDateWise.UserId = userId;
                            roomrateDateWise.CompanyId = companyId;
                            roomrateDateWise.GstTaxType = item.GstTaxType;
                            roomrateDateWise.GstAmount = 0;
                            roomrateDateWise.RatePriority = Constants.Constants.MediumPrority;
                            await _context.RoomRateDateWise.AddAsync(roomrateDateWise);
                            
                        }
                        else
                        {
                            isCustomRateExists.RoomRate = item.RoomRate;
                            isCustomRateExists.Gst = item.Gst;
                            isCustomRateExists.Discount = item.Discount;
                            isCustomRateExists.UpdatedDate = currentDate;
                            isCustomRateExists.GstAmount = 0;
                            isCustomRateExists.GstTaxType = item.GstTaxType;
                            _context.RoomRateDateWise.Update(isCustomRateExists);
                           
                        }
                    }

                    else if (item.RateType == Constants.Constants.Weekend)
                    {
                        DayOfWeek currentDay = GetDayName(item.WeekendDay);
                        List<DateOnly> dayDates = GetDayDates(item.FromDate ?? Constants.Constants.DefaultDate, item.ToDate ?? Constants.Constants.DefaultDate
                            , currentDay);

                        foreach (var date in dayDates)
                        {
                            var isRateExists = await _context.RoomRateDateWise.Where(x => x.RoomTypeId == item.RoomTypeId && x.IsActive == true && ( x.RateType == Constants.Constants.Weekend || x.RateType == Constants.Constants.Custom) && x.CompanyId == companyId && (item.FromDate >= date || item.FromDate <= date)).ToListAsync();
                            if (isRateExists.Count > 0)
                            {
                                foreach (var data in isRateExists)
                                {
                                    data.IsActive = false;
                                    data.UpdatedDate = currentDate;
                                    _context.RoomRateDateWise.Update(data);
                                }
                            }

                            var isWeekendRateExists = await _context.RoomRateDateWise.FirstOrDefaultAsync(x => x.RoomTypeId == item.RoomTypeId && x.FromDate == date && x.ToDate == date && x.IsActive == true && x.RateType == Constants.Constants.Weekend && x.CompanyId == companyId);
                            if (isWeekendRateExists == null)
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
                                roomrateDateWise.CreatedDate = currentDate;
                                roomrateDateWise.UpdatedDate = currentDate;
                                roomrateDateWise.UserId = userId;
                                roomrateDateWise.CompanyId = companyId;
                                roomrateDateWise.GstTaxType = item.GstTaxType;
                                roomrateDateWise.GstAmount = 0;
                                roomrateDateWise.RatePriority = Constants.Constants.HighPrority;
                                await _context.RoomRateDateWise.AddAsync(roomrateDateWise);
                                
                            }
                            else
                            {
                                isWeekendRateExists.RoomRate = item.RoomRate;
                                isWeekendRateExists.Gst = item.Gst;
                                isWeekendRateExists.Discount = item.Discount;
                                isWeekendRateExists.UpdatedDate = currentDate;
                                isWeekendRateExists.GstTaxType = item.GstTaxType;
                                isWeekendRateExists.GstAmount = 0;
                                _context.RoomRateDateWise.Update(isWeekendRateExists);
                               
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
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

        private List<DateOnly> GetDayDates(DateOnly startDate, DateOnly endDate, DayOfWeek weekDay)
        {
            if (startDate == Constants.Constants.DefaultDate || endDate == Constants.Constants.DefaultDate)
            {
                return new List<DateOnly>();
            }

            List<DateOnly> dates = new List<DateOnly>();

            // Find the first occurrence of the specified day of the week in the range
            DateOnly currentDate = startDate;

            while (currentDate.DayOfWeek != weekDay)
            {
                currentDate = currentDate.AddDays(1);
                if (currentDate > endDate)
                {
                    return dates;
                }
            }

            // Add all matching days within the range
            while (currentDate <= endDate)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(7);
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
    .Where(x => x.IsActive && x.RoomTypeId == isRateExists.RoomTypeId).OrderBy(x => x.FromDate)
    .Select(x => new 
    {
        Id = x.Id,
        RoomTypeId = x.RoomTypeId,
        RoomRate = x.RoomRate,
        Gst = x.Gst,
        Discount = x.Discount,
        FromDate = x.FromDate,
        ToDate = x.ToDate,
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
                                       join h in _context.HourMaster on rate.HourId equals h.Id into hours
                                       from hour in hours.DefaultIfEmpty()
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
                                           HourId = hour == null ? 0 : hour.Id,
                                           Hour = hour == null ? 0 :  hour.Hour
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
                return Ok(new { Code = 200, Message = "Room rate deleted successfully" });
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

                var documents = await _context.DocumentMaster.Where(x => x.IsActive == true && x.CompanyId == companyId && x.FinancialYear == financialYear).Select(x => new
                {
                    x.DocId,
                    x.Type,
                    x.Prefix, 
                    x.Prefix1,
                    x.Prefix2,
                    x.Suffix,
                    x.Separator,
                    DocumentNo = x.Prefix + x.Separator + x.Prefix1 + x.Separator + x.Prefix2 + x.Suffix + x.Number + x.LastNumber
            }).ToListAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }
                }
                document.UpdatedDate = DateTime.Now;
                

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Document updated successfully" });
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

                SetMastersDefault(agentDetails, companyId, userId);
                var validator = new AgentDetailValidator(_context);
                var result = await validator.ValidateAsync(agentDetails);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }


                }

                if (file != null)
                {
                    
                    agentDetails.ContractFile = await Constants.Constants.AddFile(file, "Uploads/ContractFiles");

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
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }
                }

                agent.UpdatedDate = DateTime.Now;
                if (file != null)
                {
                    agent.ContractFile = await Constants.Constants.AddFile(file, "Uploads/ContractFiles");

                }

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Agent updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        //[HttpPost("UpdateRoomDetails")] 
        //public async Task<IActionResult> UpdateRoomDetails([FromBody] )


    }
}
