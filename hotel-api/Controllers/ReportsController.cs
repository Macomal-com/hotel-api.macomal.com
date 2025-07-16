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
using Repository.ReportModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;
namespace hotel_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly DbContextSql _context;
        private int companyId;
        private int userId;

        private List<string> PendingReservationHideColumns = new List<string> { "Reservation Id" };
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

        [HttpGet("GetAllReservations")]
        public async Task<IActionResult> GetAllReservations()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            DataSet? dataSet = null;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("searchPanel", connection))
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
                return Ok(new { Code = 200, Message = "Reservations Fetched Successfully", data = dataSet });
            }
            catch (Exception)
            {
                return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }


        [HttpGet("GetSearchPanel")]
        public async Task<IActionResult> GetSearchPanel(DateTime fromDate, DateTime toDate)
        {
            
            DataSet? dataSet = null;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("searchPanel", connection))
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

        [HttpPost("GetReports1")]
        public async Task<IActionResult> GetReports1([FromBody] ReportRequest request)
        {
            try
            {
                DataSet dataSet = null;
                DataTable? dataTable = null;
                List<ColumnsData> columnNames = new List<ColumnsData>();
                DataTable? filteredDataTable = null;
                Dictionary<string, string> dynamicActionJs = new Dictionary<string, string>();
                DataTable? totalTable = null;
                int totalNoOfRows = 0;
                if (request.IsFirstRequest)
                {
                    dynamicActionJs = (await _context.DynamicActionJs
                    .Where(x => x.ReportName == request.ReportName)
                    .ToListAsync())
                    .ToDictionary(x => x.ActionName, x => x.ActionJs);
                }
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand(request.SpName, connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@companyId", request.CompanyId);
                        command.Parameters.AddWithValue("@startPageNumber", request.StartPageNumber);
                        command.Parameters.AddWithValue("@endPageNumber", request.EndPageNumber);
                        command.Parameters.AddWithValue("@startDate", request.StartDate);
                        command.Parameters.AddWithValue("@endDate", request.EndDate);
                        SqlParameter returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(returnValueParam);

                        await connection.OpenAsync();

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            dataSet = new DataSet();
                            adapter.Fill(dataSet);
                        }

                        totalNoOfRows = (int)command.Parameters["@ReturnValue"].Value;
                        await connection.CloseAsync();
                        dataTable = dataSet.Tables[0];
                        totalTable = dataSet.Tables.Count > 1 ? dataSet.Tables[1] : new DataTable();
                        filteredDataTable = dataSet.Tables[0].Copy();
                        if (filteredDataTable.Columns.Contains("Id"))
                        {
                            filteredDataTable.Columns.Remove("Id");
                        }

                        if (filteredDataTable != null)
                        {
                            if (request.IsFirstRequest)
                            {
                                foreach (DataColumn column in filteredDataTable.Columns)
                                {
                                    ColumnsData columnsData = new ColumnsData();
                                    columnsData.ColumnName = column.ColumnName;
                                    columnsData.ColumnType = column.DataType.ToString();
                                    columnNames.Add(columnsData);
                                }
                            }

                               
                        }
                    }
                }

                return Ok(new { Code = 200, Message = "Data Fetched Successfully", data = dataTable, columns = columnNames,  filteredData = filteredDataTable, dynamicActionJs = dynamicActionJs, hasMore = totalNoOfRows > request.EndPageNumber ? true : false, totalTable = totalTable });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = ex.Message });
            }
        }

    }
}
