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

        // GET: /BedManagement
        // Retrieving all Beds including their Room and Ward
        public async Task<IActionResult> Index()
        {
            var beds = await _context.Beds
                .Include(b => b.Room)
                .ThenInclude(r => r.Ward)
                .Where(b => b.IsActive)
                .ToListAsync();

            return View(beds);
        }

        // This method searches for bed numbers or room numbers in our database
        public async Task<IActionResult> Search(string query)
        {
            var filteredBeds = await _context.Beds
                .Include(b => b.Room)
                .ThenInclude(r => r.Ward)
                .Where(b => b.BedNumber.Contains(query) || b.Room.RoomNumber.Contains(query))
                .ToListAsync();

            return View("Index", filteredBeds);
        }

        // GET: BedManagement/CreateBed
        public IActionResult CreateBed()
        {
            ViewBag.RoomId = new SelectList(_context.Rooms.ToList(), "Id", "RoomNumber");
            return View();
        }

        // POST: BedManagement/CreateBed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBed([Bind("Id,BedNumber,RoomId")] Bed bed)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bed.IsActive = true; // Ensure new beds are active by default
                    _context.Add(bed);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Bed '{bed.BedNumber}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");
            }

            // Repopulate dropdown if model is invalid
            ViewBag.RoomId = new SelectList(_context.Rooms.ToList(), "Id", "RoomNumber", bed.RoomId);
            return View(bed);
        }

        // GET: BedManagement/EditBed/5
        public async Task<IActionResult> EditBed(int? id)
        {
            if (id == null) return NotFound();

            var bed = await _context.Beds
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bed == null) return NotFound();

            ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "RoomNumber", bed.RoomId);
            return View(bed);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBed(int id, [Bind("Id,BedNumber,RoomId")] Bed bed)
        {
            if (id != bed.Id) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(bed);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Bed '{bed.BedNumber}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BedExists(bed.Id)) return NotFound();
                else throw;
            }

            ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "RoomNumber", bed.RoomId);
            return View(bed);
        }

        // GET: BedManagement/DeleteBed/5
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