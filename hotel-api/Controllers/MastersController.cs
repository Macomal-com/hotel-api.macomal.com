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
using Azure;
using System.Xml.Linq;
using System.Diagnostics.Metrics;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MastersController(DbContextSql context, IMapper mapper) : ControllerBase
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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
            if(cluster.IsActive == false)
            {
                var deleteValidator = new ClusterDeleteValidator(_context);
                var deleteResult = await deleteValidator.ValidateAsync(cluster);
                if (!deleteResult.IsValid)
                {
                    var firstError = deleteResult.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Cluster deleted successfully" });
            }
            var validator = new ClusterValidator(_context);
            var result = await validator.ValidateAsync(cluster);
            if (!result.IsValid)
            {
                var firstError = result.Errors.FirstOrDefault();
                if (firstError != null)
                {
                    return Ok(new { Code = 202, message = firstError.ErrorMessage });
                }
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
        public async Task<IActionResult> PatchLandlordDetails(int id, [FromForm] string patchDoc, IFormFile? file)
        {
            try
            {
                if (patchDoc == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }
                var patchDocument = JsonConvert.DeserializeObject<JsonPatchDocument<LandlordDetails>>(patchDoc);
                if(patchDocument == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }
                var landlord = await _context.LandlordDetails.FindAsync(id);

                if (landlord == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }
                patchDocument.ApplyTo(landlord, ModelState);
                if (landlord.IsActive == false)
                {
                    var validator = new LandlordDeleteValidator(_context);

                    var result = await validator.ValidateAsync(landlord);
                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    landlord.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Landlord deleted successfully" });
                }
                else
                {
                    var validator = new LandlordValidator(_context);
                    var result = await validator.ValidateAsync(landlord);
                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }
                    landlord.UpdatedDate = DateTime.Now;
                    if (file != null)
                    {
                        landlord.FilePath = await Constants.Constants.AddFile(file, "Uploads/landlordIdentity");

                    }
                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Landlord updated successfully" });

                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }


        [HttpPost("AddLandlordDetails")]
        public async Task<IActionResult> AddLandlordDetails([FromForm] LandlordDetailsDTO landlord, IFormFile? file)
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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                if (file != null)
                {
                    cm.FilePath = await Constants.Constants.AddFile(file, "Uploads/landlordIdentity");
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
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.BuildingMaster.FirstOrDefaultAsync(x => x.IsActive == true && x.BuildingId == id && x.CompanyId == companyId);


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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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

                if (building.IsActive == false)
                {
                    var validator = new BuildingDeleteValidator(_context);

                    var result = await validator.ValidateAsync(building);
                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    building.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Building deleted successfully" });
                }
                else
                {
                    var validator = new BuildingMasterValidator(_context);
                    var result = await validator.ValidateAsync(building);
                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    building.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Building updated successfully" });
                }
                    
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
                                  where floor.IsActive == true && floor.PropertyId == companyId&& floor.FloorId == id
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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
            if (floor.IsActive == false)
            {
                var validator = new FloorDeleteValidator(_context);

                var result = await validator.ValidateAsync(floor);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                floor.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Floor deleted successfully" });
            }
            else
            {
                var validator = new FloorValidator(_context);
                var result = await validator.ValidateAsync(floor);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                floor.BuildingId = floor.BuildingId == 0 ? null : floor.BuildingId;
                floor.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Floor updated successfully" });
            }
                
        }





        // GROUPS
        [HttpGet("GetGroup")]
        public async Task<IActionResult> GetGroup()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string financialYear = (HttpContext.Request.Headers["FinancialYear"]).ToString();
                var data = await _context.GroupMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();
                var groupCode = await DocumentHelper.GetDocumentNo(_context, Constants.Constants.DocumentGroupCode, companyId, financialYear);
                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Group not found", Data = Array.Empty<object>(), groupCode });
                }

                return Ok(new { Code = 200, Message = "Group fetched successfully", Data = data, groupCode });
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
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                string financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();

                var cm = _mapper.Map<GroupMaster>(group);
                cm.IGST = cm.GST;
                cm.SGST = cm.GST / 2;
                cm.CGST = cm.GST / 2;
                SetMastersDefault(cm, companyId, userId);
                var validator = new GroupValidator(_context);

                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }


                var response = await DocumentHelper.UpdateDocumentNo(_context, Constants.Constants.DocumentGroupCode, companyId, financialYear);
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
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
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
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
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
                var data = await _context.BedTypeMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

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
                          .Where(x => x.BedTypeId == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
            if (bedType.IsActive == false)
            {
                var validator = new BedTypeDeleteValidator(_context);

                var result = await validator.ValidateAsync(bedType);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                bedType.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "BedType deleted successfully" });
            }
            else
            {
                var validator = new BedTypeValidator(_context);

                var result = await validator.ValidateAsync(bedType);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                bedType.UpdatedDate = DateTime.Now;


                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Bed Type updated successfully" });
            }
                
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
                                  rs.CompanyId == companyId
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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                          .Where(x => x.SubGroupId == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
            if(group.IsActive == false)
            {
                var validator = new SubGroupDeleteValidator(_context);

                var result = await validator.ValidateAsync(group);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                group.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Sub Group deleted successfully" });
            }
            else
            {
                var validator = new SubGroupValidator(_context);
                var result = await validator.ValidateAsync(group);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                group.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "SubGroup updated successfully" });
            }
                
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
                                  where rs.IsActive == true && rs.CompanyId == companyId
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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                          .Where(x => x.ServiceId == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
            if(service.IsActive == false)
            {
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Service deleted successfully!" });
            }
            var validator = new ServiveValidator(_context);
            var result = await validator.ValidateAsync(service);
            if (!result.IsValid)
            {
                var firstError = result.Errors.FirstOrDefault();
                if (firstError != null)
                {
                    return Ok(new { Code = 202, message = firstError.ErrorMessage });
                }
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
                                  rs.CompanyId == companyId
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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                          .Where(x => x.VendorId == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
            if (vendor.IsActive == false)
            {
                var validator = new VendorDeleteValidator(_context);

                var result = await validator.ValidateAsync(vendor);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                vendor.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Vendor deleted successfully" });
            }
            else
            {
                var validator = new VendorValidator(_context);
                var result = await validator.ValidateAsync(vendor);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                vendor.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Vendor updated successfully" });
            }
            
        }

        //VENDOR HISTORY MASTER
        [HttpGet("GetVendorHistoryMaster")]
        public async Task<IActionResult> GetVendorHistoryMaster()
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                var data = await (
                        from history in _context.VendorHistoryMaster
                        join vendor in _context.VendorMaster on history.VendorId equals vendor.VendorId
                        join service in _context.VendorServiceMaster on history.ServiceId equals service.Id
                        join staff in _context.StaffManagementMaster on history.GivenById equals staff.StaffId
                        where history.IsActive && history.CompanyId == companyId
                        select new
                        {
                            history.Id,
                            history.GivenById,
                            vendor.VendorName,
                            service.Name,
                            DueDate = history.GivenDate.ToString("dd MMMM, yyyy"),
                            history.Remarks,
                            staff.StaffName
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
        [HttpPost("AddVendorHistoryMaster")]
        public async Task<IActionResult> AddVendorHistoryMaster([FromBody] VendorHistoryMasterDTO vendor)
        {
            if (vendor == null)
            {
                return Ok(new
                {
                    Code = 400,
                    Message = "Invalid data",
                    Data = Array.Empty<object>()
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<VendorHistoryMaster>(vendor);

                // If GivenById is 0, treat it as a new staff entry
                if (cm.GivenById == 0 && !string.IsNullOrWhiteSpace(cm.GivenBy))
                {
                    var department = await _context.DepartmentMaster.Where(x => x.IsActive && x.Name == "Vendor" && x.CompanyId == companyId).FirstOrDefaultAsync();
                    if(department == null)
                    {
                        return Ok(new { Code = 404, Message = "Vendor Department doesn't exist" });
                    }
                    var newStaff = new StaffManagementMaster
                    {
                        StaffName = cm.GivenBy,
                        PhoneNo = cm.PhoneNo,
                        DepartmentId = department.Id,
                        DesignationId = 0,
                        Salary = 0,
                        VendorId = cm.VendorId,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UserId = userId,
                        CompanyId = companyId
                    };

                    var staffValidator = new StaffValidator(_context);
                    var staffValidationResult = await staffValidator.ValidateAsync(newStaff);

                    if (!staffValidationResult.IsValid)
                    {
                        var firstError = staffValidationResult.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    _context.StaffManagementMaster.Add(newStaff);
                    await _context.SaveChangesAsync();

                    cm.GivenById = newStaff.StaffId;
                }

                SetMastersDefault(cm, companyId, userId);

                var vendorValidator = new VendorHistoryValidator(_context);
                var vendorValidationResult = await vendorValidator.ValidateAsync(cm);

                if (!vendorValidationResult.IsValid)
                {
                    var firstError = vendorValidationResult.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                _context.VendorHistoryMaster.Add(cm);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { Code = 200, Message = "Service history created successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Optionally log the exception here: e.g., _logger.LogError(ex, "Failed to add vendor history");
                return StatusCode(500, new
                {
                    Code = 500,
                    Message = Constants.Constants.ErrorMessage
                });
            }
        }

        [HttpGet("GetVendorHistoryById/{Id}")]
        public async Task<IActionResult> GetVendorHistoryById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await (
                        from history in _context.VendorHistoryMaster
                        join vendor in _context.VendorMaster on history.VendorId equals vendor.VendorId
                        join service in _context.VendorServiceMaster on history.ServiceId equals service.Id
                        join staff in _context.StaffManagementMaster on history.GivenById equals staff.StaffId
                        where history.IsActive && history.CompanyId == companyId && history.Id == id
                        select new
                        {
                            history.Id,
                            history.GivenById,
                            vendor.VendorName,
                            history.VendorId,
                            service.Name,
                            history.ServiceId,
                            DueDate = history.GivenDate.ToString("yyyy-MM-dd"),
                            history.Remarks,
                            GivenBy = staff.StaffName,
                            staff.PhoneNo
                        }).ToListAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Service not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Service fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("EditVendorHistory")]
        public async Task<IActionResult> EditVendorHistory([FromBody] VendorHistoryMaster model)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

            if (model == null)
            {
                return Ok(new { Code = 400, message = "Invalid request. Data is null.", data = new object() });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingProduct = await _context.VendorHistoryMaster
                    .FirstOrDefaultAsync(u => u.Id == model.Id && u.IsActive == true);

                if (existingProduct == null)
                {
                    return Ok(new { Code = 404, message = "Service does not exist.", data = new object() });
                }

                // Update existing fields
                existingProduct.UpdatedDate = DateTime.Now;
                existingProduct.ServiceId = model.ServiceId;
                existingProduct.VendorId = model.VendorId;
                existingProduct.GivenBy = model.GivenBy;
                existingProduct.GivenDate = model.GivenDate;

                if (model.GivenById == 0 && !string.IsNullOrWhiteSpace(model.GivenBy))
                {
                    var newStaff = new StaffManagementMaster
                    {
                        StaffName = model.GivenBy,
                        PhoneNo = model.PhoneNo,
                        DepartmentId = 0,
                        DesignationId = 0,
                        Salary = 0,
                        VendorId = model.VendorId,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UserId = userId,
                        CompanyId = companyId
                    };

                    var staffValidator = new StaffValidator(_context);
                    var staffValidationResult = await staffValidator.ValidateAsync(newStaff);

                    if (!staffValidationResult.IsValid)
                    {
                        var firstError = staffValidationResult.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            await transaction.RollbackAsync();
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    _context.StaffManagementMaster.Add(newStaff);
                    await _context.SaveChangesAsync();

                    // Update GivenById only after successful staff save
                    existingProduct.GivenById = newStaff.StaffId;
                }
                else
                {
                    // Keep the existing GivenById if not creating a new one
                    existingProduct.GivenById = model.GivenById;
                }

                _context.VendorHistoryMaster.Update(existingProduct);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { Code = 200, message = "Service updated successfully", data = new object() });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Optional: log the exception
                return StatusCode(500, new { Code = 500, message = "An error occurred", data = new object() });
            }
        }

        [HttpPost("DeleteVendorHistory/{id}")]
        public async Task<IActionResult> DeleteVendorHistory(int id)
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
                var existingProduct = await _context.VendorHistoryMaster
                    .FirstOrDefaultAsync(u => u.Id == id && u.IsActive == true);

                if (existingProduct == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 404, message = "Service does not exist.", data = new object() });
                }

                // Update existing fields
                existingProduct.IsActive = false;

                _context.VendorHistoryMaster.Update(existingProduct);
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
                          .Where(x => x.PaymentId == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
            if(pm.IsActive == false)
            {
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Payment mode deleted successfully!" });
            }
            var validator = new PaymentModeValidator(_context);

            var result = await validator.ValidateAsync(pm);
            if (!result.IsValid)
            {
                var firstError = result.Errors.FirstOrDefault();
                if (firstError != null)
                {
                    return Ok(new { Code = 202, message = firstError.ErrorMessage });
                }
            }
            pm.UpdatedDate = DateTime.Now;


            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Payment Mode updated successfully" });
        }


        //STAFF MASTER
        [HttpGet("GetStaffMaster")]
        public async Task<IActionResult> GetStaffMaster(string status = "")
        {

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                int? vendorDepartmentId = null;
                List<StaffManagementMaster> data = null;

                // Only check department if status is "Vendor"
                if (status.Equals("Vendor", StringComparison.OrdinalIgnoreCase))
                {
                    var department = await _context.DepartmentMaster
                        .Where(x => x.IsActive && x.CompanyId == companyId && x.Name == status)
                        .FirstOrDefaultAsync();

                    if (department == null)
                    {
                        return Ok(new { Code = 200, Message = "Department not found", Data = Array.Empty<object>() });
                    }

                    vendorDepartmentId = department.Id;
                    data = await _context.StaffManagementMaster
                       .Where(bm => bm.IsActive
                                    && bm.CompanyId == companyId
                                   && bm.DepartmentId == vendorDepartmentId)
                       .ToListAsync();
                }
                else
                {
                    var staffData = await (
                        from bm in _context.StaffManagementMaster
                        join department in _context.DepartmentMaster
                            on bm.DepartmentId equals department.Id
                        join designation in _context.StaffDesignationMaster
                            on bm.DesignationId equals designation.Id into designationGroup
                        from designation in designationGroup.DefaultIfEmpty() // LEFT JOIN here
                        where bm.IsActive && bm.CompanyId == companyId && bm.VendorId == 0
                        select new
                        {
                            bm.StaffId,
                            bm.StaffName,
                            Department = department.Name,
                            Designation = designation != null ? designation.Name : "", // handle null
                            bm.PhoneNo,
                            bm.Salary
                        }
                   ).ToListAsync();

                    return Ok(new { Code = 200, Message = "Staff fetched successfully", Data = staffData });


                }
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
                return Ok(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<StaffManagementMaster>(sm);
                var departmentId = 0;
                if(cm.DepartmentId == 0)
                {
                    var department = new DepartmentMaster
                    {
                        Name = sm.Department,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CompanyId = companyId,
                        UserId = userId
                    };
                    _context.DepartmentMaster.Add(department);
                    await _context.SaveChangesAsync();
                   
                    cm.DepartmentId = department.Id;
                    departmentId = department.Id;
                }
                else
                {
                    departmentId = cm.DepartmentId;
                }
                if (cm.DesignationId == 0 && sm.StaffDesignation != "")
                {
                    var designation = new StaffDesignationMaster
                    {
                        Name = sm.StaffDesignation,
                        DepartmentId = departmentId,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CompanyId = companyId,
                        UserId = userId
                    };
                    _context.StaffDesignationMaster.Add(designation);
                    await _context.SaveChangesAsync();
                    
                    cm.DesignationId = designation.Id;
                }
                SetMastersDefault(cm, companyId, userId);
                var validator = new StaffValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    await transaction.RollbackAsync();
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                _context.StaffManagementMaster.Add(cm);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "Staff created successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
                var data = await (from bm in _context.StaffManagementMaster
                                       join department in _context.DepartmentMaster on bm.DepartmentId equals department.Id
                                       join designation in _context.StaffDesignationMaster on bm.DesignationId equals designation.Id
                                       where bm.IsActive && bm.CompanyId == companyId && bm.StaffId == id
                                       select new
                                       {
                                           bm.StaffId,
                                           bm.StaffName,
                                           bm.DepartmentId,
                                           bm.DesignationId,
                                           Department = department.Name,
                                           staffDesignation = designation.Name,
                                           bm.PhoneNo,
                                           bm.Salary
                                       }).ToListAsync();

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
            if(sm.IsActive == false)
            {
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Staff Deleted Successfully" });
            }
            var validator = new StaffValidator(_context);
            var result = await validator.ValidateAsync(sm);
            if (!result.IsValid)
            {
                var firstError = result.Errors.FirstOrDefault();
                if (firstError != null)
                {
                    return Ok(new { Code = 202, message = firstError.ErrorMessage });
                }
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
                    return Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetGstRangeMaster/{id}")]
        public async Task<IActionResult> GetGstRangeMaster(int id)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.GstRangeMaster.Where(bm => bm.GstId == id && bm.IsActive && bm.CompanyId == companyId).ToListAsync();

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
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();
                var rangeData = await _context.GstRangeMaster
                          .Where(x => x.GstId == id && x.IsActive && x.CompanyId == companyId).ToListAsync();
                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data, Range = rangeData });
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
            using var transaction = await _context.Database.BeginTransactionAsync();
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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }

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

        [HttpPost("PatchGstMaster/{id}")]
        public async Task<IActionResult> PatchGstMaster(int id, [FromBody] GstMaster model)
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

                var existingGst = await _context.GstMaster.FindAsync(model.Id);
                if (existingGst == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }

                // Validate incoming model (not the DB entity)
                var validator = new GstValidator(_context);
                var result = await validator.ValidateAsync(model);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    return Ok(new { Code = 202, message = firstError?.ErrorMessage });
                }

                // Update main GST master
                existingGst.TaxPercentage = model.TaxPercentage;
                existingGst.ApplicableServices = model.ApplicableServices;
                existingGst.GstType = model.GstType;
                existingGst.UpdatedDate = DateTime.Now;
                _context.GstMaster.Update(existingGst);

                // Deactivate old ranges
                var oldRanges = await _context.GstRangeMaster
                    .Where(x => x.GstId == model.Id && x.CompanyId == companyId && x.IsActive)
                    .ToListAsync();
                foreach (var range in oldRanges)
                {
                    range.IsActive = false;
                }

                // Add new ranges
                foreach (var item in model.ranges)
                {
                    if (item.TaxPercentage != 0)
                    {
                        SetMastersDefault(item, companyId, userId);
                        item.RangeId = 0;
                        item.GstId = model.Id;
                        await _context.GstRangeMaster.AddAsync(item);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Code = 200, Message = "Gst updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpPost("DeleteGstMaster/{id}")]
        public async Task<IActionResult> DeleteGstMaster(int id)
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
                var existingProduct = await _context.GstMaster
                    .FirstOrDefaultAsync(u => u.Id == id && u.IsActive == true);

                if (existingProduct == null)
                {
                    await transaction.RollbackAsync();
                    return Ok(new { Code = 404, message = "Gst does not exist.", data = new object() });
                }

                // Update existing fields
                existingProduct.IsActive = false;
                var ranges = await _context.GstRangeMaster
                    .Where(x => x.GstId == id && x.CompanyId == companyId && x.IsActive)
                    .ToListAsync();
                if(ranges != null)
                {
                    foreach (var range in ranges)
                    {
                        range.IsActive = false;
                    }
                }
                
                _context.GstMaster.Update(existingProduct);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { Code = 200, message = "Gst deleted successfully", data = new object() });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Optional: log the exception
                return StatusCode(500, new { Code = 500, message = "An error occurred", data = new object() });
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
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                if (gm.IsActive == false)
                {
                    await _context.SaveChangesAsync();
                    return Ok(new { Code = 200, Message = "Commission deleted successfully!" });
                }
                var validator = new CommissionValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                gm.UpdatedDate = DateTime.Now;

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
                          .Where(x => x.IsActive == true && x.CompanyId == companyId && x.UserId == id).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        private  (int code, string message) CreateOrUpdateUser(
       UserDetails user, int companyid = 0, int userid = 0 , string database = "")
        {
            using (SqlConnection conn = new SqlConnection(_context.Database.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand("CreateUpdateUser", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters
                cmd.Parameters.AddWithValue("@userid", user.UserId);
                cmd.Parameters.AddWithValue("@username",user.UserName);
                cmd.Parameters.AddWithValue("@password", user.Password);
                cmd.Parameters.AddWithValue("@companyid", companyid);
                cmd.Parameters.AddWithValue("@dbname",database);
                cmd.Parameters.AddWithValue("@name", user.Name);
                cmd.Parameters.AddWithValue("@emailid", user.EmailId);
                cmd.Parameters.AddWithValue("@phoneno", user.PhoneNo);
                cmd.Parameters.AddWithValue("@role", user.Roles);
                cmd.Parameters.AddWithValue("@createdby", userid);
                cmd.Parameters.AddWithValue("@isactive", user.IsActive);
                SqlParameter returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(returnValueParam);
                string message = "";
                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            message = reader["Message"]?.ToString() ?? "";

                        }
                    }

                    int code = (int)(cmd.Parameters["@ReturnValue"].Value ?? 400);

                    return (code, message);
                }
                catch (Exception ex)
                {
                    return (400, ex.Message);
                    
                }
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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                //call sp
                (int code , string message) = CreateOrUpdateUser(cm, companyId, userId, dbName);

                return Ok(new { Code = code, Message = message });
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
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
                string dbName = Convert.ToString(HttpContext.Request.Headers["Database"]);
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
                if (gm.IsActive == false)
                {
                    (int code1, string message1) = CreateOrUpdateUser(gm, companyId, userId, dbName);

                    return Ok(new { Code = code1, Message = message1 });
                    //await _context.SaveChangesAsync();
                    //return Ok(new { Code = 200, Message = "User deleted successfully!" });
                }
                var validator = new UserValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                gm.ModifyDate = DateTime.Now;
                (int code, string message) = CreateOrUpdateUser(gm, companyId, userId, dbName);

                return Ok(new { Code = code, Message = message });
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
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                if (vs.IsActive == false)
                {
                    var validator = new VendorServiceDeleteValidator(_context);

                    var result = await validator.ValidateAsync(vs);
                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    vs.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Vendor Service deleted successfully" });
                }
                else
                {
                    var validator = new VendorServiceValidator(_context);
                    var result = await validator.ValidateAsync(vs);

                    vs.UpdatedDate = DateTime.Now;                    

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Vendor Service updated successfully" });
                }
                    
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
                //var data = await _context.CompanyDetails.Where(bm => bm.IsActive).ToListAsync();
                var data = await (from com in _context.CompanyDetails
                                  join cl in _context.ClusterMaster on com.ClusterId equals cl.ClusterId  into cls
                                  from cluster in cls.DefaultIfEmpty()
                                  join landlord in _context.LandlordDetails on com.OwnerId equals landlord.LandlordId into landlordJoin
                                  from landlord in landlordJoin.DefaultIfEmpty()
                                  where com.IsActive && (cluster == null || cluster.IsActive) && (landlord == null || landlord.IsActive)
                                  select new
                                       {
                                            com.PropertyId,
                                            com.CompanyName,
                                            com.HotelTagline,
                                            com.Country,
                                            com.Gstin,
                                            com.State,
                                            com.City,
                                            com.CompanyAddress,
                                            com.Pincode,
                                            com.ContactNo1,
                                            com.Email,
                                            com.PanNo,
                                            com.ClusterId,
                                            com.OwnerId,
                                      ClusterName = cluster.ClusterName ?? "",
                                            landlord.LandlordName,
                                            com.CommissionType,
                                            com.CommissionCharge
                                       }).ToListAsync();
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
                    string financialYear = HttpContext.Request.Headers["FinancialYear"].ToString();

                    var cm = _mapper.Map<CompanyDetails>(companyDetails);
                    SetMastersDefault(cm, companyId, userId);
                    var validator = new PropertyDetailsValidator(_context);

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
                    _context.CompanyDetails.Add(cm);
                    await _context.SaveChangesAsync();

                    cm.GstType = Constants.PropertyConstants.GSTTYPE;
                    cm.IsCheckOutApplicable = Constants.PropertyConstants.ISCHECKOUTAPPLICABLE;
                    cm.CheckOutFormat = Constants.PropertyConstants.CHECKOUTFORMAT;
                    cm.IsRoomRateEditable = Constants.PropertyConstants.ISROOMRATEEDITABLE;
                    cm.CheckInTime = Constants.PropertyConstants.CHECKINTIME;
                    cm.CheckOutTime = Constants.PropertyConstants.CHECKOUTTIME;
                    cm.ApproveReservation = Constants.PropertyConstants.APPROVERESERVATION;
                    cm.CancelMethod = Constants.PropertyConstants.CANCELMETHOD;
                    cm.CancelCalculatedBy = Constants.PropertyConstants.CANCELCALCULATEBY;
                    cm.CheckOutInvoice = Constants.PropertyConstants.CHECKOUTINVOICE;
                    cm.IsWhatsappNotification = Constants.PropertyConstants.WHATSAPPNOTIFICATIONENABLE;
                    cm.IsEmailNotification = Constants.PropertyConstants.EMAILNOTIFICATIONENABLE;
                    cm.IsDefaultCheckInTimeApplicable = Constants.PropertyConstants.DEFAULTCHECKIN;
                    cm.IsDefaultCheckOutTimeApplicable = Constants.PropertyConstants.DEFAULTCHECKOUT;
                    cm.CalculateRoomRates = Constants.PropertyConstants.CALCULATEROOMRATES;
                    cm.ReservationNotification = false;
                    cm.CheckinNotification = false;
                    cm.RoomShiftNotification = false;
                    cm.CheckOutNotification = false;
                    cm.CancelBookingNotification = false;
                    cm.CancelMethod = Constants.Constants.DateWiseCancel;
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
                                PropertyId = cm.PropertyId,
                                FilePath = fileName
                            };
                            propertyImages.Add(propertyImage);
                        }

                        _context.PropertyImages.AddRange(propertyImages);
                    }
                    DocumentHelper.CreateDocument(_context, "CP", cm.PropertyId, financialYear, Constants.Constants.DocumentCancelPolicy, userId);

                    DocumentHelper.CreateDocument(_context, "RES", cm.PropertyId, financialYear, Constants.Constants.DocumentReservation, userId);


                    DocumentHelper.CreateDocument(_context, "INV", cm.PropertyId, financialYear, Constants.Constants.DocumentInvoice, userId);

                    DocumentHelper.CreateDocument(_context, "KOT", cm.PropertyId, financialYear, Constants.Constants.DocumentKot, userId);
                    DocumentHelper.CreateDocument(_context, "G", cm.PropertyId, financialYear, Constants.Constants.DocumentGroupCode, userId);

                    var department = new DepartmentMaster
                    {
                        Name = "Vendor",
                        CompanyId = cm.PropertyId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        IsActive = true,
                        UserId = userId
                    };
                    _context.DepartmentMaster.Add(department);


                    var gstmaster = new GstMaster
                    {
                        TaxPercentage = 12,
                        ApplicableServices = Constants.Constants.Reservation,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UserId = userId,
                        CompanyId = cm.PropertyId,
                        GstType = Constants.Constants.SingleGst,
                        RangeStart = 0,
                        RangeEnd = 0
                    };
                    _context.GstMaster.Add(gstmaster);


                    var mode = new PaymentMode
                    {
                        PaymentModeName = "Cash",
                        ProviderContact = "",
                        ProviderEmail = "",
                        TransactionCharges = 0,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UserId = userId,
                        CompanyId = cm.PropertyId,
                        TransactionType = ""
                    };
                    _context.PaymentMode.Add(mode);

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

                    return Ok(new { Code = 200, Message = "Company updated successfully" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return StatusCode(500, new { Code = 500, Message = ex.Message });
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

                var data = await _context.HourMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

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
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                if (gm.IsActive == false)
                {
                    await _context.SaveChangesAsync();
                    return Ok(new { Code = 200, Message = "Hour deleted successfully!" });
                }
                var validator = new HourValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                gm.UpdatedDate = DateTime.Now;

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

                var data = await _context.ExtraPolicies.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

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
                          .Where(x => x.PolicyId == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                if (gm.IsActive == false)
                {
                    await _context.SaveChangesAsync();
                    return Ok(new { Code = 200, Message = "Extra Policy deleted successfully!" });
                }
                var validator = new ExtrapolicyValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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

                var data = await _context.CancelPolicyMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

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

                var getbookingno = await DocumentHelper.GetDocumentNo(_context,Constants.Constants.DocumentCancelPolicy, companyId, financialYear);
                if (getbookingno == null)
                {
                    return Ok(new { Code = 400, message = "Document number not found." });
                }
                return Ok(new { Code = 200, Message = "Data fetched successfully", Data = getbookingno });
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
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                if (gm.IsActive == false)
                {
                    await _context.SaveChangesAsync();
                    return Ok(new { Code = 200, Message = "Cancel Policy deleted successfully!" });
                }
                var validator = new CancelPolicyValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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

                var data = await _context.ReminderMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

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
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
                if (gm.IsActive == false)
                {
                    var validator = new ReminderDeleteValidator(_context);

                    var result = await validator.ValidateAsync(gm);
                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    gm.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Reminder deleted successfully" });
                }
                else
                {
                    var validator = new ReminderValidator(_context);
                    var result = await validator.ValidateAsync(gm);
                    if (!result.IsValid)
                    {
                        var firstError = result.Errors.FirstOrDefault();
                        if (firstError != null)
                        {
                            return Ok(new { Code = 202, message = firstError.ErrorMessage });
                        }
                    }

                    gm.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    return Ok(new { Code = 200, Message = "Reminder updated successfully" });
                }
                    
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
                                  where history.IsActive && history.CompanyId == companyId
                                  select new
                                  {
                                      history.Id,
                                      reminder.ReminderType,
                                      history.DaysBefore,
                                      DueDate = history.DueDate.ToString("yyyy-MM-dd"),
                                      history.BillPaid,
                                      history.ReminderTime,
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
                          .Where(x => x.Id == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

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
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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
        public async Task<IActionResult> PatchReminderHistoryMaster(int id, [FromForm] string patchDoc, IFormFile? file)
        {
            try
            {
                if (patchDoc == null)
                {
                    return Ok(new { Code = 500, Message = "Invalid Data" });

                }
                var patchDocument = JsonConvert.DeserializeObject<JsonPatchDocument<ReminderHistoryMaster>>(patchDoc);
                var gm = await _context.ReminderHistoryMaster.FindAsync(id);

                if (gm == null)
                {
                    return Ok(new { Code = 404, Message = "Data Not Found" });
                }
                // Handle file upload
                if (file != null)
                {
                    var documentPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "reminderDocuments");
                    Directory.CreateDirectory(documentPath);

                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(documentPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    gm.DocumentPath = fileName;
                }
                patchDocument.ApplyTo(gm, ModelState);
                if (gm.IsActive == false)
                {
                    await _context.SaveChangesAsync();
                    return Ok(new { Code = 200, Message = "Reminder History deleted successfully!" });
                }
                var validator = new ReminderHistoryValidator(_context);
                var result = await validator.ValidateAsync(gm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
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


        //DEPARTMENT MASTER
        [HttpGet("GetDepartments")]
        public async Task<IActionResult> GetDepartments()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.DepartmentMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId && bm.Name != "Vendor").ToListAsync();


                if (data.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "No departments found.", Data = Array.Empty<object>() });
                }
                return Ok(new { Code = 200, Message = "Departments fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        //DEPARTMENT MASTER
        [HttpGet("GetStaffDesignation")]
        public async Task<IActionResult> GetStaffDesignation()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.StaffDesignationMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();


                if (data.Count == 0)
                {
                    return Ok(new { Code = 200, Message = "No designations found.", Data = Array.Empty<object>() });
                }
                return Ok(new { Code = 200, Message = "Designations fetched successfully", Data = data });
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
                          .Where(x => x.GuestId == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Data not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Data fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        //ASSET MASTER
        [HttpGet("GetAssets")]
        public async Task<IActionResult> GetAssets()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.AssetMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Assets not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Assets fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetAssetById/{id}")]
        public async Task<IActionResult> GetAssetById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.AssetMaster
                          .Where(x => x.AssetId == id && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();

                return data == null
                    ? Ok(new { Code = 404, Message = "Assets not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Assets fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddAssetMaster")]
        public async Task<IActionResult> AddAssetMaster([FromBody] AssetMasterDTO asset)
        {
            if (asset == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<AssetMaster>(asset);
                SetMastersDefault(cm, companyId, userId);
                var validator = new AssetValidator(_context);
                var result = await validator.ValidateAsync(cm);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                _context.AssetMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Asset created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPatch("PatchAssetMaster/{id}")]
        public async Task<IActionResult> PatchAssetMaster(int id, [FromBody] JsonPatchDocument<AssetMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var asset = await _context.AssetMaster.FindAsync(id);

            if (asset == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(asset, ModelState);
            if (asset.IsActive == false)
            {
                var validator = new AssetDeleteValidator(_context);

                var result = await validator.ValidateAsync(asset);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }

                asset.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Asset deleted successfully" });
            }
            else
            {
                var validator = new AssetValidator(_context);

                var result = await validator.ValidateAsync(asset);
                if (!result.IsValid)
                {
                    var firstError = result.Errors.FirstOrDefault();
                    if (firstError != null)
                    {
                        return Ok(new { Code = 202, message = firstError.ErrorMessage });
                    }
                }
                asset.UpdatedDate = DateTime.Now;


                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Asset updated successfully" });
            }

        }

        [HttpGet("GetRoomAssets")]
        public async Task<IActionResult> GetRoomAssets()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var room = await _context.RoomMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();
                var data = await _context.AssetMaster.Where(bm => bm.IsActive && bm.CompanyId == companyId).ToListAsync();
                var result = await (
                    from ram in _context.RoomAssetMapping
                    join rm in _context.RoomMaster on ram.RoomId equals rm.RoomId
                    join am in _context.AssetMaster on ram.AssetId equals am.AssetId
                    where ram.IsActive && ram.CompanyId == companyId && rm.CompanyId == companyId && am.CompanyId == companyId
                    group new { ram, rm, am } by new { ram.RoomId, rm.RoomNo } into g
                    select new RoomAssetMappingDTO
                    {
                        RoomId = g.Key.RoomId,
                        RoomNo = g.Key.RoomNo,
                        AssetData = g.Select(x => new MappingDTO
                        {
                            Id = x.ram.Id,
                            AssetId = x.ram.AssetId,
                            AssetName = x.am.AssetName,
                            Quantity = x.ram.Quantity,
                            AssetOwner = x.ram.AssetOwner,
                            CreatedDate = x.ram.CreatedDate.ToString("dd-MM-yyyy")
                        }).ToList()
                    }
                ).ToListAsync();

                return Ok(new { Code = 200, Message = "Data fetched successfully", Rooms = room, Assets = data, Mappings = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("AddRoomAssetMaster")]
        public async Task<IActionResult> AddRoomAssetMaster([FromBody] RoomAssetMappingDTO asset)
        {
            if (asset == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var data = await _context.RoomAssetMapping
                            .Where(x => x.RoomId == asset.RoomId && x.IsActive && x.CompanyId == companyId).ToListAsync();



                foreach (var item in asset.AssetData)
                {
                    var a = new RoomAssetMapping
                    {
                        RoomId = asset.RoomId,
                        AssetId = item.AssetId,
                        Quantity = item.Quantity,
                        AssetOwner = item.AssetOwner,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CompanyId = companyId,
                        UserId = userId
                    };
                    _context.RoomAssetMapping.Add(a);
                }
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Mapping created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetRoomAssetById/{id}")]
        public async Task<IActionResult> GetRoomAssetById(int id)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.RoomAssetMapping
                             .Where(x => x.RoomId == id && x.IsActive && x.CompanyId == companyId).ToListAsync();
                    
                return data == null
                    ? Ok(new { Code = 404, Message = "Mappings not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Mappings fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("CheckAssets")]
        public async Task<IActionResult> CheckAssets([FromBody] RoomAssetMappingDTO asset)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

            try
            {
                var existingAssets = await (
                    from ram in _context.RoomAssetMapping
                    join am in _context.AssetMaster on ram.AssetId equals am.AssetId
                    where ram.RoomId == asset.RoomId && ram.IsActive && ram.CompanyId == companyId
                    select new { ram.AssetId, am.AssetName }
                ).ToListAsync();

                var requestedAssetIds = asset.AssetData.Select(a => a.AssetId).ToList();

                // Find duplicates by matching assetIds
                var duplicates = existingAssets
                    .Where(x => requestedAssetIds.Contains(x.AssetId))
                    .Distinct()
                    .ToList();

                if (duplicates.Any())
                {
                    var duplicateNames = string.Join(", ", duplicates.Select(d => d.AssetName));

                    return Ok(new
                    {
                        Code = 409,
                        Message = $"This Room already contains assets: {duplicateNames}. Do you still want to proceed?",
                    });
                }

                return Ok(new
                {
                    Code = 200,
                    Message = "No duplicate assets found."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Code = 500,
                    Message = Constants.Constants.ErrorMessage
                });
            }
        }

        [HttpPost("UpdateRoomAssetMapping")]
        public async Task<IActionResult> UpdateRoomAssetMapping([FromBody] RoomAssetMappingDTO asset)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.RoomAssetMapping
                            .Where(x => x.RoomId == asset.RoomId && x.IsActive && x.CompanyId == companyId).ToListAsync();

                foreach (var item in asset.AssetData)
                {
                    if (item.Id != 0)
                    {
                        var existing = data.FirstOrDefault(x => x.Id == item.Id);
                        if (existing != null)
                        {
                            existing.AssetId = item.AssetId;
                            existing.Quantity = item.Quantity;
                            existing.UpdatedDate = DateTime.Now;
                            existing.AssetOwner = item.AssetOwner;
                            existing.IsActive = item.IsActive;
                            _context.RoomAssetMapping.Update(existing);
                        }
                    }
                    else
                    {
                        var a = new RoomAssetMapping
                        {
                            RoomId = asset.RoomId,
                            AssetId = item.AssetId,
                            Quantity = item.Quantity,
                            AssetOwner = item.AssetOwner,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            CompanyId = companyId,
                            UserId = userId
                        };
                        _context.RoomAssetMapping.Add(a);
                    }
                }
                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Mappings updated successfully" });
            }

            catch(Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }

        [HttpPost("DeleteRoomAssetMapping")]
        public async Task<IActionResult> DeleteRoomAssetMapping(string type="", int assetId = 0, int roomId = 0)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                if(type == "All")
                {
                    var data = await _context.RoomAssetMapping
                            .Where(x => x.RoomId == roomId && x.IsActive && x.CompanyId == companyId).ToListAsync();
                    foreach(var item in data)
                    {
                        item.IsActive = false;
                        item.UpdatedDate = DateTime.Now;
                    }
                }
                else
                {
                    var data = await _context.RoomAssetMapping
                            .Where(x => x.Id == assetId && x.IsActive && x.CompanyId == companyId).FirstOrDefaultAsync();
                    if(data == null)
                    {
                        return Ok(new { Code = 404, Message = "Asset not Found!" });
                    }
                    data.IsActive = false;
                    data.UpdatedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return Ok(new { Code = 200, Message = "Mappings updated successfully" });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }

        }
    }
}
