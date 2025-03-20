using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Repository.Models;
using RepositoryModels.Repository;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbContextSql _context ;

        public AuthController(DbContextSql context)
        {
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]).ToString() != null ? Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]) : 0 ;

                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]).ToString()!=null ? Convert.ToInt32(HttpContext.Request.Headers["UserId"]) : 0;
                

                var validator = new LoginModalValidator();
                var result = validator.Validate(model);
                if (!result.IsValid)
                {
                    var error = result.Errors.Select(x => new
                    {
                        Error = x.ErrorMessage,
                        Field = x.PropertyName
                    }).ToList();
                    return Ok(new { Code = 400, Message = error });
                }
                var isUserExists = await _context.UserDetails.FirstOrDefaultAsync(x => x.UserName == model.Username && x.Password == model.Password && x.IsActive == true);

                if(isUserExists == null)
                {
                    return Ok(new { Code = 404, Message = "Invalid username or password" });
                }
                var clustersList = await _context.ClusterMaster.Where(x => x.IsActive == true).ToListAsync();
                if (isUserExists.Roles == Constants.Constants.SuperAdmin) 
                {                    
                    //if no cluster found then find properties
                    if(clustersList.Count == 0)
                    {
                        var propertiesList = await _context.CompanyDetails.Where(x => x.IsActive == true).ToListAsync();

                        return Ok(new { Code = 200, Message = "Login successfully", user = isUserExists, clustersList = new List<object>() ,propertiesList = propertiesList });
                    }
                    else
                    {
                        return Ok(new { Code = 200, Message = "Login successfully", user = isUserExists, clustersList = clustersList, propertiesList = new List<object>() });
                    }
                }
                else
                {
                    if (clustersList.Count == 0)
                    {
                        var propertiesList = await (from mapp in _context.UserPropertyMapping
                                                    join prop in _context.CompanyDetails on mapp.PropertyId equals prop.PropertyId
                                                    where mapp.UserId == isUserExists.UserId && mapp.IsActive == true
                                                    select new
                                                    {
                                                        PropertyId = prop.PropertyId,
                                                        CompanyName = prop.CompanyName
                                                    }).ToListAsync();
                        return Ok(new { Code = 200, Message = "Login successfully", user = isUserExists, clustersList = new List<object>(), propertiesList = propertiesList });


                    }
                    else
                    {
                        var propertiesList = await (from mapp in _context.UserPropertyMapping
                                                    join prop in _context.ClusterMaster on mapp.ClusterId equals prop.ClusterId
                                                    where mapp.UserId == isUserExists.UserId && mapp.IsActive == true
                                                    select new
                                                    {
                                                        ClusterId = prop.ClusterId,
                                                        ClusterName = prop.ClusterName
                                                    }).ToListAsync();
                        return Ok(new { Code = 200, Message = "Login successfully", user = isUserExists, clustersList = propertiesList , propertiesList = new List<object>() });
                    }
                   
                }
                    
                
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetPropertiesByUser/{clusterId}")]
        public async Task<IActionResult> GetPropertiesByUser(int clusterId)
        {
            try
            {
                int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]).ToString() != null ? Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]) : 0;

                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]).ToString() != null ? Convert.ToInt32(HttpContext.Request.Headers["UserId"]) : 0;
                var isUserExists = await _context.UserDetails.FirstOrDefaultAsync(x => x.UserId == userId);
                if(isUserExists == null)
                {
                    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
                }
                else
                {
                    if(isUserExists.Roles == Constants.Constants.SuperAdmin)
                    {
                        var propertiesList = await _context.CompanyDetails.Where(x => x.ClusterId == clusterId && x.IsActive == true).Select(x=> new
                        {
                            PropertyId = x.PropertyId,
                            PropertyName = x.CompanyName,
                            PropertyAddress = x.CompanyAddress
                        }).ToListAsync();

                        return Ok(new { Code = 200, Message = "Property found successfully", data = propertiesList });
                    }
                    else
                    {
                        if(clusterId == 0)
                        {
                            var propertiesList = await (from mapp in _context.UserPropertyMapping
                                                        join prop in _context.CompanyDetails on mapp.PropertyId equals prop.PropertyId
                                                        where mapp.UserId == isUserExists.UserId && mapp.IsActive == true
                                                        select new
                                                        {
                                                            PropertyId = prop.PropertyId,
                                                            PropertyName = prop.CompanyName,
                                                            PropertyAddress = prop.CompanyAddress
                                                        }).ToListAsync();

                            return Ok(new { Code = 200, Message = "Property found successfully", data = propertiesList });
                        }
                        else
                        {
                            var propertiesList = await (from mapp in _context.UserPropertyMapping
                                                        join prop in _context.CompanyDetails on mapp.PropertyId equals prop.PropertyId
                                                        where mapp.UserId == isUserExists.UserId && mapp.ClusterId == clusterId && mapp.IsActive == true
                                                        select new
                                                        {
                                                            PropertyId = prop.PropertyId,
                                                            PropertyName = prop.CompanyName,
                                                            PropertyAddress = prop.CompanyAddress
                                                        }).ToListAsync();

                            return Ok(new { Code = 200, Message = "Property found successfully", data = propertiesList });
                        }
                    }
                }

                
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });

            }
        }

    }
}
