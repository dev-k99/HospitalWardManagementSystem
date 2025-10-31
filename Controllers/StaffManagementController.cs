using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.Models;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class StaffManagementController : Controller
    {
        private readonly WardSystemDBContext _context;

        public StaffManagementController(WardSystemDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var staff = await _context.Staff.ToListAsync();
            return View(staff);
        }

        public IActionResult CreateStaff()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff([Bind("Id,FirstName,LastName,Role,Email")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                staff.IsActive = true;
                _context.Add(staff);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Staff member '{staff.FirstName} {staff.LastName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        public async Task<IActionResult> EditStaff(int? id)
        {
            if (id == null) return NotFound();
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(int id, [Bind("Id,FirstName,LastName,Role,Email")] Staff staff)
        {
            if (id != staff.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Staff member '{staff.FirstName} {staff.LastName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffExists(staff.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        public async Task<IActionResult> DeleteStaff(int? id)
        {
            if (id == null) return NotFound();
            var staff = await _context.Staff.FirstOrDefaultAsync(m => m.Id == id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost, ActionName("DeleteStaff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            
            var staff = await _context.Staff.FindAsync(id);
            if(staff!=null)
            {
                var staffName = $"{staff.FirstName} {staff.LastName}";
                _context.Staff.Remove(staff);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Staff member '{staffName}' has been permanently deleted.";
            }
           
            return RedirectToAction(nameof(Index));
        }

        //SoftDeleteStaff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDeleteStaff(int id)
        {

            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                var staffName = $"{staff.FirstName} {staff.LastName}";
                staff.IsActive = false;//SoftDeletes
                _context.Staff.Update(staff);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Staff member '{staffName}' has been deactivated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(int id)
        {
            return _context.Staff.Any(e => e.Id == id);
        }

        public async Task<IActionResult> DetailsStaff(int? id)
        {
            if (id == null) return NotFound();
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        public async Task<IActionResult> Search(string query, string role, string status, string department)
        {
            var staffQuery = _context.Staff.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                staffQuery = staffQuery.Where(s => 
                    s.FirstName.Contains(query) || 
                    s.LastName.Contains(query) || 
                    s.Email.Contains(query));
            }

            if (!string.IsNullOrEmpty(role))
            {
                staffQuery = staffQuery.Where(s => s.Role == role);
            }

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "Active";
                staffQuery = staffQuery.Where(s => s.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(department))
            {
                // Map department to role for filtering
                switch (department)
                {
                    case "Medical":
                        staffQuery = staffQuery.Where(s => s.Role == "Doctor");
                        break;
                    case "Nursing":
                        staffQuery = staffQuery.Where(s => s.Role == "Nurse" || s.Role == "Nursing Sister");
                        break;
                    case "Administration":
                        staffQuery = staffQuery.Where(s => s.Role == "Administrator");
                        break;
                    case "Support":
                        staffQuery = staffQuery.Where(s => s.Role != "Doctor" && s.Role != "Nurse" && s.Role != "Nursing Sister" && s.Role != "Administrator");
                        break;
                }
            }

            var staff = await staffQuery.ToListAsync();
            return View("ManageStaff", staff);
        }
    }
}
