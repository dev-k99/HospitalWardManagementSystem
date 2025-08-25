using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.Models;

namespace WardSystemProject.Controllers
{
   [Authorize] // Restricts access to authenticated users
    public class RoomManagementController : Controller
    {
        private readonly WardSystemDBContext _context;

        public RoomManagementController(WardSystemDBContext context)
        {
            _context = context;
        }

        // GET: RoomManagement
        public async Task<IActionResult> ManageRooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Ward)
                .Where(r => r.IsActive)
                .ToListAsync();
            return View(rooms);
        }

        // This method searches for room numbers in the database
        public async Task<IActionResult> Search(string query)
        {
            var filteredRooms = await _context.Rooms
                .Include(r => r.Ward)
                .Where(r => r.IsActive && r.RoomNumber.Contains(query))
                .ToListAsync();

            return View("Index", filteredRooms);
        }

        // GET: RoomManagement/Create
        public IActionResult Create()
        {
            ViewBag.WardId = new SelectList(_context.Wards, "Id", "Name");
            return View();
        }

        // POST: RoomManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            try
            {
                room.IsActive = true;
                _context.Add(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Room '{room.RoomNumber}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            ViewBag.WardId = new SelectList(_context.Wards, "Id", "Name", room.WardId);
            return View(room);
        }

        // GET: RoomManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Ward)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();

            ViewBag.WardId = new SelectList(_context.Wards, "Id", "Name", room.WardId);
            return View(room);
        }

        // POST: RoomManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id) return NotFound();

            try
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Room '{room.RoomNumber}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(room.Id)) return NotFound();
                else throw;
            }

            ViewBag.WardId = new SelectList(_context.Wards, "Id", "Name", room.WardId);
            return View(room);
        }

        // GET: RoomManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Ward)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();

            return View(room);
        }

        // POST: RoomManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                var roomNumber = room.RoomNumber;
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Room '{roomNumber}' has been permanently deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: RoomManagement/SoftDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                var roomNumber = room.RoomNumber;
                room.IsActive = false;
                _context.Update(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Room '{roomNumber}' has been deactivated successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: RoomManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Rooms == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Ward)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();

            return View(room);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}