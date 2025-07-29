using hotel_api.Configurations;
using hotel_api.Notifications.ReminderEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Quartz;
using RepositoryModels.Repositories;
using RepositoryModels.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddSwaggerGen(options =>

{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Your API",
        Version = "v1"
    });

    // Register the custom operation filter
    options.OperationFilter<AddRequiredHeaderParameter>();
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

//builder.Services.AddQuartz(q =>
//{
//    var jobKey = new JobKey("ReminderEmailJob");

//    q.AddJob<ReminderEmailJob>(opts => opts.WithIdentity(jobKey));

//    q.AddTrigger(opts => opts
//        .ForJob(jobKey)
//        .WithIdentity("ReminderEmailJob-trigger")
//        .WithCronSchedule("0 * * * * ?")); // every minute
//});



var configuration = builder.Configuration;
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDbContext<DbContextSql>((serviceProvider, dbContextBuilder) =>
{
    var connectionStringPlaceHolder = configuration.GetConnectionString("SqlConnection");
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var dbName = httpContextAccessor.HttpContext?.Request.Headers["Database"].FirstOrDefault();
    var connectionString = connectionStringPlaceHolder?.Replace("{dbName}", dbName);
    dbContextBuilder.UseSqlServer(connectionString);
});

builder.Services.AddDirectoryBrowser();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddAutoMapper(typeof(AutomapperConfig));

//builder.Services.AddQuartzHostedService();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = new PathString("/uploads")
});
app.UseDirectoryBrowser(new DirectoryBrowserOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = new PathString("/uploads")
});
app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();

//app.UseSwagger();
//app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "hotel api"));
app.UseAuthorization();

app.MapControllers();

app.Run();
