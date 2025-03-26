﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public class AQIDataGeneratorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly Random _random = new();

    public AQIDataGeneratorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var sensors = await dbContext.Sensors.Where(s => s.IsActive).ToListAsync();

            foreach (var sensor in sensors)
            {
                var aqiData = new AQIData
                {
                    SensorId = sensor.SensorId,
                    Timestamp = DateTime.Now,
                    PM25 = RandomFloat(10, 100),
                    PM10 = RandomFloat(20, 150),
                    CO2 = RandomFloat(400, 1000)
                };

                dbContext.AQIData.Add(aqiData);
            }

            await dbContext.SaveChangesAsync();

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private float RandomFloat(float min, float max)
    {
        return (float)(_random.NextDouble() * (max - min) + min);
    }
}
