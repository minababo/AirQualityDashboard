using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using AirQualityDashboard.Data;
using AirQualityDashboard.Models;
using AirQualityDashboard.Models.ViewModels;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AQIDataGeneratorService _generatorService;

    public AdminController(ApplicationDbContext context, AQIDataGeneratorService generatorService)
    {
        _context = context;
        _generatorService = generatorService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var totalSensors = await _context.Sensors.CountAsync();
        var activeSensors = await _context.Sensors.CountAsync(s => s.IsActive);
        ViewBag.TotalSensors = totalSensors;
        ViewBag.ActiveSensors = activeSensors;
        ViewBag.SimulationStatus = _generatorService.IsRunning ? "Running" : "Paused";

        var threshold = await _context.AlertThresholdSettings.FirstOrDefaultAsync();
        var alerts = new List<string>();

        if (threshold != null)
        {
            var latest = _context.AQIData
                .Include(a => a.Sensor)
                .GroupBy(a => a.SensorId)
                .Select(g => g.OrderByDescending(r => r.Timestamp).FirstOrDefault())
                .ToList();

            foreach (var r in latest)
            {
                if (r.PM25 > threshold.PM25Threshold) alerts.Add($"{r.Sensor.SensorName} - PM2.5 exceeded threshold");
                if (r.PM10 > threshold.PM10Threshold) alerts.Add($"{r.Sensor.SensorName} - PM10 exceeded threshold");
                if (r.RH > threshold.RHThreshold) alerts.Add($"{r.Sensor.SensorName} - Humidity exceeded threshold");
                if (r.Temp > threshold.TempThreshold) alerts.Add($"{r.Sensor.SensorName} - Temperature exceeded threshold");
                if (r.Wind > threshold.WindThreshold) alerts.Add($"{r.Sensor.SensorName} - Wind speed exceeded threshold");
            }
        }

        ViewBag.Alerts = alerts;

        return View("Dashboard");
    }

    public IActionResult Sensors(string sortOrder)
    {
        ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewBag.LocationSortParm = sortOrder == "location" ? "location_desc" : "location";

        var sensors = from s in _context.Sensors
                      select s;

        switch (sortOrder)
        {
            case "name_desc":
                sensors = sensors.OrderByDescending(s => s.SensorName);
                break;
            case "location":
                sensors = sensors.OrderBy(s => s.LocationName);
                break;
            case "location_desc":
                sensors = sensors.OrderByDescending(s => s.LocationName);
                break;
            default:
                sensors = sensors.OrderBy(s => s.SensorName);
                break;
        }

        return View("Sensors/Index", sensors.ToList());
    }


    public IActionResult Create()
    {
        return View("Sensors/Create");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SensorId,SensorName,LocationName,Latitude,Longitude,IsActive")] Sensor sensor)
    {
        if (ModelState.IsValid)
        {
            _context.Add(sensor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Sensors));
        }
        return View("Sensors/Create", sensor);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var sensor = await _context.Sensors.FindAsync(id);
        if (sensor == null) return NotFound();

        return View("Sensors/Edit", sensor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SensorId,SensorName,LocationName,Latitude,Longitude,IsActive")] Sensor sensor)
    {
        if (id != sensor.SensorId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(sensor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SensorExists(sensor.SensorId))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Sensors));
        }
        return View("Sensors/Edit", sensor);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var sensor = await _context.Sensors.FirstOrDefaultAsync(m => m.SensorId == id);
        if (sensor == null) return NotFound();

        return View("Sensors/Delete", sensor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        if (sensor != null)
            _context.Sensors.Remove(sensor);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Sensors));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id, string filter = "24h", int page = 1, string sortOrder = "timestamp_desc")
    {
        if (id == null) return NotFound();

        var sensor = await _context.Sensors
            .Include(s => s.AQIDataRecords)
            .FirstOrDefaultAsync(m => m.SensorId == id);

        if (sensor == null) return NotFound();

        DateTime fromDate = filter switch
        {
            "7d" => DateTime.Now.AddDays(-7),
            "30d" => DateTime.Now.AddDays(-30),
            _ => DateTime.Now.AddHours(-24)
        };

        var filteredRecords = sensor.AQIDataRecords
        .Where(r => r.Timestamp >= fromDate)
        .ToList();

            filteredRecords = sortOrder switch
            {
                "timestamp" => filteredRecords.OrderBy(r => r.Timestamp).ToList(),
                "pm25" => filteredRecords.OrderBy(r => r.PM25).ToList(),
                "pm10" => filteredRecords.OrderBy(r => r.PM10).ToList(),
                "rh" => filteredRecords.OrderBy(r => r.RH).ToList(),
                "temp" => filteredRecords.OrderBy(r => r.Temp).ToList(),
                "wind" => filteredRecords.OrderBy(r => r.Wind).ToList(),
                "timestamp_desc" => filteredRecords.OrderByDescending(r => r.Timestamp).ToList(),
                "pm25_desc" => filteredRecords.OrderByDescending(r => r.PM25).ToList(),
                "pm10_desc" => filteredRecords.OrderByDescending(r => r.PM10).ToList(),
                "rh_desc" => filteredRecords.OrderByDescending(r => r.RH).ToList(),
                "temp_desc" => filteredRecords.OrderByDescending(r => r.Temp).ToList(),
                "wind_desc" => filteredRecords.OrderByDescending(r => r.Wind).ToList(),
                _ => filteredRecords.OrderByDescending(r => r.Timestamp).ToList()
            };


        int pageSize = 10;
        int totalPages = (int)Math.Ceiling((double)filteredRecords.Count / pageSize);

        var pagedRecords = filteredRecords
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var now = DateTime.UtcNow;
        var hourlyAverages = _context.AQIData
            .Where(r => r.SensorId == id && r.Timestamp >= now.AddHours(-48))
            .AsEnumerable()
            .GroupBy(r => new DateTime(r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour, 0, 0))
            .Select(g => new HourlyAverage
            {
                Timestamp = g.Key,
                PM25 = g.Average(x => x.PM25),
                PM10 = g.Average(x => x.PM10),
                RH = g.Average(x => x.RH),
                Temp = g.Average(x => x.Temp),
                Wind = g.Average(x => x.Wind)
            })
            .OrderBy(x => x.Timestamp)
            .ToList();

        var latestReading = sensor.AQIDataRecords
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefault();

        var viewModel = new SensorDetailsViewModel
        {
            Sensor = sensor,
            PagedAQIData = pagedRecords,
            FullAQIData = filteredRecords,
            Filter = filter,
            CurrentPage = page,
            TotalPages = totalPages,
            HourlyAverages = hourlyAverages,
            LatestReading = latestReading
        };

        return View("Sensors/Details", viewModel);
    }


    private bool SensorExists(int id)
    {
        return _context.Sensors.Any(e => e.SensorId == id);
    }

    [HttpPost]
    public IActionResult StartSimulation()
    {
        _generatorService.Resume();
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    public IActionResult StopSimulation()
    {
        _generatorService.Pause();
        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> ConfigureSimulation()
    {
        var settings = await _context.SimulationSettings.FirstOrDefaultAsync() ?? new SimulationSettings();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigureSimulation(SimulationSettings model)
    {
        var existing = await _context.SimulationSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.SimulationSettings.Add(model);
        }
        else
        {
            existing.PM25Min = model.PM25Min;
            existing.PM25Max = model.PM25Max;
            existing.PM10Min = model.PM10Min;
            existing.PM10Max = model.PM10Max;
            existing.RHMin = model.RHMin;
            existing.RHMax = model.RHMax;
            existing.TempMin = model.TempMin;
            existing.TempMax = model.TempMax;
            existing.WindMin = model.WindMin;
            existing.WindMax = model.WindMax;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Dashboard");
    }

    public async Task<IActionResult> ConfigureAlerts()
    {
        var settings = await _context.AlertThresholdSettings.FirstOrDefaultAsync() ?? new AlertThresholdSettings();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigureAlerts(AlertThresholdSettings model)
    {
        var existing = await _context.AlertThresholdSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.AlertThresholdSettings.Add(model);
        }
        else
        {
            existing.PM25Threshold = model.PM25Threshold;
            existing.PM10Threshold = model.PM10Threshold;
            existing.RHThreshold = model.RHThreshold;
            existing.TempThreshold = model.TempThreshold;
            existing.WindThreshold = model.WindThreshold;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var threshold = await _context.AlertThresholdSettings.FirstOrDefaultAsync();
        var alerts = new List<object>();

        if (threshold != null)
        {
            var latest = _context.AQIData
                .Include(a => a.Sensor)
                .GroupBy(a => a.SensorId)
                .Select(g => g.OrderByDescending(r => r.Timestamp).FirstOrDefault())
                .ToList();

            foreach (var r in latest)
            {
                if (r == null || r.Sensor == null) continue;

                if (r.PM25 > threshold.PM25Threshold)
                    alerts.Add(new { sensorId = r.Sensor.SensorId, message = $"{r.Sensor.SensorName} - PM2.5 exceeded threshold" });
                if (r.PM10 > threshold.PM10Threshold)
                    alerts.Add(new { sensorId = r.Sensor.SensorId, message = $"{r.Sensor.SensorName} - PM10 exceeded threshold" });
                if (r.RH > threshold.RHThreshold)
                    alerts.Add(new { sensorId = r.Sensor.SensorId, message = $"{r.Sensor.SensorName} - Humidity exceeded threshold" });
                if (r.Temp > threshold.TempThreshold)
                    alerts.Add(new { sensorId = r.Sensor.SensorId, message = $"{r.Sensor.SensorName} - Temperature exceeded threshold" });
                if (r.Wind > threshold.WindThreshold)
                    alerts.Add(new { sensorId = r.Sensor.SensorId, message = $"{r.Sensor.SensorName} - Wind speed exceeded threshold" });
            }
        }

        return Json(alerts);
    }

}