using Booking.Application.Abstractions;
using Booking.Domain.Repositories;
using Booking.Infrastructure.Persistence;
using Booking.Infrastructure.Repositories;
using Booking.Infrastructure.Services;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

using StackExchange.Redis;

namespace Booking.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. PostgreSQL configuration
        var postgresConnectionString = configuration.GetConnectionString("Postgres");
        services.AddDbContext<BookingDbContext>(options => options.UseNpgsql(postgresConnectionString));

        // 2. ValKey configuration
        var valKeyConnectionString = configuration.GetConnectionString("ValKey");
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(valKeyConnectionString!));

        // 3. Repositories & Services
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISeatLockService, SeatLockService>();

        // Mongo Configuration
        var mongoConn = configuration.GetConnectionString("Mongo");
        var mongoClient = new MongoClient(mongoConn);
        var mongoDb = mongoClient.GetDatabase("booking_read_db");
        services.AddSingleton(mongoDb);

        // 4. MassTransit (RabbitMQ + Outbox)
        services.AddMassTransit(o =>
        {
            // Register the consumer
            o.AddConsumer<Consumers.SeatsReservedConsumer>();

            o.AddEntityFrameworkOutbox<BookingDbContext>(cfg =>
            {
                cfg.UsePostgres();
                cfg.UseBusOutbox();
            });

            o.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}