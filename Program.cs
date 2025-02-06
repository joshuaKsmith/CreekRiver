using CreekRiver.Models;
using CreekRiver.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;


////////////////////////////////////////////////////////////////////
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<CreekRiverDbContext>(builder.Configuration["CreekRiverDbConnectionString"]);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//////////////////////////////////////////////////////////////////////

app.MapGet("/api/campsites", (CreekRiverDbContext db) =>
{
    return db.Campsites
    .Select(c => new CampsiteDTO
    {
        Id = c.Id,
        Nickname = c.Nickname,
        ImageUrl = c.ImageUrl,
        CampsiteTypeId = c.CampsiteTypeId
    }).ToList();
});

app.MapGet("/api/campsites/{id}", (CreekRiverDbContext db, int id) =>
{
    var campsite = db.Campsites
        .Include(c => c.CampsiteType)
        .Where(c => c.Id == id)
        .Select(c => new CampsiteDTO
        {
            Id = c.Id,
            Nickname = c.Nickname,
            CampsiteTypeId = c.CampsiteTypeId,
            CampsiteType = new CampsiteTypeDTO
            {
                Id = c.CampsiteType.Id,
                CampsiteTypeName = c.CampsiteType.CampsiteTypeName,
                FeePerNight = c.CampsiteType.FeePerNight,
                MaxReservationDays = c.CampsiteType.MaxReservationDays
            }
        })
        .FirstOrDefault();
    if (campsite == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(campsite);
});

app.MapPost("/api/campsites", (CreekRiverDbContext db, Campsite campsite) =>
{
    db.Campsites.Add(campsite);
    db.SaveChanges();
    return Results.Created($"/api/campsites/{campsite.Id}", campsite);
});


//////////////////////////////////////////////////////////////////////
app.Run();
