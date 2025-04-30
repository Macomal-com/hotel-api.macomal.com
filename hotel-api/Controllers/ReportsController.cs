using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Repository.Models;
using RepositoryModels.Repository;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.Data.SqlClient;
using System.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Filters;
namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly DbContextSql _context;
        private int companyId;
        private int userId;
        public ReportsController(DbContextSql contextSql,  IHttpContextAccessor httpContextAccessor)
        {
            _context = contextSql;
           
            var headers = httpContextAccessor.HttpContext?.Request?.Headers;
            if (headers != null)
            {
                if (headers.TryGetValue("CompanyId", out var companyIdHeader) &&
           int.TryParse(companyIdHeader, out int comp))
                {
                    this.companyId = comp;
                }


                

                if (headers.TryGetValue("UserId", out var userIdHeader) &&
                int.TryParse(companyIdHeader, out int id))
                {
                    this.userId = id;
                }
            }


        }




        [HttpGet("GetInvoiceHistoryReport")]
        public async Task<IActionResult> GetInvoiceHistoryReport(DateTime fromDate, DateTime toDate)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            DataSet? dataSet = null;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("invoice_history_list", connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@companyId", companyId);
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                        command.Parameters.AddWithValue("@toDate", toDate);
                        await connection.OpenAsync();

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            dataSet = new DataSet();
                            adapter.Fill(dataSet);
                        }
                        await connection.CloseAsync();
                    }
                }
                return Ok(new { Code = 200, Message = "Reservation Fetched Successfully", data = dataSet });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetReports")]
        public async Task<IActionResult> GetReports(string status)
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            DataSet? dataSet = null;
            string spName = "";
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    if (status == "Pending")
                    {
                        spName = "PendingReservations";
                    }
                    else if (status == "CheckIn")
                    {
                        spName = "CheckedInReservations";
                    }
                    else if (status == "PaymentList")
                    {
                        spName = "paymentList";
                    }
                    else if (status == "searchPanel")
                    {
                        spName = "searchPanel";
                    }
                    else if (status == "reservationList")
                    {
                        spName = "reservationList";
                    }
                    else if (status == "rejectedBookingList")
                    {
                        spName = "rejectedBookingList";
                    }
                    else if (status == "cancelBookingList")
                    {
                        spName = "cancelBooking";
                    }
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@companyId", companyId);
                        await connection.OpenAsync();

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            dataSet = new DataSet();
                            adapter.Fill(dataSet);
                        }
                        await connection.CloseAsync();
                    }
                }
                return Ok(new { Code = 200, Message = "Reservation Fetched Successfully", data = dataSet });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

    }
}
