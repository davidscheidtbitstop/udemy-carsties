using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Data;
using SearchService.Endpoints;
using SearchService.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var app = builder.Build();

// Configure the HTTP request pipeline.

try
{
    await app.InitDb();
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

app.MapSearchEndpoints();

app.Run();
