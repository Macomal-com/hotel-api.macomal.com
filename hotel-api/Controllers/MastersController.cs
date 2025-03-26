using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.Models;
using RepositoryModels.Repository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data;

namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MastersController(DbContextSql context, IMapper mapper) : ControllerBase
    {
        private readonly DbContextSql _context = context;
        private readonly IMapper _mapper = mapper;

        private static void SetClusterDefaults(ICommonParams model, int companyid, int userId)
        {
            model.CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            model.UpdatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            model.IsActive = true;
            model.CompanyId = companyid;
            model.UserId = userId;
        }

        private static void SetMastersDefault(ICommonProperties model, int companyid, int userId)
        {
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            model.IsActive = true;
            model.CompanyId = companyid;
            model.UserId = userId;
        }



        //-----------------------------
        //GET APIS
        //-----------------------------

        //CLUSTER APIS
        [HttpGet("GetClusterMaster")]
        public async Task<IActionResult> GetClusterMaster()
        {
            try
            {
                var data = await _context.ClusterMaster.Where(bm => bm.IsActive).ToListAsync();

                return Ok(new { Code = 200, Message = "Cluster details fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetClusterById/{id}")]
        public async Task<IActionResult> GetClusterById(int id)
        {
            try
            {
                var data = await _context.ClusterMaster
                          .Where(x => x.ClusterId == id && x.IsActive == true).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Cluster not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Cluster details fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("AddClusterMaster")]
        public async Task<IActionResult> AddClusterMaster([FromBody] ClusterDTO clusterMaster)
        {
            if (clusterMaster == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<ClusterMaster>(clusterMaster);

                var validator = new ClusterValidator(_context);
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

                await _context.ClusterMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Cluster Created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchCluster/{id}")]
        public async Task<IActionResult> PatchCluster(int id, [FromBody] JsonPatchDocument<ClusterMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var cluster = await _context.ClusterMaster.FindAsync(id);

            if (cluster == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(cluster, ModelState);

            var validator = new ClusterValidator(_context);
            var result = await validator.ValidateAsync(cluster);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    Field = x.PropertyName
                }).ToList();
                return Ok(new { Code = 202, Message = errors });
            }


            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                                    .Where(x => x.Value.Errors.Any())
                                    .SelectMany(x => x.Value.Errors)
                                    .Select(x => x.ErrorMessage)
                                    .ToList();
                return Ok(new { Code = 500, Message = errorMessages });
            }
            cluster.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Cluster updated successfully" });
        }


        //LANDLORD APIS
        [HttpGet("GetLandlordDetails")]
        public async Task<IActionResult> GetLandlordDetails()
        {
            try
            {
                var data = await _context.LandlordDetails.Where(bm => bm.IsActive).ToListAsync();

                return Ok(new { Code = 200, Message = "Landlords fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetLandlordById/{id}")]
        public async Task<IActionResult> GetLandlordById(int id)
        {
            try
            {
                var data = await _context.LandlordDetails
                          .Where(x => x.LandlordId == id && x.IsActive == true).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Landlord not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Landlord fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPatch("PatchLandlordDetails/{id}")]
        public async Task<IActionResult> PatchLandlordDetails(int id, [FromBody] JsonPatchDocument<LandlordDetails> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var landlord = await _context.LandlordDetails.FindAsync(id);

                if (landlord == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(landlord, ModelState);
                var validator = new LandlordValidator(_context);
                var result = await validator.ValidateAsync(landlord);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, Message = errors });
                }
                landlord.UpdatedDate = DateTime.Now;
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

                return Ok(new { Code = 200, Message = "Landlord updated successfully" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }


        [HttpPost("AddLandlordDetails")]
        public async Task<IActionResult> AddLandlordDetails([FromBody] LandlordDetailsDTO landlord)
        {
            if (landlord == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<LandlordDetails>(landlord);

                var validator = new LandlordValidator(_context);
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

                await _context.LandlordDetails.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Landlord created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        //BUILDING MASTER
        [HttpGet("GetBuildingMaster")]
        public async Task<IActionResult> GetBuildingMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await (from rs in _context.BuildingMaster
                                 
                                  where rs.IsActive == true && rs.PropertyId == companyId 
                                  select new
                                  {
                                      rs.BuildingId,
                                      rs.BuildingName,
                                      rs.BuildingDescription,
                                      rs.NoOfFloors,
                                      rs.NoOfRooms,
                                     
                                  }).ToListAsync();
                return Ok(new { Code = 200, Message = "Buildings fetched successfully", Data = data });


            }

            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetBuildingById/{id}")]
        public async Task<IActionResult> GetBuildingById(int id)
        {
            try
            {
                var data = await _context.BuildingMaster.FirstOrDefaultAsync(x=>x.IsActive == true && x.BuildingId == id);


                return data == null
                    ? NotFound(new { Code = 404, Message = "Building not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Building fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddBuildingMaster")]
        public async Task<IActionResult> AddBuildingMaster([FromBody] BuildingMasterDTO buildingMaster)
        {
            if (buildingMaster == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<BuildingMaster>(buildingMaster);

                var validator = new BuildingMasterValidator(_context);
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

                await _context.BuildingMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Building created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchBuildingMaster/{id}")]
        public async Task<IActionResult> PatchBuildingMaster(int id, [FromBody] JsonPatchDocument<BuildingMaster> patchDocument)
        {
            try { 
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var building = await _context.BuildingMaster.FindAsync(id);

            if (building == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(building, ModelState);
            var validator = new BuildingMasterValidator(_context);
            var result = await validator.ValidateAsync(building);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    Field = x.PropertyName
                }).ToList();
                return Ok(new { Code = 202, Message = errors });
            }

            building.UpdatedDate = DateTime.Now;
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

            return Ok(new { Code = 200, Message = "Building updated successfully" });
        }
            catch(Exception ex){
            
return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage
    });
    }
        }

        //FLOORs
        [HttpGet("GetFloorMaster")]
        public async Task<IActionResult> GetFloorMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await (from floor in _context.FloorMaster
                                  join building in _context.BuildingMaster on floor.BuildingId equals building.BuildingId into floorBuilding
                                  from buildingMas in floorBuilding.DefaultIfEmpty()
                                  join prop in _context.CompanyDetails on floor.PropertyId equals prop.PropertyId
                                  where floor.IsActive == true && floor.PropertyId == companyId
                                  select new
                                  {
                                      FloorId = floor.FloorId,
                                      FloorNumber = floor.FloorNumber,
                                      NoOfRooms = floor.NoOfRooms,
                                      BuildingName = buildingMas.BuildingName,

                                  }).ToListAsync();

                return Ok(new { Code = 200, Message = "Floors fetched successfully", Data = data });
            }
            
            catch (Exception ex)
            {

                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetFloorMasterById/{id}")]
        public async Task<IActionResult> GetFloorMasterById(int id)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await (from floor in _context.FloorMaster
                                  join building in _context.BuildingMaster on floor.BuildingId equals building.BuildingId into floorBuilding
                                  from buildingMas in floorBuilding.DefaultIfEmpty()
                                  join prop in _context.CompanyDetails on floor.PropertyId equals prop.PropertyId
                                  where floor.IsActive == true && floor.PropertyId == companyId && floor.FloorId == id
                                  select new
                                  {
                                      FloorId = floor.FloorId,
                                      FloorNumber = floor.FloorNumber,
                                      NoOfRooms = floor.NoOfRooms,
                                      BuildingId = floor.BuildingId ?? 0,
                                      PropertyId = floor.PropertyId,
                                      BuildingName = buildingMas.BuildingName,
                                      BuildingObj = new
                                      {
                                          Label = buildingMas.BuildingName,
                                          Value = floor.BuildingId ?? 0,
                                      }

                                  }).FirstOrDefaultAsync();

                return Ok(new { Code = 200, Message = "Floors fetched successfully", Data = data });
            }

            catch (Exception ex)
            {

                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddFloorMaster")]
        public async Task<IActionResult> AddFloorMaster([FromBody] FloorMasterDTO floorMaster)
        {
            if (floorMaster == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });


            try
            {
                
               

                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<FloorMaster>(floorMaster);
                cm.BuildingId = cm.BuildingId == 0 ? null : cm.BuildingId;
                var validator = new FloorValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var error = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        PropertyName = x.PropertyName
                    }).ToList();

                    return Ok(new { Code = 400, Message = error });
                }

                SetMastersDefault(cm, companyId, userId);

                await _context.FloorMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Floor created successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchFloorMaster/{id}")]
        public async Task<IActionResult> PatchFloorMaster(int id, [FromBody] JsonPatchDocument<FloorMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var floor = await _context.FloorMaster.FindAsync(id);

            if (floor == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(floor, ModelState);
            var validator = new FloorValidator(_context);
            var result = await validator.ValidateAsync(floor);
            if (!result.IsValid)
            {
                var error = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    PropertyName = x.PropertyName
                }).ToList();

                return Ok(new { Code = 400, Message = error });
            }
            floor.BuildingId = floor.BuildingId == 0 ? null : floor.BuildingId;
            floor.UpdatedDate = DateTime.Now;
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

            return Ok(new { Code = 200, Message = "Floor updated successfully" });
        }





        // GROUPS
        [HttpGet("GetGroup")]
        public async Task<IActionResult> GetGroup()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.GroupMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Group not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Group fetched successfully", Data = data });
            }
           
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetGroupById/{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            try
            {
                var data = await _context.GroupMaster
                          .Where(x => x.Id == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Group not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Group fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddGroup")]
        public async Task<IActionResult> AddGroup([FromBody] GroupMasterDTO group)
        {
            if (group == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });
            
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<GroupMaster>(group);
                SetMastersDefault(cm, companyId, userId);
                var validator = new GroupValidator(_context);

                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }

                

                await _context.GroupMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Group created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchGroup/{id}")]
        public async Task<IActionResult> PatchGroup(int id, [FromBody] JsonPatchDocument<GroupMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var group = await _context.GroupMaster.FindAsync(id);

                if (group == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(group, ModelState);

                var validator = new GroupValidator(_context);
                var result = await validator.ValidateAsync(group);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }

                group.UpdatedDate = DateTime.Now;
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

                return Ok(new { Code = 200, Message = "Group updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }




        //BED TYPES
        [HttpGet("GetBedTypes")]
        public async Task<IActionResult> GetBedTypes()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.BedTypeMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Bed Types not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Bed Types fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetBedTypeById/{id}")]
        public async Task<IActionResult> GetBedTypeById(int id)
        {
            try
            {
                var data = await _context.BedTypeMaster
                          .Where(x => x.BedTypeId == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Bed Type not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Bed Type fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddBedTypeMaster")]
        public async Task<IActionResult> AddBedTypeMaster([FromBody] BedTypeMasterDTO bedtypeMaster)
        {
            if (bedtypeMaster == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<BedTypeMaster>(bedtypeMaster);
                SetMastersDefault(cm, companyId, userId);
                var validator = new BedTypeValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }
                _context.BedTypeMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Bed Type created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchBedTypeMaster/{id}")]
        public async Task<IActionResult> PatchBedTypeMaster(int id, [FromBody] JsonPatchDocument<BedTypeMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var bedType = await _context.BedTypeMaster.FindAsync(id);

            if (bedType == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(bedType, ModelState);
            var validator = new BedTypeValidator(_context);

            var result = await validator.ValidateAsync(bedType);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    Field = x.PropertyName
                }).ToList();
                return Ok(new { Code = 202, message = errors });
            }
            bedType.UpdatedDate = DateTime.Now;
            

            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Bed Type updated successfully" });
        }



        //SUB GROUP
        [HttpGet("GetSubGroup")]
        public async Task<IActionResult> GetSubGroup()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await (from rs in _context.SubGroupMaster
                                  join gm in _context.GroupMaster on rs.GroupId equals gm.Id
                                  where rs.IsActive == true && gm.IsActive == true &&
                                  rs.CompanyId == companyId && rs.UserId == userId
                                  select new
                                  {
                                      rs.SubGroupId,
                                      rs.SubGroupName,
                                      rs.Description,
                                      rs.GroupId,
                                      gm.GroupName
                                  }).ToListAsync();
                if (data.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "Sub Group not found", Data = Array.Empty<object>() });
                }
                return Ok(new { Code = 200, Message = "Sub Group fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddSubGroup")]
        public async Task<IActionResult> AddSubGroup([FromBody] SubGroupMasterDTO group)
        {
            if (group == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });
            
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<SubGroupMaster>(group);
                SetMastersDefault(cm, companyId, userId);
               
                var validator = new SubGroupValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }
                _context.SubGroupMaster.Add(cm);
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "SubGroup created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetSubGroupById/{Id}")]
        public async Task<IActionResult> GetSubGroupById(int id)
        {
            try
            {
                var data = await _context.SubGroupMaster
                          .Where(x => x.SubGroupId == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Sub Group not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Sub Group fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchSubGroup/{id}")]
        public async Task<IActionResult> PatchSubGroup(int id, [FromBody] JsonPatchDocument<SubGroupMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var group = await _context.SubGroupMaster.FindAsync(id);

            if (group == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(group, ModelState);
            var validator = new SubGroupValidator(_context);
            var result = await validator.ValidateAsync(group);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    Field = x.PropertyName
                }).ToList();
                return Ok(new { Code = 202, Message = errors });
            }
            group.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "SubGroup updated successfully" });
        }



        //SERVICE MASTER
        [HttpGet("GetServicableMaster")]
        public async Task<IActionResult> GetServicableMaster()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await (from rs in _context.ServicableMaster
                                  join gm in _context.GroupMaster on rs.GroupId equals gm.Id
                                  join sgm in _context.SubGroupMaster on rs.SubGroupId equals sgm.SubGroupId
                                  where rs.IsActive == true && rs.CompanyId == companyId && rs.UserId == userId
                                  && gm.IsActive == true && sgm.IsActive == true
                                  select new
                                  {
                                      rs.ServiceId,
                                      rs.ServiceName,
                                      rs.ServiceDescription,
                                      rs.GroupId,
                                      rs.SubGroupId,
                                      rs.Amount,
                                      rs.Discount,
                                      rs.TaxType,
                                      gm.GroupName,
                                      sgm.SubGroupName
                                  }).ToListAsync();               

                return Ok(new { Code = 200, Message = "Servicable Master fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddServicableMaster")]
        public async Task<IActionResult> AddServicableMaster([FromBody] ServicableMasterDTO service)
        {
            if (service == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });
            
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<ServicableMaster>(service);
                SetMastersDefault(cm, companyId, userId);

                var validator = new ServiveValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }

                _context.ServicableMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Service created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetServicableById/{Id}")]
        public async Task<IActionResult> GetServicableById(int id)
        {
            try
            {
                var data = await _context.ServicableMaster
                          .Where(x => x.ServiceId == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Service not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Services fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchServicableMaster/{id}")]
        public async Task<IActionResult> PatchServicableMaster(int id, [FromBody] JsonPatchDocument<ServicableMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var service = await _context.ServicableMaster.FindAsync(id);

            if (service == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(service, ModelState);
            var validator = new ServiveValidator(_context);
            var result = await validator.ValidateAsync(service);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    Field = x.PropertyName
                }).ToList();
                return Ok(new { Code = 202, Message = errors });
            }
            service.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Service updated successfully" });
        }


        //VENDOR MASTER
        [HttpGet("GetVendorMaster")]
        public async Task<IActionResult> GetVendorMaster()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await (from rs in _context.VendorMaster
                                  join gm in _context.VendorServiceMaster on rs.ServiceId equals gm.Id
                                  where rs.IsActive == true && gm.IsActive == true &&
                                  rs.CompanyId == companyId && rs.UserId == userId
                                  select new
                                  {
                                      rs.VendorId,
                                      rs.VendorName,
                                      rs.VendorEmail,
                                      rs.VendorPhone,
                                      rs.ServiceId,
                                      rs.CompanyName,
                                      gm.Name
                                  }).ToListAsync();
                if (data.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "Service not found", Data = Array.Empty<object>() });
                }
                return Ok(new { Code = 200, Message = "Service fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddVendorMaster")]
        public async Task<IActionResult> AddVendorMaster([FromBody] VendorMasterDTO vendor)
        {
            if (vendor == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<VendorMaster>(vendor);
                SetMastersDefault(cm, companyId, userId);

                var validator = new VendorValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }
                _context.VendorMaster.Add(cm);
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Vendor created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetVendorById/{Id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            try
            {
                var data = await _context.VendorMaster
                          .Where(x => x.VendorId == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Vendor not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Vendor fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchVendor/{id}")]
        public async Task<IActionResult> PatchVendor(int id, [FromBody] JsonPatchDocument<VendorMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var vendor = await _context.VendorMaster.FindAsync(id);

            if (vendor == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(vendor, ModelState);
            var validator = new VendorValidator(_context);
            var result = await validator.ValidateAsync(vendor);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    Field = x.PropertyName
                }).ToList();
                return Ok(new { Code = 202, Message = errors });
            }
            vendor.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Vendor updated successfully" });
        }



        //PAYMENT MODE
        [HttpGet("GetPaymentMode")]
        public async Task<IActionResult> GetPaymentMode()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.PaymentMode.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Payment Mode not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Payment Mode fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetPaymentModeById/{id}")]
        public async Task<IActionResult> GetPaymentModeById(int id)
        {
            try
            {
                var data = await _context.PaymentMode
                          .Where(x => x.PaymentId == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Payment Mode not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Payment Mode fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddPaymentMode")]
        public async Task<IActionResult> AddPaymentMode([FromBody] PaymentModeDTO pm)
        {
            if (pm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<PaymentMode>(pm);
                SetMastersDefault(cm, companyId, userId);
                var validator = new PaymentModeValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }
                _context.PaymentMode.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Payment Mode created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchPaymentMode/{id}")]
        public async Task<IActionResult> PatchPaymentMode(int id, [FromBody] JsonPatchDocument<PaymentMode> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var pm = await _context.PaymentMode.FindAsync(id);

            if (pm == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(pm, ModelState);
            var validator = new PaymentModeValidator(_context);

            var result = await validator.ValidateAsync(pm);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    Field = x.PropertyName
                }).ToList();
                return Ok(new { Code = 202, message = errors });
            }
            pm.UpdatedDate = DateTime.Now;


            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Payment Mode updated successfully" });
        }


        //STAFF MASTER
        [HttpGet("GetStaffMaster")]
        public async Task<IActionResult> GetStaffMaster()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await (from rs in _context.StaffManagementMaster
                                  join gm in _context.VendorMaster on rs.VendorId equals gm.VendorId into gmJoin
                                  from gm in gmJoin.DefaultIfEmpty() // Left join to include staff rows with VendorId = 0
                                  where rs.IsActive == true
                                        && (gm == null || gm.IsActive == true) // If there's no matching Vendor (gm is null), consider it as active
                                        && rs.CompanyId == companyId
                                        && rs.UserId == userId
                                        && (rs.VendorId == 0 || rs.VendorId == gm.VendorId) // Include rows where VendorId is 0 or matches
                                  select new
                                  {
                                      rs.StaffId,
                                      rs.StaffName,
                                      rs.StaffRole,
                                      rs.PhoneNo,
                                      rs.Salary,
                                      rs.VendorId,
                                      VendorName = gm != null ? gm.VendorName : "No Vendor"
                                  }).ToListAsync();


                if (data.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "Staff not found", Data = Array.Empty<object>() });
                }
                return Ok(new { Code = 200, Message = "Staff fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddStaffMaster")]
        public async Task<IActionResult> AddStaffMaster([FromBody] StaffManagementMasterDTO sm)
        {
            if (sm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<StaffManagementMaster>(sm);
                SetMastersDefault(cm, companyId, userId);

                var validator = new StaffValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }
                _context.StaffManagementMaster.Add(cm);
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Staff created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetStaffById/{Id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            try
            {
                var data = await _context.StaffManagementMaster
                          .Where(x => x.StaffId == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Staff not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Staff fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchStaff/{id}")]
        public async Task<IActionResult> PatchStaff(int id, [FromBody] JsonPatchDocument<StaffManagementMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var sm = await _context.StaffManagementMaster.FindAsync(id);

            if (sm == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(sm, ModelState);
            var validator = new StaffValidator(_context);
            var result = await validator.ValidateAsync(sm);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(x => new
                {
                    Error = x.ErrorMessage,
                    Field = x.PropertyName
                }).ToList();
                return Ok(new { Code = 202, Message = errors });
            }
            sm.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Staff updated successfully" });
        }



        //GST MASTER
        [HttpGet("GetGstMaster")]
        public async Task<IActionResult> GetGstMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.GstMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetGstById/{id}")]
        public async Task<IActionResult> GetGstById(int id)
        {
            try
            {
                var data = await _context.GstMaster
                          .Where(x => x.Id == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddGstMaster")]
        public async Task<IActionResult> AddGstMaster([FromBody] GstMasterDTO gm)
        {
            if (gm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<GstMaster>(gm);
                SetMastersDefault(cm, companyId, userId);
                var validator = new GstValidator(_context);

                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }



                await _context.GstMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Gst created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchGstMaster/{id}")]
        public async Task<IActionResult> PatchGstMaster(int id, [FromBody] JsonPatchDocument<GstMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var gm = await _context.GstMaster.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(gm, ModelState);

                var validator = new GstValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }

                gm.UpdatedDate = DateTime.Now;
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

                return Ok(new { Code = 200, Message = "Gst updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }


        //COMMISSION MASTER 
        [HttpGet("GetCommissionMaster")]
        public async Task<IActionResult> GetCommissionMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.CommissionMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetCommissionById/{id}")]
        public async Task<IActionResult> GetCommissionById(int id)
        {
            try
            {
                var data = await _context.CommissionMaster
                          .Where(x => x.Id == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddCommissionMaster")]
        public async Task<IActionResult> AddCommissionMaster([FromBody] CommissionMasterDTO gm)
        {
            if (gm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<CommissionMaster>(gm);
                SetMastersDefault(cm, companyId, userId);
                var validator = new CommissionValidator(_context);

                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }



                await _context.CommissionMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Commission created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchCommissionMaster/{id}")]
        public async Task<IActionResult> PatchCommissionMaster(int id, [FromBody] JsonPatchDocument<CommissionMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var gm = await _context.CommissionMaster.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(gm, ModelState);

                var validator = new CommissionValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }

                gm.UpdatedDate = DateTime.Now;
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

                return Ok(new { Code = 200, Message = "Commission updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }

        //USER CREATION
        [HttpGet("GetUserMaster")]
        public async Task<IActionResult> GetUserMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.UserDetails.Where(bm => bm.IsActive == true).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var data = await _context.UserDetails
                          .Where(x => x.IsActive == true && x.UserId == id).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddUserMaster")]
        public async Task<IActionResult> AddUserMaster([FromBody] UserDetailsDTO gm)
        {
            if (gm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string dbName = Convert.ToString(HttpContext.Request.Headers["Database"]);

                var cm = _mapper.Map<UserDetails>(gm);
                cm.CreatedDate = DateTime.Now;
                cm.ModifyDate = DateTime.Now;
                cm.IsActive = true;
                cm.CompanyId = companyId;
                var validator = new UserValidator(_context);

                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }
                cm.DBName = dbName;
                cm.BranchId = 0;
                cm.CreatedDate = DateTime.Now;
                cm.ModifyDate = DateTime.Now;
                cm.CreatedBy = 0;
                cm.City = 0;
                cm.Other1 = 0;
                cm.Other2 = 0;
                await _context.UserDetails.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchUserMaster/{id}")]
        public async Task<IActionResult> PatchUserMaster(int id, [FromBody] JsonPatchDocument<UserDetails> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var gm = await _context.UserDetails.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(gm, ModelState);

                var validator = new UserValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }

                gm.ModifyDate = DateTime.Now;
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

                return Ok(new { Code = 200, Message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }




        //VENDOR SERVICE MASTER
        [HttpGet("GetVendorService")]
        public async Task<IActionResult> GetVendorService()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.VendorServiceMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Vendor Service not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Vendor Service fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetVendorServiceById/{id}")]
        public async Task<IActionResult> GetVendorServiceById(int id)
        {
            try
            {
                var data = await _context.VendorServiceMaster
                          .Where(x => x.Id == id && x.IsActive).FirstOrDefaultAsync();

                return data == null
                    ? NotFound(new { Code = 404, Message = "Vendor Service not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Vendor Service fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddVendorService")]
        public async Task<IActionResult> AddVendorService([FromBody] VendorServiceMasterDTO vs)
        {
            if (vs == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<VendorServiceMaster>(vs);
                SetMastersDefault(cm, companyId, userId);
                var validator = new VendorServiceValidator(_context);

                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }



                await _context.VendorServiceMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Vendor Service created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchVendorService/{id}")]
        public async Task<IActionResult> PatchVendorService(int id, [FromBody] JsonPatchDocument<VendorServiceMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var vs = await _context.VendorServiceMaster.FindAsync(id);

                if (vs == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(vs, ModelState);

                var validator = new VendorServiceValidator(_context);
                var result = await validator.ValidateAsync(vs);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 202, message = errors });
                }

                vs.UpdatedDate = DateTime.Now;
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

                return Ok(new { Code = 200, Message = "Vendor Service updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }








        [HttpGet("GetCompanyDetails")]
        public async Task<IActionResult> GetCompanyDetails()
        {
            try
            {
                var data = await _context.CompanyDetails.Where(bm => bm.IsActive).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Company details not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Company details fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {                 
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddCompanyDetails")]
        public async Task<IActionResult> AddCompanyDetails([FromBody] CompanyDetailsDTO companyDetails)
        {
            if (companyDetails == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<CompanyDetails>(companyDetails);
                SetClusterDefaults(cm, companyId, userId);

                _context.CompanyDetails.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Company created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetOwnerMaster")]
        public async Task<IActionResult> GetOwnerMaster()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.OwnerMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Owners not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Owners fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetRoomRateMaster")]
        public async Task<IActionResult> GetRoomRateMaster()
        {
            try
            {
                var data = await _context.RoomRateMaster.Where(bm => bm.IsActive).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Room Rates not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Room Rates fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var data = await _context.UserCreation.Where(uc=>uc.IsActive).ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "User not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Users fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        

        

        //-----------------------------
        //GET BY ID APIS
        //-----------------------------

        [HttpGet("GetCompanyById")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            try
            {
                var data = await _context.CompanyDetails
                          .Where(x => x.PropertyId == id && x.IsActive).FirstOrDefaultAsync();

                return data != null
                    ? Ok(new { Code = 200, Message = "Company details fetched successfully", Data = data })
                    : NotFound(new { Code = 404, Message = "Company not found", Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        

        

        

        [HttpGet("GetFloorById")]
        public async Task<IActionResult> GetFloorById(int id)
        {
            try
            {
                var data = await _context.FloorMaster
                          .Where(x => x.FloorId == id && x.IsActive).FirstOrDefaultAsync();

                return data != null
                    ? NotFound(new { Code = 404, Message = "Floor not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Floor fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        

        [HttpGet("GetOwnerById")]
        public async Task<IActionResult> GetOwnerById(int id)
        {
            try
            {
                var data = await _context.OwnerMaster
                          .Where(x => x.OwnerId == id && x.IsActive).FirstOrDefaultAsync();

                return data != null
                    ? NotFound(new { Code = 404, Message = "Owner not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Owner fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

      

       
        [HttpGet("GetRoomRateById")]
        public async Task<IActionResult> GetRoomRateById(int id)
        {
            try
            {
                var data = await _context.RoomRateMaster
                          .Where(x => x.RoomRateId == id && x.IsActive).FirstOrDefaultAsync();

                return data != null
                    ? NotFound(new { Code = 404, Message = "Room Rate not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Room Rate fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

       

        

        //-----------------------------
        //PATCH APIS
        //-----------------------------

        

        [HttpPatch("PatchCompanyMaster/{id}")]
        public async Task<IActionResult> PatchCompanyMaster(int id, [FromBody] JsonPatchDocument<CompanyDetails> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var company = await _context.CompanyDetails.Where(x => x.IsActive && x.PropertyId == id).FirstOrDefaultAsync();


            if (company == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(company, ModelState);
            company.UpdatedDate = DateTime.Now.ToString();
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

            return Ok(new { Code = 200, Message = "Company details updated successfully" });
        }

       

        

     

        [HttpPatch("PatchOwnerMaster/{id}")]
        public async Task<IActionResult> PatchOwnerMaster(int id, [FromBody] JsonPatchDocument<OwnerMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var owner = await _context.OwnerMaster.FindAsync(id);

            if (owner == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(owner, ModelState);
            owner.UpdatedDate = DateTime.Now;
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

            return Ok(new { Code = 200, Message = "Owner updated successfully" });
        }

       
        
        [HttpPatch("PatchRoomRateMaster/{id}")]
        public async Task<IActionResult> PatchRoomRateMaster(int id, [FromBody] JsonPatchDocument<RoomRateMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var room = await _context.RoomRateMaster.FindAsync(id);

            if (room == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(room, ModelState);
            room.UpdatedDate = DateTime.Now.ToString();
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

            return Ok(new { Code = 200, Message = "Room Rate updated successfully" });
        }

        [HttpPatch("PatchUser/{id}")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] JsonPatchDocument<UserCreation> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var vendor = await _context.UserCreation.FindAsync(id);

            if (vendor == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(vendor, ModelState);
            vendor.UpdatedDate = DateTime.Now;
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

            return Ok(new { Code = 200, Message = "User updated successfully" });
        }

        

        

        //-----------------------------
        //POST APIS
        //-----------------------------

        

       

        

       

        
        
        [HttpPost("AddOwnerMaster")]
        public async Task<IActionResult> AddOwnerMaster([FromBody] OwnerMasterDTO owner)
        {
            if (owner == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<OwnerMaster>(owner);
                SetMastersDefault(cm, companyId, userId);

                _context.OwnerMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Owner created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        
        
        [HttpPost("AddRoomRateMaster")]
        public async Task<IActionResult> AddRoomRateMaster([FromBody] RoomRateMasterDTO room)
        {
            if (room == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<RoomRateMaster>(room);
                SetClusterDefaults(cm, companyId, userId);

                _context.RoomRateMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room Rate created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] UserCreationDTO user)
        {
            if (user == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<UserCreation>(user);
                SetMastersDefault(cm, companyId, userId);

                _context.UserCreation.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }
    }
}
