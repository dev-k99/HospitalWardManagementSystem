using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.Models;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Administrator")]// Restricts access to authenticated users
    public class BedManagementController : Controller
    {
        private readonly WardSystemDBContext _context;

        public BedManagementController(WardSystemDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var beds = await _context.Beds
                                     .Include(b => b.Room)
                                     .ThenInclude(r => r.Ward)
                                     .ToListAsync();
            return View(beds);
        }
        public async Task<IActionResult> Search(string query)
        {
            var filteredBeds = await _context.Beds
                .Include(r => r.Room)
                .Where(r => r.IsActive && r.BedNumber.Contains(query))
                .ToListAsync();

            return View("ManageBeds", filteredBeds);
        }
        // GET: Beds/CreateBed
        public IActionResult CreateBed()
        {
            ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "RoomNumber");
            return View();
        }

        // POST: Beds/CreateBed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBed(string bedNumber, int roomId)
        {
            try
            {
                var bed = new Bed
                {
                    BedNumber = bedNumber,
                    RoomId = roomId,
                    IsActive = true
                };

                _context.Add(bed);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Bed '{bed.BedNumber}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "RoomNumber", roomId);
                return View();
            }
        }

        // GET: Beds/Edit/5
        public async Task<IActionResult> EditBed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null) return NotFound();

            ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "RoomNumber", bed.RoomId);
            return View(bed);
        }

        // POST: Beds/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBed(int id, Bed bed)
        {
            if (id != bed.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bed);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Bed '{bed.BedNumber}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Beds.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "RoomNumber", bed.RoomId);
            return View(bed);
        }

        // GET: Beds/Details/5
        public async Task<IActionResult> DetailsBed(int id)
        {
            var bed = await _context.Beds
                                    .Include(b => b.Room)
                                    .ThenInclude(r => r.Ward)
                                    .Include(b => b.Patient)
                                    .FirstOrDefaultAsync(b => b.Id == id);

            if (bed == null) return NotFound();

            return View(bed);
        }

     
     
        public async Task<IActionResult> DeleteBed(int? id)
        {
            if (id == null) return NotFound();

            var bed = await _context.Beds
                .Include(b => b.Room)
                .ThenInclude(r => r.Ward)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bed == null) return NotFound();

            return View(bed);
        }

        // POST: BedManagement/DeleteBed/5 (Hard Delete)
        [HttpPost, ActionName("DeleteBed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed != null)
            {
                var bedNumber = bed.BedNumber;
                _context.Beds.Remove(bed);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bed '{bedNumber}' has been permanently deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDeleteBed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed != null)
            {
                var bedNumber = bed.BedNumber;
                bed.IsActive = false; // This part performs the soft delete
                _context.Update(bed);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bed '{bedNumber}' has been deactivated successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: A specific bed details
        public async Task<IActionResult> DetailsBed(int? id)
        {
            if (id == null || _context.Beds == null)
            {
                return NotFound();
            }

            var bed = await _context.Beds
                .Include(b => b.Room)
                .ThenInclude(r => r.Ward)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bed == null) return NotFound();

            return View(bed);
        }

        private bool BedExists(int id)
        {
            return _context.Beds.Any(e => e.Id == id);
        }
    }
}