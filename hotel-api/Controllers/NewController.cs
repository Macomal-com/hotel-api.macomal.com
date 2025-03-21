using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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


    }
}
