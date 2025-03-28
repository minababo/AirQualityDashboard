using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AirQualityDashboard.Data;
using AirQualityDashboard.Models;

namespace AirQualityDashboard.Controllers
{
    [Authorize]
    public class SensorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SensorController(ApplicationDbContext context)
        {
            _context = context;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sensors.ToListAsync());
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

            return View(viewModel);
        }
    }
}