using Domain.Dto;
using Domain.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.AppDbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<ILoanJobService, LoanJobService>();

var app = builder.Build();
// Hangfire Dashboard (for monitoring jobs)
app.UseHangfireDashboard("/hangfire");
//Add swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Credit Phone Lock System API v1");
    c.RoutePrefix = string.Empty; // 👈 makes Swagger UI the landing page
});

#region EndPoints

app.MapPost("/AddCustomer", async (CreateCustomerDto dto, ICustomerService customerService) =>
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

app.MapGet("/DeviceStatus/{imei}", async (string imei, ICustomerService customerService) =>
{
    var result = await customerService.GetDeviceStatusByIMEIAsync(imei);
    if (!result.Status || result.Data == null)
        return Results.NotFound(result.Message); // 404 if not found
    return Results.Ok(result);
})
.WithName("DeviceStatus")
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