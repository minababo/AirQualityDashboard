using System.Diagnostics;
using AirQualityDashboard.Models;
using Microsoft.AspNetCore.Mvc;
using AirQualityDashboard.Data;
using Microsoft.EntityFrameworkCore;
using AirQualityDashboard.Models.ViewModels;

namespace AirQualityDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var now = DateTime.UtcNow;

            var sensors = _context.Sensors
                .Where(s => s.IsActive)
                .Include(s => s.AQIDataRecords)
                .ToList()
                .Select(s => new SensorMapViewModel
                {
                    SensorId = s.SensorId,
                    SensorName = s.SensorName,
                    LocationName = s.LocationName,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    LatestPM25 = s.AQIDataRecords
                        .OrderByDescending(a => a.Timestamp)
                        .Select(a => (double?)a.PM25)
                        .FirstOrDefault(),
                    LatestTimestamp = s.AQIDataRecords
                        .OrderByDescending(a => a.Timestamp)
                        .Select(a => (DateTime?)a.Timestamp)
                        .FirstOrDefault(),
                    TrendTimestamps = s.AQIDataRecords
                        .Where(a => a.Timestamp >= now.AddHours(-24))
                        .AsEnumerable()
                        .GroupBy(a => new DateTime(a.Timestamp.Year, a.Timestamp.Month, a.Timestamp.Day, a.Timestamp.Hour, 0, 0))
                        .OrderBy(g => g.Key)
                        .Select(g => g.Key)
                        .ToList(),
                    TrendPM25 = s.AQIDataRecords
                        .Where(a => a.Timestamp >= now.AddHours(-24))
                        .AsEnumerable()
                        .GroupBy(a => new DateTime(a.Timestamp.Year, a.Timestamp.Month, a.Timestamp.Day, a.Timestamp.Hour, 0, 0))
                        .OrderBy(g => g.Key)
                        .Select(g => g.Average(x => x.PM25))
                        .ToList(),
                    TrendPM10 = s.AQIDataRecords
                        .Where(a => a.Timestamp >= now.AddHours(-24))
                        .AsEnumerable()
                        .GroupBy(a => new DateTime(a.Timestamp.Year, a.Timestamp.Month, a.Timestamp.Day, a.Timestamp.Hour, 0, 0))
                        .OrderBy(g => g.Key)
                        .Select(g => g.Average(x => x.PM10))
                        .ToList()
                })
                .ToList();

            return View(sensors);
        }


        public IActionResult Sensors()
        {
            return RedirectToAction("Index", "Sensor");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult TestMap()
        {
            return View();
        }
    }
}