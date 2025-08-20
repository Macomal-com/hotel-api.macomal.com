using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.ReportModels;
using RepositoryModels.Repository;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
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
        public ReportsController(DbContextSql contextSql, IHttpContextAccessor httpContextAccessor)
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
                int.TryParse(userIdHeader, out int id))
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

                return Ok(new { Code = 200, Message = "Data Fetched Successfully", data = dataTable, columns = columnNames, filteredData = filteredDataTable, dynamicActionJs = dynamicActionJs, hasMore = totalNoOfRows > request.EndPageNumber ? true : false, totalTable = totalTable });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = ex.Message });
            }
        }


        private string ColumnAlignment(string datatype)
        {
            string alignment = "left";
            switch (datatype)
            {
                case "System.String":
                    // Handle string
                    break;

                case "System.Int32":
                    alignment = "right";
                    break;

                case "System.Int64":
                    alignment = "right";
                    break;

                case "System.Int16":
                    alignment = "right";
                    break;

                case "System.Byte":
                    // Handle byte
                    break;

                case "System.Boolean":
                    // Handle bool
                    break;

                case "System.DateTime":
                    // Handle DateTime
                    break;

                case "System.Decimal":
                    alignment = "right";
                    break;

                case "System.Double":
                    alignment = "right";
                    break;

                case "System.Single":
                    alignment = "right";
                    break;

                case "System.Guid":
                    // Handle Guid
                    break;

                case "System.Char":
                    // Handle char
                    break;

                case "System.Byte[]":
                    // Handle byte array (e.g., file/image)
                    break;

                default:
                    // Handle unknown type
                    break;
            }

            return alignment;

        }

        [HttpPost("GetReports2")]
        public async Task<IActionResult> GetReports2([FromBody] ReportRequestBody request)
        {
            ReportResponse response = new ReportResponse
                ();
            try
            {
                DataSet dataSet = null;
                DataTable? dataTable = null;
                List<HeaderAndFooter> columnNames = new List<HeaderAndFooter>();
                DataTable? filteredDataTable = null;
                int totalRows = 0;
                // Dictionary<string, string> dynamicActionJs = new Dictionary<string, string>();
                DataTable? totalTable = null;
                //int totalNoOfRows = 0;
                //if (request.IsFirstRequest)
                //{
                //    dynamicActionJs = (await _context.DynamicActionJs
                //    .Where(x => x.ReportName == request.ReportName)
                //    .ToListAsync())
                //    .ToDictionary(x => x.ActionName, x => x.ActionJs);
                //}
                string searchFilters = "";
                if (request.SearchFilters.Count > 0)
                {
                    searchFilters = GetSearchFilter(request.SearchFilters);
                }
                



                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("Reports", connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@reportName", request.ReportName);
                        command.Parameters.AddWithValue("@startPageNumber", request.StartPageNumber);
                        command.Parameters.AddWithValue("@endPageNumber", request.EndPageNumber);
                        command.Parameters.AddWithValue("@searchFilters", searchFilters);

                        command.Parameters.AddWithValue("@companyId", companyId);
                        
                        command.Parameters.AddWithValue("@startDate", request.StartDate);
                        command.Parameters.AddWithValue("@endDate", request.EndDate);

                        // Add return value parameter
                        SqlParameter totalRowsParam = new SqlParameter("@TotalRows", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(totalRowsParam);

                        await connection.OpenAsync();

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            dataSet = new DataSet();
                            adapter.Fill(dataSet);
                        }

                        totalRows = (int)totalRowsParam.Value;
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
                                DataColumnCollection columns = totalTable.Columns.Count > 0 ? totalTable.Columns : dataTable.Columns;
                                
                                    DataRow firstRow = totalTable.Rows.Count > 0 ? totalTable.Rows[0] : null;
                                    foreach (DataColumn column in columns)
                                    {
                                        HeaderAndFooter columnsData = new HeaderAndFooter();
                                        string input = column.ColumnName;
                                        int splitIndex = input.TakeWhile(c => char.IsLetterOrDigit(c)).Count();

                                        columnsData.AccessorKey = splitIndex == input.Length
    ? input.ToLower() // no special character found
    : input.Substring(0, splitIndex).ToLower() + input.Substring(splitIndex);
                                        columnsData.Header = column.ColumnName;
                                        columnsData.ColumnType = column.DataType.ToString();
                                        columnsData.muiTableHeadCellProps.Align = ColumnAlignment(column.DataType.ToString());
                                        columnsData.muiTableBodyCellProps.Align = ColumnAlignment(column.DataType.ToString());
                                        columnsData.Footer = firstRow == null ? "" : firstRow[column.ColumnName]?.ToString() ?? ""; // Safely handle nulls
                                        columnNames.Add(columnsData);
                                    }
                                }
                                else
                                {
                                    foreach (DataColumn column in filteredDataTable.Columns)
                                    {
                                        HeaderAndFooter columnsData = new HeaderAndFooter();
                                        columnsData.AccessorKey = char.ToLower(column.ColumnName[0]) + column.ColumnName.Substring(1);
                                        columnsData.Header = column.ColumnName;
                                        columnsData.ColumnType = column.DataType.ToString();
                                        columnNames.Add(columnsData);
                                    }
                                }

                            


                        }
                    }
                }

                response.ReportData = dataTable;
                response.Columns = columnNames;
                response.FilteredData = filteredDataTable;
                response.TotalTable = totalTable;
                response.HasMore = totalRows > request.EndPageNumber ? true : false;
                response.TotalRows = totalRows;


                return Ok(new { Code = 200, Message = "Data Fetched Successfully", data = response });

            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = ex.Message });
            }
        }


        private string GetSearchFilter(List<SearchFilters> searchFilters)
        {
            StringBuilder filter = new StringBuilder("where ");
            for (int i = 0; i < searchFilters.Count; i++)
            {
                if (i == 0)
                {
                    filter = filter.Append($"[{searchFilters[i].Id}] like '%{searchFilters[i].Value}%'");
                }
                else
                {
                    filter = filter.Append($" and [{searchFilters[i].Id}] like '%{searchFilters[i].Value}%'");
                }
            }
            return filter.ToString();
        }

        [HttpPost("GetRoomAvailabilityExcel")]
        public async Task<IActionResult> GetRoomAvailabilityExcel([FromBody] RoomAvailabilityExcel request)
        {
            try
            {
                List<int> CompanyId = new List<int>();

                var isUserExists = await _context.UserDetails.FirstOrDefaultAsync(x => x.IsActive == true && x.UserId == userId);

                if (isUserExists == null)
                {
                    return Ok(new { Code = 400, Message = "User not found" });
                }

                foreach (KeyValuePair<int, bool> kvp in request.ClusterIds)
                {
                    if (isUserExists.Roles == Constants.Constants.SuperAdmin || kvp.Value == true)
                    {
                        var properties = await _context.CompanyDetails.Where(x => x.IsActive == true && x.ClusterId == kvp.Key).ToListAsync();

                        foreach (var item in properties)
                        {
                            CompanyId.Add(item.PropertyId);
                        }
                    }
                    else
                    {
                        var properties = await (from p in _context.CompanyDetails
                                                join u in _context.UserPropertyMapping on p.PropertyId equals u.PropertyId
                                                where p.IsActive == true && p.ClusterId == kvp.Key && u.UserId == userId
                                                select new
                                                {
                                                    p.PropertyId
                                                }).ToListAsync();

                        foreach (var item in properties)
                        {
                            CompanyId.Add(item.PropertyId);
                        }
                    }
                }


                var writer = new StringWriter();
                var serializer = new XmlSerializer(typeof(List<int>));
                serializer.Serialize(writer, CompanyId);
                string xml = writer.ToString();

                DataSet dataSet = new DataSet();
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("GetRoomAvailabilityExcel", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@startDate", request.StartDate);
                        command.Parameters.AddWithValue("@endDate", request.EndDate);
                        command.Parameters.AddWithValue("@xml", xml);

                        await connection.OpenAsync();

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataSet);
                        }
                        await connection.CloseAsync();
                    }
                    MemoryStream? stream = await ExcelExport(dataSet, request.StartDate, request.EndDate, CompanyId);
                    if (stream == null)
                    {
                        return Ok("Error while creating excel");
                    }
                    else
                    {
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Room Availability.xlsx");
                    }


                }
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = ex.Message });
            }
        }


        private async Task<MemoryStream?> ExcelExport(DataSet dataSet, string startDate, string endDate, List<int> companyIds)
        {
            MemoryStream stream = null;


            try
            {
                string clusterName = "";
                var properties = await (from p in _context.CompanyDetails
                                        join c in _context.ClusterMaster on p.ClusterId equals c.ClusterId
                                        where p.IsActive == true && c.IsActive == true
                                        select new
                                        {
                                            p.PropertyId,
                                            c.ClusterId,
                                            c.ClusterName,
                                            p.CompanyName
                                        }).ToListAsync();

                //get service status
                var serviceStatusDict = await _context.ServicesStatus
                                        .ToDictionaryAsync(s => s.RoomStatus.Trim(), s => s.Colour.Trim());

                Assembly asm = Assembly.GetExecutingAssembly();
                string path = System.IO.Path.GetDirectoryName(asm.Location);
                string filePath = path + "\\" + "Room Avaialability" + ".xlsx";

                XLWorkbook wb = new XLWorkbook();
                IXLWorksheet worksheet = wb.Worksheets.Add("Room Avaialability"); ;
                int currentRow = 1;



                //Heading
                string headerText = $"Room Availability ({startDate} - {endDate})";
                worksheet.Row(currentRow).Cell(1).Value = headerText;
                worksheet.Row(currentRow).Height = worksheet.Row(currentRow).Height + 15;
                worksheet.Row(currentRow).Style.Alignment.WrapText = true;
                worksheet.Row(currentRow).Cell(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(currentRow).Cell(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Row(currentRow).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Row(currentRow).Style.Border.OutsideBorderColor = XLColor.Black;
                currentRow++;

                int count = 0;

                //insert table 
                for (int t = 0; t < dataSet.Tables.Count; t++)
                {
                    DataTable table = dataSet.Tables[t];
                    count = table.Columns.Count;
                    if (table.Rows.Count == 0)
                    {
                        continue;
                    }
                    string? propertyName = properties.Where(x => x.PropertyId == companyIds[t]).Select(x => x.CompanyName).FirstOrDefault();
                    string? currentClusterName = properties.Where(x => x.PropertyId == companyIds[t]).Select(x => x.ClusterName).FirstOrDefault();

                    if (currentClusterName != null && currentClusterName != clusterName)
                    {


                        clusterName = currentClusterName;
                        //Cluster name
                        worksheet.Row(currentRow).Cell(1).Value = clusterName;
                        worksheet.Row(currentRow).Style.Alignment.WrapText = true;
                        worksheet.Row(currentRow).Cell(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Row(currentRow).Style.Font.Bold = true;
                        worksheet.Row(currentRow).Style.Font.FontColor = XLColor.Brown;
                        worksheet.Row(currentRow).Height = worksheet.Row(currentRow).Height + 10;
                        worksheet.Range(currentRow, 1, currentRow, count).Merge().SetValue(clusterName);


                        currentRow++;
                    }

                    //Property name
                    worksheet.Row(currentRow).Cell(1).Value = propertyName;
                    worksheet.Row(currentRow).Style.Alignment.WrapText = true;
                    worksheet.Row(currentRow).Cell(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Row(currentRow).Style.Font.Bold = true;
                    worksheet.Row(currentRow).Style.Font.FontColor = XLColor.AppleGreen;
                    worksheet.Row(currentRow).Height = worksheet.Row(currentRow).Height + 5;
                    worksheet.Range(currentRow, 1, currentRow, count).Merge().SetValue(propertyName);
                    currentRow++;


                    //Columns
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = table.Columns[i].ColumnName;
                        worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, i + 1).Style.Font.FontSize = 10;

                    }

                    //worksheet.Range(currentRow, 1, currentRow, table.Columns.Count).Style.Fill.BackgroundColor = XLColor.FromArgb(79, 129, 189);  // Same as #4F81BD
                    //worksheet.Range(currentRow, 1, currentRow, table.Columns.Count).Style
                    //    .Font.Bold = true;
                    //worksheet.Range(currentRow, 1, currentRow, table.Columns.Count).Style.Font.FontColor = XLColor.White;

                    currentRow++;



                    var excelTable = worksheet.Cell(currentRow, 1).InsertData(table);

                    // Loop through all cells in the inserted range
                    foreach (var cell in excelTable.Cells())
                    {
                        string cellValue = cell.Value.ToString().Trim();
                        if (cellValue != "")
                        {
                            //dirty
                            if (cellValue == Constants.Constants.Dirty)
                            {
                                var color = serviceStatusDict.GetValueOrDefault(cellValue);
                                cell.Style.Font.Bold = true;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Fill.BackgroundColor = color != null ? XLColor.FromHtml(color) : XLColor.Black;
                            }
                            //under repair
                            else if (cell.Value.ToString().Trim() == Constants.Constants.UnderRepair)
                            {
                                var color = serviceStatusDict.GetValueOrDefault(cellValue);
                                cell.Style.Font.Bold = true;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Fill.BackgroundColor = color != null ? XLColor.FromHtml(color) : XLColor.Black;
                            }
                            //management block
                            else if (cell.Value.ToString().Trim() == Constants.Constants.ManagementBlock)
                            {
                                var color = serviceStatusDict.GetValueOrDefault(cellValue);
                                cell.Style.Font.Bold = true;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Fill.BackgroundColor = color != null ? XLColor.FromHtml(color) : XLColor.Black;
                            }
                            //dnd
                            else if (cell.Value.ToString().Trim() == Constants.Constants.Dnd)
                            {
                                var color = serviceStatusDict.GetValueOrDefault(cellValue);
                                cell.Style.Font.Bold = true;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Fill.BackgroundColor = color != null ? XLColor.FromHtml(color) : XLColor.Black;
                            }
                            //checkin
                            else if (cell.Value.ToString().Trim() == Constants.Constants.CheckIn)
                            {
                                var color = serviceStatusDict.GetValueOrDefault(cellValue);
                                cell.Style.Font.Bold = true;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Fill.BackgroundColor = color != null ? XLColor.FromHtml(color) : XLColor.Black;
                            }
                            //pending
                            else if (cell.Value.ToString().Trim() == Constants.Constants.Pending)
                            {
                                var color = serviceStatusDict.GetValueOrDefault(cellValue);
                                cell.Style.Font.Bold = true;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Fill.BackgroundColor = color != null ? XLColor.FromHtml(color) : XLColor.Black;
                            }
                            //confirmed
                            else if (cell.Value.ToString().Trim() == Constants.Constants.Confirmed)
                            {
                                var color = serviceStatusDict.GetValueOrDefault(cellValue);
                                cell.Style.Font.Bold = true;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Fill.BackgroundColor = color != null ? XLColor.FromHtml(color) : XLColor.Black;
                            }
                        }


                    }

                    currentRow += table.Rows.Count;
                }

                //merge row 1
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, count).Merge().SetValue(headerText);

                //add border to each cell
                var range = worksheet.RangeUsed();
                range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                range.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                //adjust column width
                worksheet.Columns().AdjustToContents();

                stream = new MemoryStream();
                wb.SaveAs(stream);
                return stream;
            }
            catch (Exception ex)
            {
                return stream;
            }


        }

        [HttpPost("GetReportExcel")]
        public async Task<IActionResult> GetReportExcel([FromBody] ReportRequestBody request)
        {
            try
            {
           
               
                string searchFilters = "";
                if (request.SearchFilters.Count > 0)
                {
                    searchFilters = GetSearchFilter(request.SearchFilters);
                }

                DataSet dataSet = new DataSet();
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("Reports", connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@reportName", request.ReportName);
                        command.Parameters.AddWithValue("@startPageNumber", request.StartPageNumber);
                        command.Parameters.AddWithValue("@endPageNumber", request.EndPageNumber);
                        command.Parameters.AddWithValue("@searchFilters", searchFilters);

                        command.Parameters.AddWithValue("@companyId", companyId);

                        command.Parameters.AddWithValue("@startDate", request.StartDate);
                        command.Parameters.AddWithValue("@endDate", request.EndDate);
                        command.Parameters.AddWithValue("@isExcel", "yes");

                        // Add return value parameter
                        SqlParameter totalRowsParam = new SqlParameter("@TotalRows", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(totalRowsParam);

                        await connection.OpenAsync();

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            dataSet = new DataSet();
                            adapter.Fill(dataSet);
                        }

                        MemoryStream? stream;
                        string message = "";    
                            
                        (stream, message)   = await ExcelExport(dataSet, request.StartDate, request.EndDate, request.ReportHeading);
                        if (stream == null)
                        {
                            return Ok(message);
                        }
                        else
                        {
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{request.ReportHeading}.xlsx");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return Ok(new { Code = 500, Message = "Something went wrong" });
            }
        }


        private async Task<(MemoryStream?, string)> ExcelExport(DataSet dataSet, string startDate, string endDate, string reportHeading)
        {
            MemoryStream stream = null;


            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string path = System.IO.Path.GetDirectoryName(asm.Location);
                string filePath = $"{path}\\${reportHeading}.xlsx";

                XLWorkbook wb = new XLWorkbook();
                IXLWorksheet worksheet = wb.Worksheets.Add(reportHeading); ;
                int currentRow = 1;



                //Heading
                DateTime parsedStartDate = DateTime.Parse(startDate);
                DateTime parsedEndDate = DateTime.Parse(endDate);
                string headerText = $"{reportHeading} ({parsedStartDate.ToString("dd-MM-yyyy")} to {parsedEndDate.ToString("dd-MM-yyyy")})";
                worksheet.Row(currentRow).Cell(1).Value = headerText;
                worksheet.Row(currentRow).Height = worksheet.Row(currentRow).Height + 15;
                worksheet.Row(currentRow).Style.Alignment.WrapText = true;

                currentRow++;

                int count = 0;

                //insert columns
                
                DataTable data = dataSet.Tables[0];
                DataTable totalTable = dataSet.Tables.Count > 1 ? dataSet.Tables[1] : new DataTable();
                DataColumnCollection columns = totalTable.Columns.Count > 0 ? totalTable.Columns : data.Columns;
                if (data.Columns.Count > 0)
                {
                    count = data.Columns.Count;
                    //Columns
                    for (int i = 0; i < columns.Count; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = columns[i].ColumnName;
                        worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, i + 1).Style.Font.FontSize = 10;

                    }
                }
                currentRow++;
                var excelTable = worksheet.Cell(currentRow, 1).InsertData(data);

                currentRow += data.Rows.Count;

                var excelTotalTable = worksheet.Cell(currentRow, 1).InsertData(totalTable);
                excelTotalTable.Style.Font.SetBold();
                currentRow += totalTable.Rows.Count;

                //merge row 1
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, count).Merge().SetValue(headerText);
                worksheet.Range(1, 1, 1, count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(1, 1, 1, count).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Range(1, 1, 1, count).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(1, 1, 1, count).Style.Border.OutsideBorderColor = XLColor.Black;

                //add border to each cell
                var range = worksheet.RangeUsed();
                range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                range.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                //adjust column width
                worksheet.Columns().AdjustToContents();

                stream = new MemoryStream();
                wb.SaveAs(stream);
                return (stream, "File created successfully");
            }
            catch (Exception ex)
            {
                return (stream, ex.Message);
            }


        }
    }
}
