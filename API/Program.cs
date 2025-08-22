using Domain.Dto;
using Domain.Services;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.AppDbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();  // Needed for Swagger
builder.Services.AddSwaggerGen();            // Swagger generator

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Credit Phone Lock System API v1");
    c.RoutePrefix = string.Empty; // 👈 makes Swagger UI the landing page
});

//    app.UseHttpsRedirection();
//}



app.MapPost("/AddCustomer", async (CreateCustomerDto dto, ICustomerService customerService) =>
{
    var result = await customerService.CreateCustomerAsync(dto);
    if(result.Status==false)
        return Results.InternalServerError(result.Message);
    return Results.Ok(result);
})
.WithName("AddCustomer")
.WithOpenApi();

app.Run();