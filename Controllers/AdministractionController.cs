using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Administrator")]

    // Restricts access to authenticated users
    public class AdministrationController : Controller
    {
        private readonly WardSystemDBContext _context;

        public AdministrationController(WardSystemDBContext context)
        {
            _context = context;
        }

        // GET: /Administration
        // Retrieving all Wards
        public async Task<IActionResult> Index()
        {
            var wards = await _context.Wards
                .Include(w => w.Rooms) // Assuming a relationship with Rooms, adjust if different
                .Where(w => w.IsActive)
                .ToListAsync();

            return View(wards);
        }

        // This method searches for ward names in our database
        public async Task<IActionResult> Search(string query)
        {
            var filteredWards = await _context.Wards
                .Include(w => w.Rooms)
                .Where(w => w.Name.Contains(query) || w.Description.Contains(query))
                .ToListAsync();

            return View("Index", filteredWards);
        }

        // GET: Administration/CreateWard
        public IActionResult CreateWard()
        {
            ViewBag.RoomList = new SelectList(_context.Rooms.ToList(), "Id", "RoomNumber"); // Adjust properties as needed
            return View();
        }

        // POST: Administration/CreateWard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWard([Bind("Name,Description")] Ward ward)
        {
            if (ModelState.IsValid)
            {
                ward.IsActive = true;
                _context.Add(ward);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Ward '{ward.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(ward);
        }

        // GET: Administration/EditWard/5
        public async Task<IActionResult> EditWard(int? id)
        {
            if (id == null) return NotFound();

            var ward = await _context.Wards.FindAsync(id);
            if (ward == null) return NotFound();

            return View(ward);
        }

        // POST: Administration/EditWard/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWard(int id, [Bind("Id,Name,Description,IsActive")] Ward ward)
        {
            if (id != ward.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ward);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Ward '{ward.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WardExists(ward.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ward);
        }

        // GET: Administration/DeleteWard/5
        public async Task<IActionResult> DeleteWard(int? id)
        {
            if (id == null) return NotFound();

            var ward = await _context.Wards
                .Include(w => w.Rooms)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (ward == null) return NotFound();

            return View(ward);
        }

        // POST: Administration/DeleteWard/5
        [HttpPost, ActionName("DeleteWard")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward != null)
            {
                var wardName = ward.Name;
                _context.Wards.Remove(ward);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Ward '{wardName}' deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDeleteWard(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward != null)
            {
                var wardName = ward.Name;
                ward.IsActive = false; // This part performs the soft delete
                _context.Update(ward);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Ward '{wardName}' deactivated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: A specific ward details
        public async Task<IActionResult> DetailsWard(int? id)
        {
            if (id == null || _context.Wards == null)
            {
                return NotFound();
            }

            var ward = await _context.Wards
                .Include(w => w.Rooms)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (ward == null) return NotFound();

            return View(ward);
        }

        // Medication Management Actions
        public async Task<IActionResult> ManageMedications()
        {
            var medications = await _context.Medications.Where(m => m.IsActive).ToListAsync();
            return View(medications);
        }

        public IActionResult CreateMedication()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedication([Bind("Name,Description,Dosage")] Medication medication)
        {
            if (ModelState.IsValid)
            {
                medication.IsActive = true;
                _context.Add(medication);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Medication '{medication.Name}' created successfully!";
                return RedirectToAction(nameof(ManageMedications));
            }
            return View(medication);
        }

        public async Task<IActionResult> EditMedication(int? id)
        {
            if (id == null) return NotFound();
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null) return NotFound();
            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedication(int id, [Bind("Id,Name,Description,Dosage,IsActive")] Medication medication)
        {
            if (id != medication.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Medication '{medication.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(medication.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageMedications));
            }
            return View(medication);
        }

        public async Task<IActionResult> DeleteMedication(int? id)
        {
            if (id == null) return NotFound();
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null) return NotFound();
            return View(medication);
        }

        [HttpPost, ActionName("DeleteMedication")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicationConfirmed(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication != null)
            {
                var medicationName = medication.Name;
                medication.IsActive = false;
                _context.Update(medication);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Medication '{medicationName}' deactivated successfully!";
            }
            return RedirectToAction(nameof(ManageMedications));
        }

        // Allergy Management Actions
        public async Task<IActionResult> ManageAllergies()
        {
            var allergies = await _context.Allergies
                .Include(a => a.Patient)
                .Where(a => a.IsActive)
                .ToListAsync();
            return View(allergies);
        }

        public IActionResult CreateAllergy()
        {
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAllergy([Bind("PatientId,AllergyName")] Allergy allergy)
        {
            if (ModelState.IsValid)
            {
                allergy.IsActive = true;
                _context.Add(allergy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Allergy '{allergy.AllergyName}' created successfully!";
                return RedirectToAction(nameof(ManageAllergies));
            }
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName", allergy.PatientId);
            return View(allergy);
        }

        public async Task<IActionResult> EditAllergy(int? id)
        {
            if (id == null) return NotFound();
            var allergy = await _context.Allergies
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (allergy == null) return NotFound();
            return View(allergy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAllergy(int id, [Bind("Id,PatientId,AllergyName,IsActive")] Allergy allergy)
        {
            if (id != allergy.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(allergy);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Allergy '{allergy.AllergyName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AllergyExists(allergy.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageAllergies));
            }
            return View(allergy);
        }

        public async Task<IActionResult> DeleteAllergy(int? id)
        {
            if (id == null) return NotFound();
            var allergy = await _context.Allergies
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (allergy == null) return NotFound();
            return View(allergy);
        }

        [HttpPost, ActionName("DeleteAllergy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllergyConfirmed(int id)
        {
            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy != null)
            {
                var allergyName = allergy.AllergyName;
                allergy.IsActive = false;
                _context.Update(allergy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Allergy '{allergyName}' deactivated successfully!";
            }
                            return RedirectToAction(nameof(ManageAllergies));
        }

        // Medical Condition Management Actions
        public async Task<IActionResult> ManageMedicalConditions()
        {
            var conditions = await _context.MedicalConditions
                .Include(m => m.Patient)
                .Where(m => m.IsActive)
                .ToListAsync();
            return View(conditions);
        }

        public IActionResult CreateMedicalCondition()
        {
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicalCondition([Bind("PatientId,ConditionName")] MedicalCondition condition)
        {
            if (ModelState.IsValid)
            {
                condition.IsActive = true;
                _context.Add(condition);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Medical condition '{condition.ConditionName}' created successfully!";
                return RedirectToAction(nameof(ManageMedicalConditions));
            }
            ViewBag.PatientId = new SelectList(_context.Patients.Where(p => p.IsActive), "Id", "FirstName", condition.PatientId);
            return View(condition);
        }

        public async Task<IActionResult> EditMedicalCondition(int? id)
        {
            if (id == null) return NotFound();
            var condition = await _context.MedicalConditions
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (condition == null) return NotFound();
            return View(condition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicalCondition(int id, [Bind("Id,PatientId,ConditionName,IsActive")] MedicalCondition condition)
        {
            if (id != condition.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(condition);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Medical condition '{condition.ConditionName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalConditionExists(condition.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageMedicalConditions));
            }
            return View(condition);
        }

        public async Task<IActionResult> DeleteMedicalCondition(int? id)
        {
            if (id == null) return NotFound();
            var condition = await _context.MedicalConditions
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (condition == null) return NotFound();
            return View(condition);
        }

        [HttpPost, ActionName("DeleteMedicalCondition")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicalConditionConfirmed(int id)
        {
            var condition = await _context.MedicalConditions.FindAsync(id);
            if (condition != null)
            {
                var conditionName = condition.ConditionName;
                condition.IsActive = false;
                _context.Update(condition);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Medical condition '{conditionName}' deactivated successfully!";
            }
            return RedirectToAction(nameof(ManageMedicalConditions));
        }

        // Ward Management Actions
        public async Task<IActionResult> ManageWards()
        {
            var wards = await _context.Wards
                .Include(w => w.Rooms)
                .Where(w => w.IsActive)
                .ToListAsync();
            return View(wards);
        }

        // Staff Management Actions
        public async Task<IActionResult> ManageStaff()
        {
            var staff = await _context.Staff
                .Where(s => s.IsActive)
                .ToListAsync();
            return View(staff);
        }

        public IActionResult CreateStaff()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff([Bind("FirstName,LastName,Role,Email")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                staff.IsActive = true;
                _context.Add(staff);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Staff member '{staff.FirstName} {staff.LastName}' created successfully!";
                return RedirectToAction(nameof(ManageStaff));
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
        public async Task<IActionResult> EditStaff(int id, [Bind("Id,FirstName,LastName,Role,Email,IsActive")] Staff staff)
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
                return RedirectToAction(nameof(ManageStaff));
            }
            return View(staff);
        }

        public async Task<IActionResult> DeleteStaff(int? id)
        {
            if (id == null) return NotFound();

            var staff = await _context.Staff.FindAsync(id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        [HttpPost, ActionName("DeleteStaff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaffConfirmed(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                var staffName = $"{staff.FirstName} {staff.LastName}";
                staff.IsActive = false;
                _context.Update(staff);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Staff member '{staffName}' deactivated successfully!";
            }
            return RedirectToAction(nameof(ManageStaff));
        }

        // Room Management Actions
        public async Task<IActionResult> ManageRooms()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Ward)
                .Include(r => r.Beds)
                .Where(r => r.IsActive)
                .ToListAsync();
            return View(rooms);
        }

        public IActionResult CreateRoom()
        {
            ViewBag.WardList = new SelectList(_context.Wards.Where(w => w.IsActive).ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom([Bind("RoomNumber,WardId")] Room room)
        {
            if (ModelState.IsValid)
            {
                room.IsActive = true;
                _context.Add(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Room '{room.RoomNumber}' created successfully!";
                return RedirectToAction(nameof(ManageRooms));
            }
            ViewBag.WardList = new SelectList(_context.Wards.Where(w => w.IsActive).ToList(), "Id", "Name");
            return View(room);
        }

        public async Task<IActionResult> EditRoom(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            ViewBag.WardList = new SelectList(_context.Wards.Where(w => w.IsActive).ToList(), "Id", "Name");
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, [Bind("Id,RoomNumber,WardId,IsActive")] Room room)
        {
            if (id != room.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Room '{room.RoomNumber}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageRooms));
            }
            ViewBag.WardList = new SelectList(_context.Wards.Where(w => w.IsActive).ToList(), "Id", "Name");
            return View(room);
        }

        public async Task<IActionResult> DeleteRoom(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Ward)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();

            return View(room);
        }

        [HttpPost, ActionName("DeleteRoom")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                var roomNumber = room.RoomNumber;
                room.IsActive = false;
                _context.Update(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Room '{roomNumber}' deactivated successfully!";
            }
            return RedirectToAction(nameof(ManageRooms));
        }

        // Bed Management Actions
        public async Task<IActionResult> ManageBeds()
        {
            var beds = await _context.Beds
                .Include(b => b.Room)
                .Include(b => b.Room.Ward)
                .Include(b => b.Patient)
                .Where(b => b.IsActive)
                .ToListAsync();
            return View(beds);
        }

        public IActionResult CreateBed()
        {
            ViewBag.RoomList = new SelectList(_context.Rooms.Where(r => r.IsActive).ToList(), "Id", "RoomNumber");
            ViewBag.PatientList = new SelectList(_context.Patients.Where(p => p.IsActive).ToList(), "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBed([Bind("BedNumber,RoomId,PatientId")] Bed bed)
        {
            if (ModelState.IsValid)
            {
                bed.IsActive = true;
                _context.Add(bed);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bed '{bed.BedNumber}' created successfully!";
                return RedirectToAction(nameof(ManageBeds));
            }
            ViewBag.RoomList = new SelectList(_context.Rooms.Where(r => r.IsActive).ToList(), "Id", "RoomNumber");
            ViewBag.PatientList = new SelectList(_context.Patients.Where(p => p.IsActive).ToList(), "Id", "FirstName");
            return View(bed);
        }

        public async Task<IActionResult> EditBed(int? id)
        {
            if (id == null) return NotFound();

            var bed = await _context.Beds.FindAsync(id);
            if (bed == null) return NotFound();

            ViewBag.RoomList = new SelectList(_context.Rooms.Where(r => r.IsActive).ToList(), "Id", "RoomNumber");
            ViewBag.PatientList = new SelectList(_context.Patients.Where(p => p.IsActive).ToList(), "Id", "FirstName");
            return View(bed);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBed(int id, [Bind("Id,BedNumber,RoomId,PatientId,IsActive")] Bed bed)
        {
            if (id != bed.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bed);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Bed '{bed.BedNumber}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BedExists(bed.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageBeds));
            }
            ViewBag.RoomList = new SelectList(_context.Rooms.Where(r => r.IsActive).ToList(), "Id", "RoomNumber");
            ViewBag.PatientList = new SelectList(_context.Patients.Where(p => p.IsActive).ToList(), "Id", "FirstName");
            return View(bed);
        }

        public async Task<IActionResult> DeleteBed(int? id)
        {
            if (id == null) return NotFound();

            var bed = await _context.Beds
                .Include(b => b.Room)
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bed == null) return NotFound();

            return View(bed);
        }

        [HttpPost, ActionName("DeleteBed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBedConfirmed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed != null)
            {
                var bedNumber = bed.BedNumber;
                bed.IsActive = false;
                _context.Update(bed);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bed '{bedNumber}' deactivated successfully!";
            }
            return RedirectToAction(nameof(ManageBeds));
        }

        private bool WardExists(int id)
        {
            return _context.Wards.Any(e => e.Id == id);
        }

        private bool MedicationExists(int id)
        {
            return _context.Medications.Any(e => e.Id == id);
        }

        private bool AllergyExists(int id)
        {
            return _context.Allergies.Any(e => e.Id == id);
        }

        private bool MedicalConditionExists(int id)
        {
            return _context.MedicalConditions.Any(e => e.Id == id);
        }

        private bool StaffExists(int id)
        {
            return _context.Staff.Any(e => e.Id == id);
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        private bool BedExists(int id)
        {
            return _context.Beds.Any(e => e.Id == id);
        }
    }
}