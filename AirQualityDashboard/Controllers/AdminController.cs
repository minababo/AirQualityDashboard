using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AirQualityDashboard.Data;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var totalSensors = await _context.Sensors.CountAsync();
        var activeSensors = await _context.Sensors.CountAsync(s => s.IsActive);

        ViewBag.TotalSensors = totalSensors;
        ViewBag.ActiveSensors = activeSensors;

        return View();
    }
}