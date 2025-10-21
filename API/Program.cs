using Domain.Dto;
using Domain.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.AppDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Configuration
// Add services to the container.
builder.Services.AddEndpointsApiExplorer();  // Needed for Swagger
builder.Services.AddSwaggerGen();            // Swagger generator

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

//Add repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICustomerService, CustomerService>(); 
builder.Services.AddScoped<IAdminService, AdminService>(); 
builder.Services.AddScoped<ILoanJobService, LoanJobService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000", "http://localhost:5173",             // local dev
            "https://credit-frontend.onrender.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();
// Enable CORS
app.UseCors("AllowFrontend");

// Hangfire Dashboard (for monitoring jobs)
app.UseHangfireDashboard("/hangfire");
//Add swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Credit Phone Lock System API v1");
    c.RoutePrefix = string.Empty; // 👈 makes Swagger UI the landing page
});
#endregion
#region EndPoints

app.MapPost("/LoginUser", async (LoginDto dto, IAdminService adminService) =>
{
    var result = await adminService.LoginAsync( dto.Email, dto.Password);
    if (result.Status == false)
        return Results.InternalServerError(result.Message);
    return Results.Ok(result);
})
.WithName("LoginUser")
.WithOpenApi();
app.MapPost("/AddAdminUser", async (RegisterDto dto, IAdminService adminService) =>
{
    var result = await adminService.RegisterAsync(dto.FullName, dto.Email, dto.Password,false);
    if (result.Status == false)
        return Results.InternalServerError(result.Message);
    return Results.Ok(result);
})
.WithName("CreateAdminUser")
.WithOpenApi();
app.MapPost("/AddSuperAdminUser", async (RegisterDto dto, IAdminService adminService) =>
{
    var result = await adminService.RegisterAsync(dto.FullName, dto.Email, dto.Password,true);
    if (result.Status == false)
        return Results.InternalServerError(result.Message);
    return Results.Ok(result);
})
.WithName("CreateSuperAdminUser")
.WithOpenApi();
app.MapPost("/AddCustomer", async ([FromForm] CreateCustomerDto dto, ICustomerService customerService) =>
{
    var result = await customerService.CreateCustomerAsync(dto);
    if (result.Status == false)
        return Results.InternalServerError(result.Message);
    return Results.Ok(result);
})
.WithName("AddCustomer")
.WithOpenApi();
app.MapGet("/customers/{imei}", async (string imei, ICustomerService customerService) =>
{
    var result = await customerService.GetCustomerByIMEIAsync(imei);
    if (!result.Status || result.Data == null)
        return Results.NotFound(result.Message); // 404 if not found
    return Results.Ok(result);
})
.WithName("GetCustomerByIMEI")
.WithOpenApi();
app.MapGet("/defaulters", async (ICustomerService customerService) =>
{
    var result = await customerService.GetAllDefaultersAsync();
    if (!result.Status || result.Data == null)
        return Results.NotFound(result.Message); // 404 if not found
    return Results.Ok(result);
})
.WithName("GetAllDefaulters")
.WithOpenApi();

app.MapGet("/DeviceStatus/{imei}", async (string imei, ICustomerService customerService) =>
{
    var result = await customerService.GetDeviceStatusByIMEIAsync(imei);
    if (!result.Status || result.Data == null)
        return Results.NotFound(result.Message); // 404 if not found
    return Results.Ok(result);
})
.WithName("DeviceStatus")
.WithOpenApi();

app.MapPut("/LockDevice/{imei}", async (string imei, ICustomerService customerService) =>
{
    var result = await customerService.FlagStatusByIMEI(imei,1);
    if (!result.Status)
        return Results.NotFound(result.Message); // 404 if not found
    return Results.Ok(result);
})
.WithName("LockDevice")
.WithOpenApi();
app.MapPut("/UnlockDevice/{imei}", async (string imei, ICustomerService customerService) =>
{
    var result = await customerService.FlagStatusByIMEI(imei,2);
    if (!result.Status)
        return Results.NotFound(result.Message); // 404 if not found
    return Results.Ok(result);
})
.WithName("UnlockDevice")
.WithOpenApi();

#endregion

// Register recurring jobs
RecurringJob.AddOrUpdate<ILoanJobService>(
    "loan-overdue-check",
    service => service.CheckAndUpdateLoanStatuses(),
    Cron.Daily // runs every day at midnight
);

//RecurringJob.AddOrUpdate<ILoanJobService>(
//    "lock-phone",
//    service => service.CheckAndUpdateLoanStatuses(),
//    "0 */6 * * *"
//);

app.Run();