﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Repository.DTO;
using Repository.Models;
using RepositoryModels.Repository;
using System.Diagnostics.Metrics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbContextSql _context ;
        private int companyId;
        private string financialYear = string.Empty;
        private int userId;
        public AuthController(DbContextSql context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
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
                isUserExists.UserId = isUserExists.RefUserId;
                return Ok(new { Code = 200, Message = "Logged-In successfully", user = isUserExists });
                
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }
        [HttpGet("GetClusterOrProperty")]
        public async Task<IActionResult> GetClusterOrProperty()
        {
            try
            {
                int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]).ToString() != null ? Convert.ToInt32(HttpContext.Request.Headers["UserId"]) : 0;
                var clustersList = await _context.ClusterMaster.Where(x => x.IsActive == true).ToListAsync();
                var propertiesList = await _context.CompanyDetails.Where(x => x.IsActive == true && x.ClusterId == 0).ToListAsync();

                bool isAnyClusterOrProperty = clustersList.Count > 0 || propertiesList.Count > 0;

                var isUserExists = await _context.UserDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.UserId == userId);
                if (isUserExists == null)
                {
                    return Ok(new { Code = 500, Message = "User not found" });
                }
                //if ()
                if (isUserExists.Roles == Constants.Constants.SuperAdmin)
                {
                    if (clustersList.Count == 0 && propertiesList.Count == 0)
                    {
                        return Ok(new { Code = 404, Message = "No data found" });
                    }
                    else
                    {
                        return Ok(new { Code = 200, Message = "Data fetched successfully", clustersList, propertiesList, isAnyClusterOrProperty });
                    }
                }
                else
                {
                    var authClusterList =  (from cluster in clustersList
                                                 join auth in _context.UserPropertyMapping on cluster.ClusterId equals auth.ClusterId
                                                 where auth.IsActive == true && auth.UserId == userId
                                                 select new ClusterMaster
                                                 {
                                                     ClusterId = cluster.ClusterId,
                                                     ClusterName = cluster.ClusterName,
                                                     ClusterDescription = cluster.ClusterDescription,
                                                     ClusterLocation = cluster.ClusterDescription,
                                                     NoOfProperties = cluster.NoOfProperties
                                                 }).ToList();


                    var authPropertyList = (from property in propertiesList
                                            join auth in _context.UserPropertyMapping on property.PropertyId equals auth.PropertyId
                                            where auth.IsActive == true && auth.UserId == userId
                                            select new CompanyDetails
                                            {
                                                PropertyId = property.PropertyId,
                                                CompanyName = property.CompanyName
                                            }).ToList();

                    return Ok(new { Code = 200, Message = "Data fetched successfully", clustersList = authClusterList, propertiesList = authPropertyList , isAnyClusterOrProperty });

                }
            }
            catch (Exception ex)
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
                    //if(isUserExists.Roles == Constants.Constants.SuperAdmin)
                        if (true)
                        {
                        if (clusterId == 0)
                        {
                            var propertiesList = await _context.CompanyDetails.Where(x => x.IsActive == true).Select(x => new
                            {
                                PropertyId = x.PropertyId,
                                PropertyName = x.CompanyName,
                                PropertyAddress = x.CompanyAddress,
                                PropertyLogo = x.PropertyLogo,
                                Gstin = x.Gstin,
                                CheckInTime = x.CheckInTime,
                                CheckOutTime = x.CheckOutTime,
                                CheckOutFormat = x.CheckOutFormat,
                                IsCheckOutApplicable = x.IsCheckOutApplicable,
                                IsRoomRateEditable = x.IsRoomRateEditable,
                                GstType = x.GstType,
                                ApproveReservation = x.ApproveReservation,
                                x.HotelTagline,
                                x.ContactNo1,
                                x.Email,
                                x.CancelCalculatedBy,
                                x.CancelMethod,
                                x.CheckOutInvoice,
                                x.IsEarlyCheckInPolicyEnable,
                                x.IsDefaultCheckInTimeApplicable,
                                x.IsLateCheckOutPolicyEnable,
                                x.IsDefaultCheckOutTimeApplicable,
                                x.IsEmailNotification,
                                x.IsWhatsappNotification,
                                x.CheckoutWithBalance,
                                x.ReservationNotification,
                                x.CheckinNotification,
                                x.CheckOutNotification,
                                x.RoomShiftNotification,
                                x.CancelBookingNotification
                            }).ToListAsync();

                            return Ok(new { Code = 200, Message = "Property found successfully", data = propertiesList });
                        }

                        else
                        {
                            var propertiesList = await _context.CompanyDetails.Where(x => x.ClusterId == clusterId && x.IsActive == true).Select(x => new
                            {
                                PropertyId = x.PropertyId,
                                PropertyName = x.CompanyName,
                                PropertyAddress = x.CompanyAddress,
                                PropertyLogo = x.PropertyLogo,
                                Gstin = x.Gstin,
                                CheckInTime = x.CheckInTime,
                                CheckOutTime = x.CheckOutTime,
                                CheckOutFormat = x.CheckOutFormat,
                                IsCheckOutApplicable = x.IsCheckOutApplicable,
                                IsRoomRateEditable = x.IsRoomRateEditable,
                                GstType = x.GstType,
                                ApproveReservation = x.ApproveReservation,
                                x.HotelTagline,
                                x.ContactNo1,
                                x.Email,
                                x.CancelCalculatedBy,
                                x.CancelMethod,
                                x.CheckOutInvoice,
                                x.IsEarlyCheckInPolicyEnable,
                                x.IsDefaultCheckInTimeApplicable,
                                x.IsLateCheckOutPolicyEnable,
                                x.IsDefaultCheckOutTimeApplicable,
                                x.IsEmailNotification,
                                x.IsWhatsappNotification,
                                x.CheckoutWithBalance,
                                x.CalculateRoomRates,
                                x.ReservationNotification,
                                x.CheckinNotification,
                                x.CheckOutNotification,
                                x.RoomShiftNotification,
                                x.CancelBookingNotification
                            }).ToListAsync();

                            if(propertiesList.Count == 0)
                            {
                                return Ok(new { Code = 404, Message = "No data found", data = propertiesList });
                            }
                            return Ok(new { Code = 200, Message = "Property found successfully", data = propertiesList });
                        }
                           
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
                                                            PropertyAddress = prop.CompanyAddress,
                                                            PropertyLogo = prop.PropertyLogo,
                                                            Gstin = prop.Gstin,
                                                            CheckInTime = prop.CheckInTime,
                                                            CheckOutTime = prop.CheckOutTime,
                                                            CheckOutFormat = prop.CheckOutFormat,
                                                            IsCheckOutApplicable = prop.IsCheckOutApplicable,
                                                            IsRoomRateEditable= prop.IsRoomRateEditable,
                                                            GstType = prop.GstType,
                                                            prop.HotelTagline,
                                                            prop.ContactNo1,
                                                            prop.Email,
                                                            prop.CancelCalculatedBy,
                                                            prop.CancelMethod,
                                                            prop.CheckOutInvoice,
                                                            prop.IsEarlyCheckInPolicyEnable,
                                                            prop.IsDefaultCheckInTimeApplicable,
                                                            prop.IsLateCheckOutPolicyEnable,
                                                            prop.IsDefaultCheckOutTimeApplicable,
                                                            prop.IsEmailNotification,
                                                            prop.IsWhatsappNotification
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
                                                            PropertyAddress = prop.CompanyAddress,
                                                            PropertyLogo = prop.PropertyLogo,
                                                            Gstin = prop.Gstin,
                                                            CheckInTime = prop.CheckInTime,
                                                            CheckOutTime = prop.CheckOutTime,
                                                            CheckOutFormat = prop.CheckOutFormat,
                                                            IsCheckOutApplicable = prop.IsCheckOutApplicable,
                                                            IsRoomRateEditable = prop.IsRoomRateEditable,
                                                            GstType = prop.GstType,
                                                            prop.HotelTagline,
                                                            prop.ContactNo1,
                                                            prop.Email,
                                                            prop.CancelCalculatedBy,
                                                            prop.CancelMethod,
                                                            prop.CheckOutInvoice,
                                                            prop.IsEarlyCheckInPolicyEnable,
                                                            prop.IsDefaultCheckInTimeApplicable,
                                                            prop.IsLateCheckOutPolicyEnable,
                                                            prop.IsDefaultCheckOutTimeApplicable,
                                                            prop.IsEmailNotification,
                                                            prop.IsWhatsappNotification
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


        [HttpGet("GetPropertyAuthFormData")]
        public async Task<IActionResult> GetPropertyAuthFormData()
        {
            try
            {
                var users = await _context.UserDetails.Where(x => x.IsActive == true).Select(x => new
                {
                    UserId = x.UserId,
                    UserName = x.UserName
                }).ToListAsync();

                //var clusters = await _context.ClusterMaster.Where(x => x.IsActive == true).Select(x => new
                //{
                //    ClusterId = x.ClusterId,
                //    ClusterName = x.ClusterName,
                //    ClusterLocation = x.ClusterLocation,
                //    ClusterItem = x.ClusterName + " : " + x.ClusterLocation
                //}).ToListAsync();
                

                //var properties = await _context.CompanyDetails.Where(x => x.IsActive == true).Select(x => new
                //{
                //    PropertyId = x.PropertyId,
                //    PropertyName = x.CompanyName,
                //    ClusterId = x.ClusterId
                //}).ToListAsync();

                var result = new
                {
                    Users = users,
                    //Clusters = clusters,
                    //PropertiesWithOutCluster = properties.Where(x=>x.ClusterId == 0).ToList(),
                    //Properties = properties.Where(x => x.ClusterId > 0).ToList(),
                };

                return Ok(new { Code = 200, Message = "Data fetch successfully", data = result });
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });

            }
        }


        [HttpGet("GetUsersProperty")]
        public async Task<IActionResult> GetUsersProperty()
        {
            try
            {
                var userClusters = await (
    from up in _context.UserPropertyMapping
    join u in _context.UserDetails on up.UserId equals u.UserId
    join c in _context.ClusterMaster on up.ClusterId equals c.ClusterId
    where up.IsActive == true
        && up.ClusterId != 0
        && u.IsActive == true
        && c.IsActive == true
    group new { up, u, c } by new { up.ClusterId, up.UserId, c.ClusterName, u.UserName, up.AllProperties } into g
    orderby g.Key.UserName
    select new
    {
        Id = g.Key.AllProperties ? g.Max(x => x.up.Id) : 0,
        ClusterId = g.Key.ClusterId,
        UserId = g.Key.UserId,
        ClusterName = g.Key.ClusterName,
        UserName = g.Key.UserName
    }
).ToListAsync();



                var userProperties = await (from userproperty in _context.UserPropertyMapping
                                          join user in _context.UserDetails on userproperty.UserId equals user.UserId
                                          join property in _context.CompanyDetails on userproperty.PropertyId equals property.PropertyId
                                          where userproperty.IsActive == true && userproperty.ClusterId == 0 && property.IsActive == true && user.IsActive == true
                                          select new
                                          {
                                              Id = userproperty.Id,
                                              UserId = userproperty.UserId,
                                              UserName = user.UserName,
                                              PropertyId = property.PropertyId,
                                              PropertyName = property.CompanyName
                                          }).ToListAsync();

                var result = new
                {
                    UserClusters = userClusters,
                    UserProperties = userProperties
                };
                return Ok(new { Code = 200, Message = "Data fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpGet("GetUsersPropertyByClusterId")]
        public async Task<IActionResult> GetUsersPropertyByClusterId(int clusterId, int userId)
        {
            try
            {


                //var temp = await (from userproperty in _context.UserPropertyMapping
                //                            join user in _context.UserDetails on userproperty.UserId equals user.UserId

                //                            where userproperty.IsActive == true && userproperty.ClusterId == clusterId &&  user.IsActive == true
                //                            select new
                //                            {
                //                                Id = userproperty.Id,
                //                                UserId = userproperty.UserId,
                //                                UserName = user.UserName,
                //                                ClusterId = userproperty.ClusterId,
                //                                PropertyId = 0,
                //                                PropertyName =  "", 
                //                                AllProperties = userproperty.AllProperties
                //                            }).ToListAsync();

                //var result = temp.Select(item => new 
                //{
                //    Id = item.Id,
                //    UserId = item.UserId,
                //    UserName = item.UserName,
                //    ClusterId = item.ClusterId,
                //    PropertyId = 0,
                //    PropertyName = "",
                //    AllProperties = item.AllProperties
                //}).ToList(); ;
                //foreach(var item in temp)
                //{
                //    if(item.AllProperties == true)
                //    {
                //        var properties = await _context.CompanyDetails.Where(x => x.IsActive == true && x.ClusterId == item.ClusterId).ToListAsync();
                //        result.Remove(item);
                //        foreach(var p in properties)
                //        {
                //            var data = new
                //            {
                //                Id = 0,
                //                UserId = item.UserId,
                //                UserName = item.UserName,
                //                ClusterId = item.ClusterId,
                //                PropertyId = p.PropertyId,
                //                PropertyName = p.CompanyName ,
                //                AllProperties = item.AllProperties
                //            };
                //            result.Add(data);
                //        }




                //    }
                //}


                var result = await (
    from up in _context.UserPropertyMapping
    where up.IsActive == true
        && up.UserId == userId
        && up.ClusterId == clusterId
    from p in _context.CompanyDetails
        .Where(p =>
            p.IsActive == true &&
            (
                (up.AllProperties == true && up.ClusterId == p.ClusterId) ||
                (up.AllProperties != true && up.PropertyId == p.PropertyId)
            )
        )
        .DefaultIfEmpty() // LEFT JOIN
    select new
    {
        Id = up.PropertyId == 0 ? 0 : up.Id,
        PropertyName = p.CompanyName
    }
).ToListAsync();




                return Ok(new { Code = 200, Message = "Data fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetUsersPropertyById")]
        public async Task<IActionResult> GetUsersPropertyById(int userId)
        {
            try
            {
                var userPropertyMapping = await _context.UserPropertyMapping.Where(x => x.IsActive == true && x.UserId == userId).ToListAsync();

                var disabledClusterIds = userPropertyMapping
                        .Where(u => u.AllProperties)
                        .Select(u => u.ClusterId)
                        .ToHashSet();

                var disabledPropertyIds = userPropertyMapping
                    .Select(u => u.PropertyId)
                    .ToHashSet();

                

                var properties = await _context.CompanyDetails.Where(x => x.IsActive == true).Select(x => new
                {
                    PropertyId = x.PropertyId,
                    PropertyName = x.CompanyName,
                    ClusterId = x.ClusterId,
                    IsDisabled = disabledPropertyIds.Contains(x.PropertyId)
                }).ToListAsync();

                var cluster = await _context.ClusterMaster
    .Where(x => x.IsActive)
    .ToListAsync();

                var clusters = cluster
    .Select(x => new
    {
        ClusterId = x.ClusterId,
        ClusterName = x.ClusterName,
        ClusterLocation = x.ClusterLocation,
        ClusterItem = x.ClusterName + " : " + x.ClusterLocation,
        IsDisabled = disabledClusterIds.Contains(x.ClusterId) ||
                     properties.Where(p => p.ClusterId == x.ClusterId).All(p => p.IsDisabled)
    })
    .ToList();

                var result = new
                {
                    UserPropertyMapping = userPropertyMapping,
                    Clusters = clusters,
                    PropertiesWithOutCluster = properties.Where(x => x.ClusterId == 0).ToList(),
                    Properties = properties.Where(x => x.ClusterId > 0).ToList(),
                };

                return Ok(new { Code = 200, Message = "Data fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpPost("SavePropertyAuth")]
        public async Task<IActionResult> SavePropertyAuth([FromBody] List<PropertyAuthDto> dto)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentDate = DateTime.Now;
                if(dto.Count == 0)
                {
                    return Ok(new { Code = 500, Message = "Invalid data" });
                }


                foreach(var item in dto)
                {
                    //check for is all property
                    if (item.AllProperty)
                    {
                        var anyProperty = await _context.UserPropertyMapping.Where(x => x.IsActive == true && x.UserId == item.UserId && x.ClusterId == item.ClusterId).ToListAsync();

                        if (anyProperty.Count > 0)
                        {
                            _context.UserPropertyMapping.RemoveRange(anyProperty);
                            await _context.SaveChangesAsync();
                        }
                        UserPropertyMapping mapping = new UserPropertyMapping
                        {
                            UserId = item.UserId,
                            PropertyId = item.PropertyId,
                            ClusterId = item.ClusterId,
                            CreatedBy = userId,
                            IsActive = true,
                            AllProperties = item.AllProperty,
                            CreatedDate = currentDate,
                            UpdatedDate = currentDate,
                            CompanyId = companyId,

                        };



                        await _context.UserPropertyMapping.AddAsync(mapping);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var isExists = await _context.UserPropertyMapping.Where(x => x.IsActive == true && x.UserId == item.UserId && x.PropertyId == item.PropertyId && x.ClusterId == item.ClusterId).FirstOrDefaultAsync();

                        if (isExists == null)
                        {
                            UserPropertyMapping mapping = new UserPropertyMapping
                            {
                                UserId = item.UserId,
                                PropertyId = item.PropertyId,
                                ClusterId = item.ClusterId,
                                CreatedBy = userId,
                                IsActive = true,
                                AllProperties = item.AllProperty,
                                CreatedDate = currentDate,
                                UpdatedDate = currentDate,
                                CompanyId = companyId,

                            };



                            await _context.UserPropertyMapping.AddAsync(mapping);
                            await _context.SaveChangesAsync();
                        }
                    }
                   
                }

                await transaction.CommitAsync();
                return Ok(new { Code = 200, Message = "User assigned to property successfully." });

            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

        [HttpDelete("DeletePropertyAuth")]
        public async Task<IActionResult> DeletePropertyAuth(int id)
        {
            try
            {
                if(id == 0)
                {
                    return Ok(new { Code = 400, Message = "Invalid data" });
                }
                var isExists = await _context.UserPropertyMapping.Where(x => x.IsActive == true && x.Id == id).FirstOrDefaultAsync();

                if(isExists == null)
                {
                    return Ok(new { Code = 400, Message = "Data not found" });
                }

                isExists.UpdatedDate = DateTime.Now;
                isExists.IsActive = false;

                _context.UserPropertyMapping.Update(isExists);
                await _context.SaveChangesAsync();


                return Ok(new { Code = 200, Message = "Record deleted successfully" });
            }
            catch(Exception ex)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

    }
}
