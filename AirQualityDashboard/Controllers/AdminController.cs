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

        return View("Dashboard");
    }

    public IActionResult Sensors()
    {
        var sensors = _context.Sensors.ToList();
        return View("Sensors/Index", sensors);
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
    public async Task<IActionResult> Details(int? id, string filter = "24h", int page = 1)
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
            .OrderByDescending(r => r.Timestamp)
            .ToList();

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
                CO2 = g.Average(x => x.CO2)
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
}
