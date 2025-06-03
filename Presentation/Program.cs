using Business.Interfaces;
using Business.Models;
using Business.Services;
using Data.Context;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(x =>
    x.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlConnection"),
        options => options.MigrationsAssembly("Data")
    )
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(x =>
{
    x.User.RequireUniqueEmail = true;
    x.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddHttpClient<IVerificationClient, VerificationClient>();

var app = builder.Build();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Service API V1");
    c.RoutePrefix = string.Empty;
});

app.MapOpenApi();

app.UseSwagger();

app.UseHttpsRedirection();
app.UseCors(x =>
{
    x.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
