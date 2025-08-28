using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RepositoryModels.Repository;

namespace hotel_api.Middleware
{
    public class UserExistenceMiddleware
    {
        private readonly RequestDelegate _next;
       

        public UserExistenceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var _dbContext = context.RequestServices.GetRequiredService<DbContextSql>();
            var path = context.Request.Path.Value?.ToLower();
            if (path.StartsWith("/swagger") || path.StartsWith("/api/auth/login") || path.StartsWith("/api/auth/getpagesforbinding"))
            {
                await _next(context);
                return;
            }

            // Example: Assume userId comes from claims or headers
            if (!context.Request.Headers.TryGetValue("UserId", out var userIdHeader)
    || !int.TryParse(userIdHeader, out var userId))
            {
                var response = new { Code = 500, message = "User Id missing." };
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
                return;
            }

           

            bool exists = await _dbContext.UserDetails.AnyAsync(u => u.UserId == userId && u.IsActive == true);

            if (!exists)
            {
                var response = new { Code = 500, message = "User does not exists." };
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);

                return;
            }

            // ✅ User exists → continue to next middleware
            await _next(context);
        }
    }
}
