using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Endpoints;
using AuctionService.RequestHelpers;
using AuctionService.Validators;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterMapsterConfigurations();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add services to the container.
builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

try {
    app.InitDb();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
app.MapAuctionEndpoints();

app.Run();
