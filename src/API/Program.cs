using Booking.API.Hub;
using Booking.API.Services;
using Booking.API.Worker;
using Booking.Application.Abstractions;
using Booking.Application.Commands;
using Booking.Application.Handlers;
using Booking.Infrastructure.Extensions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

using Scalar.AspNetCore;

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICommandHandler<ReserveSeatCommand, bool>, ReserveSeatHandler>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // React URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR
    });
});

builder.Services.AddScoped<IRealTimeNotifier, SignalRNotifier>();

var app = builder.Build();

// Seed data
await DbSeeder.SeedAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowReactApp");

app.MapControllers();
app.MapHub<BookingHub>("/hub/bookings"); // SignalR Endpoint

app.Run();