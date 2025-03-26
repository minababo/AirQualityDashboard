using System.Diagnostics;
using AirQualityDashboard.Models;
using Microsoft.AspNetCore.Mvc;
using AirQualityDashboard.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            var sensors = _context.Sensors
                .Where(s => s.IsActive)
                .ToList();
            return View(sensors);
        }

        public IActionResult Privacy()
        {
            return View();
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
