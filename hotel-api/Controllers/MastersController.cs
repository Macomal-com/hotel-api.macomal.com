using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.Models;
using RepositoryModels.Repository;
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

        //-----------------------------
        //GET APIS
        //-----------------------------

        [HttpGet("GetCompanyDetails")]
        public async Task<IActionResult> GetCompanyDetails()
        {
            try
            {
                var data = await _context.CompanyDetails.ToListAsync();

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
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetBedTypes")]
        public async Task<IActionResult> GetBedTypes()
        {
            try
            {
                var data = await _context.BedTypeMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Bed Types not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Bed Types fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetBuildingMaster")]
        public async Task<IActionResult> GetBuildingMaster()
        {
            try
            {
                var data = await _context.BuildingMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Buildings not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Buildings fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetClusterMaster")]
        public async Task<IActionResult> GetClusterMaster()
        {
            try
            {
                var data = await _context.ClusterMaster.ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Cluster not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Cluster details fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetFloorMaster")]
        public async Task<IActionResult> GetFloorMaster()
        {
            try
            {
                var data = await _context.FloorMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Floors not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Floors fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetLandlordDetails")]
        public async Task<IActionResult> GetLandlordDetails()
        {
            try
            {
                var data = await _context.LandlordDetails.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Landlords not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Landlords fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetOwnerMaster")]
        public async Task<IActionResult> GetOwnerMaster()
        {
            try
            {
                var data = await _context.OwnerMaster.ToListAsync();

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
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetRoomCategoryMaster")]
        public async Task<IActionResult> GetRoomCategoryMaster()
        {
            try
            {
                var data = await _context.RoomCategoryMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Room Category not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Room Category fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetRoomMaster")]
        public async Task<IActionResult> GetRoomMaster()
        {
            try
            {
                var data = await _context.RoomCategoryMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Rooms not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Rooms fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetRoomRateMaster")]
        public async Task<IActionResult> GetRoomRateMaster()
        {
            try
            {
                var data = await _context.RoomRateMaster.ToListAsync();

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
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetServicableMaster")]
        public async Task<IActionResult> GetServicableMaster()
        {
            try
            {
                var data = await _context.ServicableMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Servicable Master not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Servicable Master fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetVendorMaster")]
        public async Task<IActionResult> GetVendorMaster()
        {
            try
            {
                var data = await _context.VendorMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Vendor Master not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Vendor Master fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var data = await _context.UserCreation.ToListAsync();

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
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetStaff")]
        public async Task<IActionResult> GetStaff()
        {
            try
            {
                var data = await _context.StaffManagementMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Staff not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Staff fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetPaymentMode")]
        public async Task<IActionResult> GetPaymentMode()
        {
            try
            {
                var data = await _context.PaymentMode.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Payment Mode not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Payment Mode fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetGroup")]
        public async Task<IActionResult> GetGroup()
        {
            try
            {
                var data = await _context.GroupMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Group not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Group fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetSubGroup")]
        public async Task<IActionResult> GetSubGroup()
        {
            try
            {
                var data = await _context.SubGroupMaster.ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound(new { Code = 404, Message = "Sub Group not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Sub Group fetched successfully", Data = data });
            }
            catch (SqlException sqlEx)
            {
                // _logger.LogError($"SQL Error: {sqlEx.Message}");
                return StatusCode(500, new { Code = 500, sqlEx.Message, Data = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                // _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                          .Where(x => x.PropertyId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Company not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Company details fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetClusterById")]
        public async Task<IActionResult> GetClusterById(int id)
        {
            try
            {
                var data = await _context.ClusterMaster
                          .Where(x => x.ClusterId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Cluster not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Cluster details fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetBedTypeById")]
        public async Task<IActionResult> GetBedTypeById(int id)
        {
            try
            {
                var data = await _context.BedTypeMaster
                          .Where(x => x.BedTypeId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Bed Type not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Bed Type fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetBuildingById")]
        public async Task<IActionResult> GetBuildingById(int id)
        {
            try
            {
                var data = await _context.BuildingMaster
                          .Where(x => x.BuildingId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Building not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Building fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetFloorById")]
        public async Task<IActionResult> GetFloorById(int id)
        {
            try
            {
                var data = await _context.FloorMaster
                          .Where(x => x.FloorId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Floor not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Floor fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetLandlordgById")]
        public async Task<IActionResult> GetLandlordgById(int id)
        {
            try
            {
                var data = await _context.LandlordDetails
                          .Where(x => x.LandlordId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Landlord not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Landlord fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetOwnerById")]
        public async Task<IActionResult> GetOwnerById(int id)
        {
            try
            {
                var data = await _context.OwnerMaster
                          .Where(x => x.OwnerId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Owner not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Owner fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetRoomCategoryById")]
        public async Task<IActionResult> GetRoomCategoryById(int id)
        {
            try
            {
                var data = await _context.RoomCategoryMaster
                          .Where(x => x.Id == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Room Category not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Room Category fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetRoomById")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            try
            {
                var data = await _context.RoomMaster
                          .Where(x => x.RoomId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Room not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Room fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetRoomRateById")]
        public async Task<IActionResult> GetRoomRateById(int id)
        {
            try
            {
                var data = await _context.RoomRateMaster
                          .Where(x => x.RoomRateId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Room Rate not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Room Rate fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetServicableById")]
        public async Task<IActionResult> GetServicableById(int id)
        {
            try
            {
                var data = await _context.ServicableMaster
                          .Where(x => x.ServiceId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Servicable not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Servicable fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetVendorById")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            try
            {
                var data = await _context.VendorMaster
                          .Where(x => x.VendorId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Vendor not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Vendor fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var data = await _context.UserCreation
                          .Where(x => x.Id == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "User not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "User fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetStaffById")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            try
            {
                var data = await _context.StaffManagementMaster
                          .Where(x => x.StaffId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Staff not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Staff fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetPaymentModeById")]
        public async Task<IActionResult> GetPaymentModeById(int id)
        {
            try
            {
                var data = await _context.PaymentMode
                          .Where(x => x.PaymentId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Payment Mode not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Payment Mode fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetGroupById")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            try
            {
                var data = await _context.GroupMaster
                          .Where(x => x.Id == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Group not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Gruop fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpGet("GetSubGroupById")]
        public async Task<IActionResult> GetSubGroupById(int id)
        {
            try
            {
                var data = await _context.SubGroupMaster
                          .Where(x => x.SubGroupId == id).ToListAsync();

                return data.Count == 0
                    ? NotFound(new { Code = 404, Message = "Sub Group not found", Data = Array.Empty<object>() })
                    : Ok(new { Code = 200, Message = "Sub Group fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        //-----------------------------
        //PATCH APIS
        //-----------------------------

        [HttpPatch("PatchCluster")]
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
            cluster.UpdatedDate = DateTime.Now.ToString();
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                                    .Where(x => x.Value.Errors.Any())
                                    .SelectMany(x => x.Value.Errors)
                                    .Select(x => x.ErrorMessage)
                                    .ToList();
                return Ok(new { Code = 500, Message = errorMessages});
            }

            await _context.SaveChangesAsync();

            return Ok(new { Code = 200, Message = "Cluster updated successfully" });
        }

        [HttpPatch("PatchCompanyMaster")]
        public async Task<IActionResult> PatchCompanyMaster(int id, [FromBody] JsonPatchDocument<CompanyDetails> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var company = await _context.CompanyDetails.FindAsync(id);

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

        [HttpPatch("PatchBedTypeMaster")]
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
            bedType.UpdatedDate = DateTime.Now.ToString();
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

            return Ok(new { Code = 200, Message = "Bed Type updated successfully" });
        }

        [HttpPatch("PatchBuildingMaster")]
        public async Task<IActionResult> PatchBuildingMaster(int id, [FromBody] JsonPatchDocument<BuildingMaster> patchDocument)
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
            building.UpdatedDate = DateTime.Now.ToString();
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

        [HttpPatch("PatchFloorMaster")]
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
            floor.UpdatedDate = DateTime.Now.ToString();
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

        [HttpPatch("PatchLandlordDetails")]
        public async Task<IActionResult> PatchLandlordDetails(int id, [FromBody] JsonPatchDocument<LandlordDetails> patchDocument)
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
            landlord.UpdatedDate = DateTime.Now.ToString();
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

        [HttpPatch("PatchOwnerMaster")]
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
            owner.UpdatedDate = DateTime.Now.ToString();
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

        [HttpPatch("PatchRoomCategoryMaster")]
        public async Task<IActionResult> PatchRoomCategoryMaster(int id, [FromBody] JsonPatchDocument<RoomCategoryMaster> patchDocument)
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
            roomCat.UpdatedDate = DateTime.Now.ToString();
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

        [HttpPatch("PatchRoomMaster")]
        public async Task<IActionResult> PatchRoomMaster(int id, [FromBody] JsonPatchDocument<RoomMaster> patchDocument)
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

            return Ok(new { Code = 200, Message = "Room updated successfully" });
        }

        [HttpPatch("PatchRoomRateMaster")]
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

        [HttpPatch("PatchServicableMaster")]
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
            service.UpdatedDate = DateTime.Now.ToString();
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

            return Ok(new { Code = 200, Message = "Service updated successfully" });
        }

        [HttpPatch("PatchVendorMaster")]
        public async Task<IActionResult> PatchVendorMaster(int id, [FromBody] JsonPatchDocument<VendorMaster> patchDocument)
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
            vendor.UpdatedDate = DateTime.Now.ToString();
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

            return Ok(new { Code = 200, Message = "Vendor updated successfully" });
        }

        [HttpPatch("PatchUser")]
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
            vendor.UpdatedDate = DateTime.Now.ToString();
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

        [HttpPatch("PatchStaff")]
        public async Task<IActionResult> PatchStaff(int id, [FromBody] JsonPatchDocument<StaffManagementMaster> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var staff = await _context.StaffManagementMaster.FindAsync(id);

            if (staff == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(staff, ModelState);
            staff.UpdatedDate = DateTime.Now.ToString();
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

            return Ok(new { Code = 200, Message = "Staff updated successfully" });
        }

        [HttpPatch("PatchPaymentMode")]
        public async Task<IActionResult> PatchPaymentMode(int id, [FromBody] JsonPatchDocument<PaymentMode> patchDocument)
        {
            if (patchDocument == null)
            {
                return Ok(new { Code = 500, Message = "Invalid Data" });

            }

            var mode = await _context.PaymentMode.FindAsync(id);

            if (mode == null)
            {
                return Ok(new { Code = 404, Message = "Data Not Found" });
            }

            patchDocument.ApplyTo(mode, ModelState);
            mode.UpdatedDate = DateTime.Now.ToString();
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

            return Ok(new { Code = 200, Message = "Payment Mode updated successfully" });
        }

        [HttpPatch("PatchGroup")]
        public async Task<IActionResult> PatchGroup(int id, [FromBody] JsonPatchDocument<GroupMaster> patchDocument)
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
            group.UpdatedDate = DateTime.Now.ToString();
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

        [HttpPatch("PatchSubGroup")]
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
            group.UpdatedDate = DateTime.Now.ToString();
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

            return Ok(new { Code = 200, Message = "SubGroup updated successfully" });
        }

        //-----------------------------
        //POST APIS
        //-----------------------------

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
                SetClusterDefaults(cm, companyId,userId);

                _context.ClusterMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Cluster Created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId,userId);

                _context.CompanyDetails.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Company created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.BedTypeMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Bed Type created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.BuildingMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Building created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.FloorMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Floor created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.LandlordDetails.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Landlord created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

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
                SetClusterDefaults(cm, companyId, userId);

                _context.OwnerMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Owner created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.RoomCategoryMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room Category created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.RoomMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Room created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.ServicableMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Service created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.VendorMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Vendor created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.UserCreation.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpPost("AddStaff")]
        public async Task<IActionResult> AddStaff([FromBody] StaffManagementMasterDTO staff)
        {
            if (staff == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<StaffManagementMaster>(staff);
                SetClusterDefaults(cm, companyId, userId);

                _context.StaffManagementMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Staff created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }

        [HttpPost("AddPaymentMode")]
        public async Task<IActionResult> AddPaymentMode([FromBody] PaymentModeDTO mode)
        {
            if (mode == null)
                return BadRequest(new { Code = 400, Message = "Invalid data", Data = Array.Empty<object>() });

            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);

                var cm = _mapper.Map<PaymentMode>(mode);
                SetClusterDefaults(cm, companyId, userId);

                _context.PaymentMode.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Payment Mode created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.GroupMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "Group created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
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
                SetClusterDefaults(cm, companyId, userId);

                _context.SubGroupMaster.Add(cm);
                await _context.SaveChangesAsync();

                return Ok(new { Code = 200, Message = "SubGroup created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, ex.Message, Data = Array.Empty<object>() });
            }
        }
    }
}
