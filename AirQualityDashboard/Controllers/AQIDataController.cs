using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AirQualityDashboard.Controllers
{
    public class AQIDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AQIDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AQIData.Include(a => a.Sensor);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aQIData = await _context.AQIData
                .Include(a => a.Sensor)
                .FirstOrDefaultAsync(m => m.AQIDataId == id);
            if (aQIData == null)
            {
                return NotFound();
            }

            return View(aQIData);
        }

        public IActionResult Create()
        {
            ViewData["SensorId"] = new SelectList(_context.Sensors, "SensorId", "LocationName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AQIDataId,SensorId,PM25,PM10,Timestamp")] AQIData aQIData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aQIData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SensorId"] = new SelectList(_context.Sensors, "SensorId", "LocationName", aQIData.SensorId);
            return View(aQIData);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aQIData = await _context.AQIData.FindAsync(id);
            if (aQIData == null)
            {
                return NotFound();
            }
            ViewData["SensorId"] = new SelectList(_context.Sensors, "SensorId", "LocationName", aQIData.SensorId);
            return View(aQIData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AQIDataId,SensorId,PM25,PM10,Timestamp")] AQIData aQIData)
        {
            if (id != aQIData.AQIDataId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aQIData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AQIDataExists(aQIData.AQIDataId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SensorId"] = new SelectList(_context.Sensors, "SensorId", "LocationName", aQIData.SensorId);
            return View(aQIData);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aQIData = await _context.AQIData
                .Include(a => a.Sensor)
                .FirstOrDefaultAsync(m => m.AQIDataId == id);
            if (aQIData == null)
            {
                return NotFound();
            }

            return View(aQIData);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aQIData = await _context.AQIData.FindAsync(id);
            if (aQIData != null)
            {
                _context.AQIData.Remove(aQIData);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AQIDataExists(int id)
        {
            return _context.AQIData.Any(e => e.AQIDataId == id);
        }
    }
}
