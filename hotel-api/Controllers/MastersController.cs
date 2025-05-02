using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Repository.Models;
using RepositoryModels.Repository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data;
using System.Xml.XPath;
using hotel_api.GeneralMethods;
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
                    ? Ok(new { Code = 404, Message = "Cluster not found", Data = Array.Empty<object>() })
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
                    ? Ok(new { Code = 404, Message = "Landlord not found", Data = Array.Empty<object>() })
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

                                  where rs.IsActive == true && rs.PropertyId == companyId && rs.UserId == userId
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.BuildingMaster.FirstOrDefaultAsync(x => x.IsActive == true && x.BuildingId == id && x.CompanyId == companyId && x.UserId == userId);


                return data == null
                    ? Ok(new { Code = 404, Message = "Building not found", Data = Array.Empty<object>() })
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
            try
            {
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
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    Code = 500,
                    Message = Constants.Constants.ErrorMessage
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
                                  where floor.IsActive == true && floor.PropertyId == companyId && floor.UserId == userId
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
                                  where floor.IsActive == true && floor.PropertyId == companyId && floor.UserId == userId && floor.FloorId == id
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

                var data = await _context.GroupMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Group not found", Data = Array.Empty<object>() });
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.GroupMaster
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Group not found", Data = Array.Empty<object>() })
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
                cm.IGST = cm.GST;
                cm.SGST = cm.GST / 2;
                cm.CGST = cm.GST / 2;
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
                if (group.IsActive == false)
                {
                    var validator = new GroupDeleteValidator(_context);

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

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Group deleted successfully" });
                }
                else
                {
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

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Group updated successfully" });
                }
                

               
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
                var data = await _context.BedTypeMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Bed Types not found", Data = Array.Empty<object>() });
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.BedTypeMaster
                          .Where(x => x.BedTypeId == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Bed Type not found", Data = Array.Empty<object>() })
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.SubGroupMaster
                          .Where(x => x.SubGroupId == id && x.IsActive && x.UserId == userId && x.CompanyId == companyId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Sub Group not found", Data = Array.Empty<object>() })
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.ServicableMaster
                          .Where(x => x.ServiceId == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Service not found", Data = Array.Empty<object>() })
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.VendorMaster
                          .Where(x => x.VendorId == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Vendor not found", Data = Array.Empty<object>() })
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
                var data = await _context.PaymentMode.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Payment Mode not found", Data = Array.Empty<object>() });
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.PaymentMode
                          .Where(x => x.PaymentId == id && x.IsActive && x.UserId == userId && x.CompanyId == companyId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Payment Mode not found", Data = Array.Empty<object>() })
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.StaffManagementMaster
                          .Where(x => x.StaffId == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Staff not found", Data = Array.Empty<object>() })
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

                var data = await _context.GstMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.GstMaster
                          .Where(x => x.Id == id && x.IsActive && x.UserId == userId && x.CompanyId == companyId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
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
            var transaction = _context.Database.BeginTransactionAsync();
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
                    await _context.Database.RollbackTransactionAsync();
                    return Ok(new { Code = 202, message = errors });

                }

                await _context.GstMaster.AddAsync(cm);
                await _context.SaveChangesAsync();


                if (cm.ranges.Count > 0)
                {
                    foreach (var item in cm.ranges)
                    {
                        if (item.TaxPercentage != 0)
                        {
                            SetMastersDefault(item, companyId, userId);
                            item.GstId = cm.Id;
                            await _context.GstRangeMaster.AddAsync(item);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                await _context.Database.CommitTransactionAsync();
                return Ok(new { Code = 200, Message = "Gst created successfully" });
            }
            catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
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

                var data = await _context.CommissionMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.CommissionMaster
                          .Where(x => x.Id == id && x.IsActive && x.UserId == userId && x.CompanyId == companyId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
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

                var data = await _context.UserDetails.Where(bm => bm.IsActive == true && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            try
            {
                var data = await _context.UserDetails
                          .Where(x => x.IsActive == true && x.UserId == id && x.CompanyId == companyId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
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

                var data = await _context.VendorServiceMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Vendor Service not found", Data = Array.Empty<object>() });
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.VendorServiceMaster
                          .Where(x => x.Id == id && x.IsActive && x.UserId == userId && x.CompanyId == companyId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Vendor Service not found", Data = Array.Empty<object>() })
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


        //PROPERTY MASTER
        [HttpGet("GetCompanyDetails")]
        public async Task<IActionResult> GetCompanyDetails()
        {
            try
            {
                var data = await _context.CompanyDetails.Where(bm => bm.IsActive).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Company details not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Company details fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddCompanyDetails")]
        public async Task<IActionResult> AddCompanyDetails([FromForm] CompanyDetailsDTO companyDetails, IFormFile[] files)
        {
            if (companyDetails == null)
                return Ok(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                    int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                    var cm = _mapper.Map<CompanyDetails>(companyDetails);
                    SetMastersDefault(cm, companyId, userId);

                    _context.CompanyDetails.Add(cm);
                    await _context.SaveChangesAsync();

                    var savedObject = await _context.CompanyDetails
                                                     .FirstOrDefaultAsync(c => c.CompanyName == cm.CompanyName);
                    if(savedObject == null)
                    {
                        return Ok(new { Code = 404, Message = "Data Not Found!" });
                    }
                    savedObject.GstType = Constants.PropertyConstants.GSTTYPE;
                    savedObject.IsCheckOutApplicable = Constants.PropertyConstants.ISCHECKOUTAPPLICABLE;
                    savedObject.CheckOutFormat = Constants.PropertyConstants.CHECKOUTFORMAT;
                    savedObject.IsRoomRateEditable = Constants.PropertyConstants.ISROOMRATEEDITABLE;
                    savedObject.CheckInTime = Constants.PropertyConstants.CHECKINTIME;
                    savedObject.CheckOutTime = Constants.PropertyConstants.CHECKOUTTIME;
                    savedObject.ApproveReservation = Constants.PropertyConstants.APPROVERESERVATION;
                    savedObject.CancelMethod = Constants.PropertyConstants.CANCELMETHOD;
                    savedObject.CancelCalculatedBy = Constants.PropertyConstants.CANCELCALCULATEBY;
                    savedObject.CheckOutInvoice = Constants.PropertyConstants.CHECKOUTINVOICE;
                    savedObject.IsWhatsappNotification = false;
                    savedObject.IsEmailNotification = false;
                    if (files != null || files?.Length != 0)
                    {
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        var propertyImages = new List<PropertyImages>();

                        foreach (var file in files)
                        {
                            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var propertyImage = new PropertyImages
                            {
                                PropertyId = savedObject.PropertyId,
                                FilePath = fileName
                            };
                            propertyImages.Add(propertyImage);
                        }

                        _context.PropertyImages.AddRange(propertyImages);
                    }

                    

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new { Code = 200, Message = "Property created successfully" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }
            }
        }

        [HttpGet("GetImagesById/{id}")]
        public async Task<IActionResult> GetImagesById(int id)
        {
            try
            {
                var data = await _context.PropertyImages.Where(bm => bm.PropertyId == id).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "No Images Found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Images fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetCompanyById/{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            try
            {
                var data = await _context.CompanyDetails
                          .Where(x => x.PropertyId == id && x.IsActive).FirstOrDefaultAsync();

                return data != null
                    ? Ok(new { Code = 200, Message = "Company details fetched successfully", Data = data })
                    : Ok(new { Code = 404, Message = "Company not found", Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchCompanyMaster/{id}")]
        public async Task<IActionResult> PatchCompanyMaster(int id, [FromForm] string patchDoc, IFormFile[]? files, IFormFile? logo)
        {
            if (patchDoc == "")
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }
            var patchDocument = JsonConvert.DeserializeObject<JsonPatchDocument<CompanyDetails>>(patchDoc);
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var company = await _context.CompanyDetails.Where(x => x.IsActive && x.PropertyId == id).FirstOrDefaultAsync();
                    if (company == null)
                    {
                        return Ok(new { Code = 404, Message = "Data Not Found" });
                    }
                    patchDocument?.ApplyTo(company, ModelState);
                    if (logo != null)
                    {
                        var uploadLogoPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads/logo");
                        if (!Directory.Exists(uploadLogoPath))
                        {
                            Directory.CreateDirectory(uploadLogoPath);
                        }
                        var fileName = $"{Guid.NewGuid()}_{logo.FileName}";
                        var filePath = Path.Combine(uploadLogoPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await logo.CopyToAsync(stream);
                        company.PropertyLogo = fileName.ToString();
                    }
                    company.UpdatedDate = DateTime.Now;
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

                    if (files != null || files.Length != 0)
                    {
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        var propertyImages = new List<PropertyImages>();

                        foreach (var file in files)
                        {
                            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var propertyImage = new PropertyImages
                            {
                                PropertyId = id,
                                FilePath = fileName
                            };
                            propertyImages.Add(propertyImage);
                        }

                        _context.PropertyImages.AddRange(propertyImages);

                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return Ok(new { Code = 200, Message = "Company created successfully" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }
            }
        }

       


        //HOUR MASTER
        [HttpGet("GetHourMaster")]
        public async Task<IActionResult> GetHourMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.HourMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetHourById/{id}")]
        public async Task<IActionResult> GetHourById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.HourMaster
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddHourMaster")]
        public async Task<IActionResult> AddHourMaster([FromBody] HourMasterDTO gm)
        {
            if (gm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<HourMaster>(gm);
                SetMastersDefault(cm, companyId, userId);
                var validator = new HourValidator(_context);

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



                await _context.HourMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Hour created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchHourMaster/{id}")]
        public async Task<IActionResult> PatchHourMaster(int id, [FromBody] JsonPatchDocument<HourMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var gm = await _context.HourMaster.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(gm, ModelState);

                var validator = new HourValidator(_context);
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

                return Ok(new { Code = 200, Message = "Hour updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }


        //PAX MASTER
        [HttpGet("GetPaxMaster")]
        public async Task<IActionResult> GetPaxMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.PaxMaster.Where(bm => bm.IsActive).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        //EXTRA POLICY MASTER
        [HttpGet("GetExtraPolicyMaster")]
        public async Task<IActionResult> GetExtraPolicyMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.ExtraPolicies.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetExtraPolicyById/{id}")]
        public async Task<IActionResult> GetExtraPolicyById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.ExtraPolicies
                          .Where(x => x.PolicyId == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddExtraPolicyMaster")]
        public async Task<IActionResult> AddExtraPolicyMaster([FromBody] ExtraPoliciesDTO gm)
        {
            if (gm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<ExtraPolicies>(gm);
                SetMastersDefault(cm, companyId, userId);
                var validator = new ExtrapolicyValidator(_context);

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



                await _context.ExtraPolicies.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Policy created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchPolicyMaster/{id}")]
        public async Task<IActionResult> PatchExtraPolicyMaster(int id, [FromBody] JsonPatchDocument<ExtraPolicies> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var gm = await _context.ExtraPolicies.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(gm, ModelState);

                var validator = new ExtrapolicyValidator(_context);
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

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Policy updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }


        //CANCEL POLICY MASTER
        [HttpGet("GetCancelPolicyMaster")]
        public async Task<IActionResult> GetCancelPolicyMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.CancelPolicyMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetPolicyNumber")]
        public async Task<IActionResult> GetPolicyNumber()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string financialYear = (HttpContext.Request.Headers["FinancialYear"]).ToString();

                var getbookingno = await _context.DocumentMaster.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Type == Constants.Constants.DocumentCancelPolicy && x.FinancialYear == financialYear && x.IsActive);

                if (getbookingno == null || getbookingno.Suffix == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found.", data = getbookingno });
                }
                var bookingno = getbookingno.Prefix + getbookingno.Separator + getbookingno.Prefix1 + getbookingno.Separator + getbookingno.Prefix2 + getbookingno.Suffix + getbookingno.Number + getbookingno.LastNumber;
                
                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = bookingno });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetCancelPolicyById/{id}")]
        public async Task<IActionResult> GetCancelPolicyById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.CancelPolicyMaster
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddCancelPolicyMaster")]
        public async Task<IActionResult> AddCancelPolicyMaster([FromBody] CancelPolicyMasterDTO gm)
        {
            if (gm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string  financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();

                var cm = _mapper.Map<CancelPolicyMaster>(gm);
                SetMastersDefault(cm, companyId, userId);
                var validator = new CancelPolicyValidator(_context);

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
                if(cm.CancellationTime == "Day")
                {
                    cm.FromTime = cm.FromTime * 24;
                    cm.ToTime = cm.ToTime * 24;
                }

                var response = await DocumentHelper.UpdateDocumentNo(_context, Constants.Constants.DocumentCancelPolicy, companyId, financialYear);
                if (response == null)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }
                await _context.CancelPolicyMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Policy created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchCancelPolicyMaster/{id}")]
        public async Task<IActionResult> PatchCancelPolicyMaster(int id, [FromBody] JsonPatchDocument<CancelPolicyMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var gm = await _context.CancelPolicyMaster.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(gm, ModelState);

                var validator = new CancelPolicyValidator(_context);
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

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Policy updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }


        //REMINDER MASTER
        [HttpGet("GetReminderMaster")]
        public async Task<IActionResult> GetReminderMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.ReminderMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetReminderById/{id}")]
        public async Task<IActionResult> GetReminderById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.ReminderMaster
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddReminderMaster")]
        public async Task<IActionResult> AddReminderMaster([FromBody] ReminderMasterDTO gm)
        {
            if (gm == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<ReminderMaster>(gm);
                SetMastersDefault(cm, companyId, userId);
                var validator = new ReminderValidator(_context);

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


                await _context.ReminderMaster.AddAsync(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Reminder created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchReminderMaster/{id}")]
        public async Task<IActionResult> PatchReminderMaster(int id, [FromBody] JsonPatchDocument<ReminderMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var gm = await _context.ReminderMaster.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(gm, ModelState);

                var validator = new ReminderValidator(_context);
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

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Reminder updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }



        //REMINDER HISTORY MASTER
        [HttpGet("GetReminderHistoryMaster")]
        public async Task<IActionResult> GetReminderHistoryMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                var data = await (from history in _context.ReminderHistoryMaster
                                  join reminder in _context.ReminderMaster on history.ReminderId equals reminder.Id
                                  where history.IsActive && history.CompanyId == companyId && history.UserId == userId
                                  select new
                                  {
                                      history.Id,
                                      reminder.ReminderType,
                                      history.DaysBefore,
                                      DueDate = history.DueDate.ToString("yyyy-MM-dd"),
                                      history.BillPaid,
                                      history.DocumentPath
                                  }).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetReminderHistoryById/{id}")]
        public async Task<IActionResult> GetReminderHistoryById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.ReminderHistoryMaster
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddReminderHistoryMaster")]
        public async Task<IActionResult> AddReminderHistoryMaster([FromForm] string formData, IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(formData))
                return Ok(new { Code = 400, Message = "Invalid Data" });

            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

            var cm = JsonConvert.DeserializeObject<ReminderHistoryMaster>(formData);

            if (cm == null)
                return Ok(new { Code = 400, Message = "Invalid Data" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                SetMastersDefault(cm, companyId, userId);

                // Handle file upload
                if (file != null)
                {
                    var documentPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "reminderDocuments");
                    Directory.CreateDirectory(documentPath);

                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(documentPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    cm.DocumentPath = fileName;
                }

                // Validate
                var validator = new ReminderHistoryValidator(_context);

                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 202, message = errors });
                }

                await _context.ReminderHistoryMaster.AddAsync(cm);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Code = 200, Message = "Reminder saved successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Code = 500, Message = "An error occurred", Detail = ex.Message });
            }
        }

        [HttpPatch("PatchReminderHistoryMaster/{id}")]
        public async Task<IActionResult> PatchReminderHistoryMaster(int id, [FromBody] JsonPatchDocument<ReminderHistoryMaster> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }

                var gm = await _context.ReminderHistoryMaster.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                patchDocument.ApplyTo(gm, ModelState);

                var validator = new ReminderHistoryValidator(_context);
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
                if(gm.BillPaid) gm.BillPaidDate = DateOnly.FromDateTime(DateTime.Now);

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Reminder updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
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

                var data = await _context.GuestDetails.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.UserId == userId).ToListAsync();

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

        [HttpGet("GetGuestDetailsById/{id}")]
        public async Task<IActionResult> GetGuestDetailsById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.GuestDetails
                          .Where(x => x.GuestId == id && x.IsActive && x.CompanyId == companyId && x.UserId == userId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }
    }
}
