﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System.Data;

namespace hotel_api.Controllers
{

    

    public class CategoryWiseCount
    {
        public string Name { get; set; } = string.Empty; //category
        public int Data { get; set; } //count
        public string Color { get; set; } = string.Empty;//colour
    }


    public class BookingSourceCount
    {
        public string Name { get; set; } = string.Empty; //booking type
        public List<int> Data { get; set; } = new List<int>(); // 
        public string Color { get; set; } = string.Empty;//colour
        public List<string> Dates = new List<string>();
    }


    public class BookingAverageCount
    {
        public string Label { get; set; } = string.Empty; //booking type
        public int Value { get; set; } 
        public string Color { get; set; } = string.Empty;//colour
        public string Width = string.Empty;
    }

    public class ReservationsCount
    {
        public string Name { get; set; } = string.Empty; //booking type
        public List<int> Data { get; set; } = new List<int>(); // 
        public string Color { get; set; } = string.Empty;//colour
        
    }

    public class RservationCount
    {
        public string Name { get; set; } = string.Empty; //booking type
        public List<int> Data { get; set; } = new List<int>(); // 
        public string Color { get; set; } = string.Empty;//colour
        public List<string> Dates = new List<string>();
    }


    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController: ControllerBase
    {
        private readonly DbContextSql _context;
        private readonly IMapper _mapper;
        private int companyId;
        private string financialYear = string.Empty;
        private int userId;

        private List<string> ColourCodes = new List<string> { "#FF5733", "#33FF57", "#3357FF", "#FF631A" , "#007360" };

        public DashboardController(DbContextSql contextSql, IMapper map, IHttpContextAccessor httpContextAccessor)
        {
            _context = contextSql;
            _mapper = map;
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


        [HttpGet("GetDashboardData")]
        public async Task<IActionResult> GetDashboardData(DateOnly startDate, DateOnly endDate)
        {

            DataSet? dataSet = new DataSet();
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    using (var command = new SqlCommand("check_room_availaibility_new", connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@userCheckInDate", startDate);
                        command.Parameters.AddWithValue("@userCheckOutDate", endDate);
                        command.Parameters.AddWithValue("@userCheckInTime", "00:00");
                        command.Parameters.AddWithValue("@userCheckOutTime", "23:59");
                        command.Parameters.AddWithValue("@companyId", companyId);
                        command.Parameters.AddWithValue("@pageName", "dashboarddata");
                        command.Parameters.AddWithValue("@roomTypeId", 0);
                        command.Parameters.AddWithValue("@roomId", 0);
                        command.Parameters.AddWithValue("@roomStatus", "");
                        //command.Parameters.AddWithValue("@companyId", companyId);
                        //command.Parameters.AddWithValue("@startDate", endDate);
                        //command.Parameters.AddWithValue("@endDate", startDate);

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataSet);
                        }

                        int count = 0;

                        // 1. Available Rooms (Clean) - Category wise
                        var availableRoomsData = dataSet.Tables[0].AsEnumerable().Select((row, index) => new CategoryWiseCount
                        {
                            Name = row.Field<string>(0),
                            Data = row.Field<int>(1),
                            
                            Color = row.Field<string>(2)  
                        }).ToList();

                        var availableRooms = new
                        {
                            Total = availableRoomsData.Sum(x => x.Data),
                            List = availableRoomsData
                        };

                        // 2. Check-in Rooms
                        var checkInRooms = dataSet.Tables[1].AsEnumerable().Select((row, index) => new CategoryWiseCount
                        {
                            Name = row.Field<string>(0),
                            Data = row.Field<int>(1),
                            Color = row.Field<string>(2)
                        }).ToList();

                        var checkInTotal = new
                        {
                            Total = checkInRooms.Sum(x => x.Data),
                            List = checkInRooms
                        };

                        // 3. Checkout Rooms
                        var checkOutRooms = dataSet.Tables[2].AsEnumerable().Select((row, index) => new CategoryWiseCount
                        {
                            Name = row.Field<string>(0),
                            Data = row.Field<int>(1),
                            Color = row.Field<string>(2)
                        }).ToList();

                        var checkOutTotal = new
                        {
                            Total = checkOutRooms.Sum(x => x.Data),
                            List = checkOutRooms
                        };

                        // 4. Pending + Confirmed Bookings
                        var pendingConfirmedRooms = dataSet.Tables[3].AsEnumerable().Select((row, index) => new CategoryWiseCount
                        {
                            Name = row.Field<string>(0),
                            Data = row.Field<int>(1),
                            Color = row.Field<string>(2)
                        }).ToList();

                        var bookings = new
                        {
                            Total = pendingConfirmedRooms.Sum(x => x.Data),
                            List = pendingConfirmedRooms
                        };

                        // 5. All Room Status
                        var roomStatus = dataSet.Tables[4].AsEnumerable().Select(row => new CategoryWiseCount
                        {
                            Name = row.Field<string>(0),
                            Color = row.Field<string>(1),
                            Data = row.Field<int>(2)
                        }).ToList();

                        // 6. Booking Source (OTA & Other)
                        var bookingSources = new List<BookingSourceCount>();
                        var bookingSourceOTA = new BookingSourceCount { Name = "OTA", Color = "#FF5733" };
                        var bookingSourceOther = new BookingSourceCount { Name = "Other", Color = "#3357FF" };

                        foreach (DataRow row in dataSet.Tables[5].Rows)
                        {
                            bookingSourceOTA.Dates.Add(row.Field<string>(0));
                            bookingSourceOther.Dates.Add(row.Field<string>(0));
                            bookingSourceOTA.Data.Add(row.Field<int>(1));
                            bookingSourceOther.Data.Add(row.Field<int>(2));
                        }

                        bookingSources.Add(bookingSourceOTA);
                        bookingSources.Add(bookingSourceOther);

                        // 7. Booking Averages
                        var bookingAverages = dataSet.Tables[6].AsEnumerable().Select(row => new BookingAverageCount
                        {
                            Label = row.Field<string>(0),
                            Value = row.Field<int>(1),
                            Color = row.Field<string>(2)
                        }).ToList();

                        var totalRooms = bookingAverages.Sum(x => x.Value);
                        foreach (var item in bookingAverages)
                        {
                            item.Width = totalRooms != 0
                                ? (Constants.Calculation.RoundOffDecimal((decimal)item.Value / totalRooms) * 100).ToString() + "%"
                                : "0%";
                        }

                        // 8. Reservations
                        var reservationsCounts = new List<RservationCount>();
                        var bookedReservation = new RservationCount { Name = "Booked", Color = "#f6a400" };
                        var cancelReservation = new RservationCount { Name = "Cancelled", Color = "#38d9a9" };

                        foreach (DataRow row in dataSet.Tables[7].Rows)
                        {
                            bookedReservation.Dates.Add(row.Field<string>(0));
                            cancelReservation.Dates.Add(row.Field<string>(0));
                            bookedReservation.Data.Add(row.Field<int>(2));
                            cancelReservation.Data.Add(row.Field<int>(1));
                        }

                        reservationsCounts.Add(bookedReservation);
                        reservationsCounts.Add(cancelReservation);

                        return Ok(new
                        {
                            Code = 200,
                            Message = "Data Fetched Successfully",
                            availableRooms,
                            checkInRooms = checkInTotal,
                            checkOutRooms = checkOutTotal,
                            roomStatuses = roomStatus,
                            bookingSourceStats = bookingSources,
                            bookingAverages,
                            reservationsCounts,
                            pendingConfirmedRooms = bookings
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = "Internal Server Error", Details = ex.Message });
            }


            //DataSet? dataSet = null;
            //try
            //{
            //    using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            //    {
            //        using (var command = new SqlCommand("DashboardData", connection))
            //        {
            //            command.CommandTimeout = 120;
            //            command.CommandType = CommandType.StoredProcedure;
            //            command.Parameters.AddWithValue("@companyId", companyId);
            //            command.Parameters.AddWithValue("@startDate", startDate);
            //            command.Parameters.AddWithValue("@endDate", startDate);
            //            await connection.OpenAsync();


            //            using (SqlDataReader reader = command.ExecuteReader())
            //            {
            //                // 1. Available Rooms (Clean) - Category wise

            //                int count = 0;
            //                List<CategoryWiseCount> availableRoomsData = new List<CategoryWiseCount>();
            //                while (reader.Read())
            //                {
            //                    CategoryWiseCount room = new CategoryWiseCount();
            //                    room.Name = reader.GetString(0);
            //                    room.Data = reader.GetInt32(1);
            //                    room.Color = count < ColourCodes.Count ? ColourCodes[count] : ColourCodes[0];
            //                    availableRoomsData.Add(room);
            //                    count++; 
            //                }

            //                var availableRooms = new
            //                {
            //                    Total = availableRoomsData.Sum(x => x.Data),
            //                    List = availableRoomsData
            //                };

            //                // 2. Check-in Rooms - Category wise
            //                reader.NextResult();
            //                List<CategoryWiseCount> checkInRooms = new List<CategoryWiseCount>();
            //                count = 0;
            //                while (reader.Read())
            //                {
            //                    CategoryWiseCount category = new CategoryWiseCount();
            //                    category.Name = reader.GetString(0);
            //                    category.Data = reader.GetInt32(1);
            //                    category.Color = count < ColourCodes.Count ? ColourCodes[count] : ColourCodes[0];
            //                    checkInRooms.Add(category);
            //                    count++;
            //                }

            //                var checkInTotal = new
            //                {
            //                    Total = checkInRooms.Sum(x => x.Data),
            //                    List = checkInRooms
            //                };

            //                // 3. Checkout Rooms - Category wise
            //                reader.NextResult();
            //                List<CategoryWiseCount> checkOutRooms = new List<CategoryWiseCount>();
            //                count = 0;
            //                while (reader.Read())
            //                {
            //                    CategoryWiseCount category = new CategoryWiseCount();
            //                    category.Name = reader.GetString(0);
            //                    category.Data = reader.GetInt32(1);
            //                    category.Color = count < ColourCodes.Count ? ColourCodes[count] : ColourCodes[0];
            //                    checkOutRooms.Add(category);
            //                    count++;
            //                }

            //                var checkOutTotal = new
            //                {
            //                    Total = checkOutRooms.Sum(x => x.Data),
            //                    List = checkOutRooms
            //                };

            //                // 4. Pending + Confirmed Bookings - Category wise
            //                reader.NextResult();
            //                List<CategoryWiseCount> pendingConfirmedRooms = new List<CategoryWiseCount>();
            //                count = 0;
            //                while (reader.Read())
            //                {
            //                    CategoryWiseCount category = new CategoryWiseCount();
            //                    category.Name = reader.GetString(0);
            //                    category.Data = reader.GetInt32(1);
            //                    category.Color = count < ColourCodes.Count ? ColourCodes[count] : ColourCodes[0];
            //                    pendingConfirmedRooms.Add(category);
            //                    count++;
            //                }

            //                var bookings = new
            //                {
            //                    Total = pendingConfirmedRooms.Sum(x => x.Data),
            //                    List = pendingConfirmedRooms
            //                };

            //                // 5. All Room Status - Status wise
            //                reader.NextResult();
            //                List<CategoryWiseCount> roomStatus = new List<CategoryWiseCount>();
            //                count = 0;
            //                while (reader.Read())
            //                {
            //                    CategoryWiseCount category = new CategoryWiseCount();
            //                    category.Name = reader.GetString(0);

            //                    category.Color = reader.GetString(1);
            //                    category.Data = reader.GetInt32(2);
            //                    roomStatus.Add(category);
            //                    count++;
            //                }

            //                // 6. Booking Source - Last 7 Days
            //                reader.NextResult();
            //                List<BookingSourceCount> bookingSources = new List<BookingSourceCount>();

            //                BookingSourceCount bookingSourceOTA = new BookingSourceCount();
            //                BookingSourceCount bookingSourceOther = new BookingSourceCount();
            //                bookingSourceOTA.Name = "OTA";
            //                bookingSourceOTA.Color = "#FF5733";

            //                bookingSourceOther.Name = "Other";
            //                bookingSourceOther.Color = "#3357FF";

            //                while (reader.Read())
            //                {
            //                    bookingSourceOTA.Dates.Add(reader.GetDateTime(0));
            //                    bookingSourceOther.Dates.Add(reader.GetDateTime(0));

            //                    bookingSourceOTA.Data.Add(reader.GetInt32(1));
            //                    bookingSourceOther.Data.Add(reader.GetInt32(2));
            //                }

            //                bookingSources.Add(bookingSourceOTA);
            //                bookingSources.Add(bookingSourceOther);

            //                //Booking average
            //                reader.NextResult();
            //                List<BookingAverageCount> bookingAverages = new List<BookingAverageCount>();
            //                while (reader.Read())
            //                {
            //                    BookingAverageCount average = new BookingAverageCount();
            //                    average.Label = reader.GetString(0);
            //                    average.Value = reader.GetInt32(1);
            //                    average.Color = reader.GetString(2);
            //                    bookingAverages.Add(average);
            //                }

            //                var totalRooms = bookingAverages.Sum(x => x.Value);
            //                foreach(var item in bookingAverages)
            //                {
            //                    if (totalRooms != 0)
            //                    {
            //                        item.Width = (Constants.Calculation.RoundOffDecimal((decimal)(item.Value / totalRooms)) * 100).ToString() + "%";
            //                    }
            //                    else
            //                    {
            //                        item.Width = "0%";
            //                    }

            //                }



            //                //reservations
            //                List<RservationCount> reservationsCounts = new List<RservationCount>();
            //                RservationCount bookedReservation = new RservationCount();
            //                RservationCount cancelReservation = new RservationCount();

            //                bookedReservation.Name = "Booked";
            //                bookedReservation.Color = "#f6a400";

            //                cancelReservation.Name = "Cancelled";
            //                cancelReservation.Color = "#38d9a9";

            //                reader.NextResult();
            //                while (reader.Read())
            //                {
            //                    bookedReservation.Dates.Add(reader.GetString(0));
            //                    cancelReservation.Dates.Add(reader.GetString(0));
            //                    bookedReservation.Data.Add(reader.GetInt32(2));
            //                    cancelReservation.Data.Add(reader.GetInt32(1));
            //                }

            //                reservationsCounts.Add(bookedReservation);
            //                reservationsCounts.Add(cancelReservation);

            //                await connection.CloseAsync();
            //                return Ok(new { Code = 200, Message = "Data Fetched Successfully", availableRooms = availableRooms, checkInRooms = checkInTotal, checkOutRooms = checkOutTotal, roomStatuses = roomStatus, bookingSourceStats = bookingSources, bookingAverages = bookingAverages , reservationsCounts = reservationsCounts, pendingConfirmedRooms= bookings });


            //            }

            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    return Ok(new { Code = 500, Message = Constants.Constants.ErrorMessage });
            //}
        }



        [HttpGet("GetAllStatus")]
        public async Task<IActionResult> GetAllStatus()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.Headers["CompanyId"]);
            int userId = Convert.ToInt32(HttpContext.Request.Headers["UserId"]);
            try
            {
                var data = await _context.ServicesStatus.ToListAsync();

                if (data.Count == 0)
                {
                    return Ok(new { Code = 404, Message = "Status not found", Data = Array.Empty<object>() });
                }

                return Ok(new { Code = 200, Message = "Status fetched successfully", Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = 500, Message = Constants.Constants.ErrorMessage });
            }
        }

    }
}
