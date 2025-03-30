using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public class AQIDataGeneratorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly Random _random = new();
    private bool _isRunning = true;

    public AQIDataGeneratorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool IsRunning => _isRunning;

    public void Pause()
    {
        _isRunning = false;
    }

    public void Resume()
    {
        _isRunning = true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_isRunning)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var sensors = await dbContext.Sensors.Where(s => s.IsActive).ToListAsync();

                foreach (var sensor in sensors)
                {
                    var settings = await dbContext.SimulationSettings.FirstOrDefaultAsync();

                    float RandomOrDefault(float min, float max, float defaultMin, float defaultMax)
                    {
                        return (settings != null) ? RandomFloat(min, max) : RandomFloat(defaultMin, defaultMax);
                    }

                    var aqiData = new AQIData
                    {
                        SensorId = sensor.SensorId,
                        Timestamp = DateTime.Now,
                        PM25 = RandomOrDefault(settings?.PM25Min ?? 10, settings?.PM25Max ?? 110, 10, 110),
                        PM10 = RandomOrDefault(settings?.PM10Min ?? 10, settings?.PM10Max ?? 260, 10, 260),
                        RH = RandomOrDefault(settings?.RHMin ?? 50, settings?.RHMax ?? 100, 50, 100),
                        Temp = RandomOrDefault(settings?.TempMin ?? 25, settings?.TempMax ?? 35, 25, 35),
                        Wind = RandomOrDefault(settings?.WindMin ?? 0, settings?.WindMax ?? 22, 0, 22)
                    };


                    dbContext.AQIData.Add(aqiData);
                }

                await dbContext.SaveChangesAsync();
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private float RandomFloat(float min, float max)
    {
        return (float)(_random.NextDouble() * (max - min) + min);
    }
}