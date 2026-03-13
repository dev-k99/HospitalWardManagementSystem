using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.Models;

namespace WardSystemProject.Controllers
{
    [Authorize(Roles = "Script Manager,Consumables Manager,Administrator")]
    public class ConsumableScriptController : Controller
    {
        private readonly WardSystemDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ConsumableScriptController(WardSystemDBContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ConsumableScript/Index - Dashboard
        public async Task<IActionResult> Index()
        {
            var isScriptManager = User.IsInRole("Script Manager");
            var isConsumablesManager = User.IsInRole("Consumables Manager");

            var dashboard = new ConsumablesScriptViewModel
            {
                // Script Manager Statistics
                NewPrescriptions = isScriptManager ? await _context.Prescriptions
                    .Include(p => p.Patient)
                    .Include(p => p.Doctor)
                    .Include(p => p.Medication)
                    .Where(p => p.IsActive && !_context.PrescriptionOrders.Any(po => po.PrescriptionId == p.Id))
                    .OrderByDescending(p => p.PrescriptionDate)
                    .Take(10)
                    .ToListAsync() : new List<Prescription>(),

                PendingOrders = isScriptManager ? await _context.PrescriptionOrders
                    .Include(po => po.Prescription)
                    .Include(po => po.Prescription.Patient)
                    .Include(po => po.Prescription.Medication)
                    .Where(po => po.Status == "Pending")
                    .OrderByDescending(po => po.OrderDate)
                    .Take(10)
                    .ToListAsync() : new List<PrescriptionOrder>(),

                // Consumables Manager Statistics
                LowStockItems = isConsumablesManager ? await _context.Consumables
                    .Include(c => c.Ward)
                    .Where(c => c.QuantityOnHand <= c.ReorderLevel)
                    .OrderBy(c => c.QuantityOnHand)
                    .Take(10)
                    .ToListAsync() : new List<Consumable>(),

                PendingConsumableOrders = isConsumablesManager ? await _context.ConsumableOrders
                    .Include(co => co.Consumable)
                    .Include(co => co.StockManager)
                    .Where(co => co.Status == "Pending")
                    .OrderByDescending(co => co.OrderDate)
                    .Take(10)
                    .ToListAsync() : new List<ConsumableOrder>(),

                // Stock Take Information
                LastStockTake = isConsumablesManager ? await _context.StockTakes
                    .Include(st => st.Ward)
                    .OrderByDescending(st => st.StockTakeDate)
                    .FirstOrDefaultAsync() : null,

                IsScriptManager = isScriptManager,
                IsConsumablesManager = isConsumablesManager
            };

            return View(dashboard);
        }

        // SCRIPT MANAGEMENT SECTION

        // GET: ConsumableScript/NewPrescriptions - View new prescriptions
        
        public async Task<IActionResult> NewPrescriptions()
        {
            var newPrescriptions = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Medication)
                .Where(p => p.IsActive && !_context.PrescriptionOrders.Any(po => po.PrescriptionId == p.Id))
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            return View(newPrescriptions);
        }

        // GET: ConsumableScript/ProcessPrescription/5 - Process a prescription
       
        public async Task<IActionResult> ProcessPrescription(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return NotFound();

            // Check if already processed
            var existingOrder = await _context.PrescriptionOrders
                .FirstOrDefaultAsync(po => po.PrescriptionId == id);

            if (existingOrder != null)
            {
                TempData["ErrorMessage"] = "This prescription has already been processed.";
                return RedirectToAction(nameof(NewPrescriptions));
            }

            ViewBag.Prescription = prescription;
            return View(new PrescriptionOrder { PrescriptionId = id.Value });
        }

        // POST: ConsumableScript/ProcessPrescription
        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public async Task<IActionResult> ProcessPrescription([Bind("PrescriptionId,Notes")] PrescriptionOrder prescriptionOrder, bool sendToPharmacyNow = false)
        {
            var scriptManagerId = await GetCurrentStaffIdAsync();
            if (scriptManagerId == null) return NotFound();

            if (ModelState.IsValid)
            {
                prescriptionOrder.ScriptManagerId = scriptManagerId.Value;
                prescriptionOrder.OrderDate = DateTime.UtcNow;
                prescriptionOrder.IsActive = true;

                if (sendToPharmacyNow)
                {
                    prescriptionOrder.SentToPharmacy = DateTime.UtcNow;
                    prescriptionOrder.Status = "Sent to Pharmacy";
                }
                else
                {
                    prescriptionOrder.Status = "Pending";
                }

                _context.Add(prescriptionOrder);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = sendToPharmacyNow
                    ? "Prescription processed and sent to pharmacy successfully."
                    : "Prescription order created with Pending status.";
                return RedirectToAction(nameof(ManagePrescriptionOrders));
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(p => p.Id == prescriptionOrder.PrescriptionId);

            ViewBag.Prescription = prescription;
            return View(prescriptionOrder);
        }

        // GET: ConsumableScript/ManagePrescriptionOrders
       
        public async Task<IActionResult> ManagePrescriptionOrders()
        {
            var orders = await _context.PrescriptionOrders
                .Include(po => po.Prescription)
                .Include(po => po.Prescription.Patient)
                .Include(po => po.Prescription.Doctor)
                .Include(po => po.Prescription.Medication)
                .Include(po => po.ScriptManager)
                .Where(po => po.IsActive)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: ConsumableScript/ReceiveMedication/5 - Mark medication as received
        [HttpPost]
        [ValidateAntiForgeryToken]
         
        public async Task<IActionResult> ReceiveMedication(int id)
        {
            var order = await _context.PrescriptionOrders
                .Include(po => po.Prescription)
                .Include(po => po.Prescription.Patient)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null) return NotFound();

            order.ReceivedInWard = DateTime.UtcNow;
            order.Status = "Delivered";

            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Medication for {order.Prescription.Patient.FullName} marked as received.";
            return RedirectToAction(nameof(ManagePrescriptionOrders));
        }

        // CONSUMABLES MANAGEMENT SECTION

        // GET: ConsumableScript/CheckStock - View consumable stock levels
       
        public async Task<IActionResult> CheckStock()
        {
            var consumables = await _context.Consumables
                .Include(c => c.Ward)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Ward.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View(consumables);
        }

        // GET: ConsumableScript/UpdateStock/5 - Update stock levels
      
        public async Task<IActionResult> UpdateStock(int? id)
        {
            if (id == null) return NotFound();

            var consumable = await _context.Consumables
                .Include(c => c.Ward)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consumable == null) return NotFound();

            return View(consumable);
        }

        // POST: ConsumableScript/UpdateStock
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> UpdateStock(int id, [Bind("Id,QuantityOnHand,ReorderLevel")] Consumable consumable)
        {
            if (id != consumable.Id) return NotFound();

            var existingConsumable = await _context.Consumables.FindAsync(id);
            if (existingConsumable == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingConsumable.QuantityOnHand = consumable.QuantityOnHand;
                existingConsumable.ReorderLevel = consumable.ReorderLevel;
                existingConsumable.LastUpdated = DateTime.UtcNow;

                _context.Update(existingConsumable);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Stock levels updated successfully.";
                return RedirectToAction(nameof(CheckStock));
            }

            var fullConsumable = await _context.Consumables
                .Include(c => c.Ward)
                .FirstOrDefaultAsync(c => c.Id == id);

            return View(fullConsumable);
        }

        // GET: ConsumableScript/CreateConsumableOrder - Create new consumable order
 
        public async Task<IActionResult> CreateConsumableOrder()
        {
            ViewBag.ConsumableId = new SelectList(
                await _context.Consumables
                    .Include(c => c.Ward)
                    .Where(c => c.IsActive)
                    .ToListAsync(), 
                "Id", "Name");

            return View(new ConsumableOrder());
        }

        // POST: ConsumableScript/CreateConsumableOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> CreateConsumableOrder([Bind("ConsumableId,Quantity,Notes")] ConsumableOrder consumableOrder)
        {
            var stockManagerId = await GetCurrentStaffIdAsync();
            if (stockManagerId == null) return NotFound();

            if (ModelState.IsValid)
            {
                consumableOrder.StockManagerId = stockManagerId.Value;
                consumableOrder.OrderDate = DateTime.UtcNow;
                consumableOrder.Status = "Pending";
                consumableOrder.IsActive = true;

                _context.Add(consumableOrder);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Consumable order created successfully.";
                return RedirectToAction(nameof(ManageConsumableOrders));
            }

            ViewBag.ConsumableId = new SelectList(
                await _context.Consumables
                    .Include(c => c.Ward)
                    .Where(c => c.IsActive)
                    .ToListAsync(), 
                "Id", "Name", consumableOrder.ConsumableId);

            return View(consumableOrder);
        }

        // GET: ConsumableScript/ManageConsumableOrders
 
        public async Task<IActionResult> ManageConsumableOrders()
        {
            var orders = await _context.ConsumableOrders
                .Include(co => co.Consumable)
                .Include(co => co.Consumable.Ward)
                .Include(co => co.StockManager)
                .Where(co => co.IsActive)
                .OrderByDescending(co => co.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: ConsumableScript/ReceiveConsumables/5 - Mark consumables as received
        [HttpPost]
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> ReceiveConsumables(int id)
        {
            var order = await _context.ConsumableOrders
                .Include(co => co.Consumable)
                .FirstOrDefaultAsync(co => co.Id == id);

            if (order == null) return NotFound();

            // Update consumable stock
            order.Consumable.QuantityOnHand += order.Quantity;
            order.Consumable.LastUpdated = DateTime.UtcNow;

            // Update order status
            order.ReceivedDate = DateTime.UtcNow;
            order.Status = "Received";

            _context.Update(order);
            _context.Update(order.Consumable);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Consumables received and stock updated successfully.";
            return RedirectToAction(nameof(ManageConsumableOrders));
        }

        // STOCK TAKE SECTION

        public async Task<IActionResult> WeeklyStockTake()
        {
            var stockManagerId = await GetCurrentStaffIdAsync();
            if (stockManagerId == null) return NotFound();

            var consumables = await _context.Consumables
                .Include(c => c.Ward)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Ward.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewBag.Consumables = consumables;
            ViewBag.WardId = new SelectList(
                await _context.Wards.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(),
                "Id", "Name");
            return View(new StockTake { StockManagerId = stockManagerId.Value });
        }

        // POST: ConsumableScript/WeeklyStockTake
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> WeeklyStockTake([Bind("StockTakeDate,WardId,Notes")] StockTake stockTake, [FromForm] Dictionary<int, int> countedQty)
        {
            var stockManagerId = await GetCurrentStaffIdAsync();
            if (stockManagerId == null) return NotFound();

            if (ModelState.IsValid)
            {
                stockTake.StockManagerId = stockManagerId.Value;
                stockTake.IsActive = true;

                _context.Add(stockTake);
                await _context.SaveChangesAsync();

                // Create one StockTakeDetail per consumable
                var consumables = await _context.Consumables.Where(c => c.IsActive).ToListAsync();
                foreach (var consumable in consumables)
                {
                    var counted = countedQty.TryGetValue(consumable.Id, out var qty) ? qty : consumable.QuantityOnHand;
                    _context.Add(new StockTakeDetail
                    {
                        StockTakeId = stockTake.Id,
                        ConsumableId = consumable.Id,
                        SystemQuantity = consumable.QuantityOnHand,
                        CountedQuantity = counted
                    });
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Weekly stock take completed successfully.";
                return RedirectToAction(nameof(StockTakeHistory));
            }

            var allConsumables = await _context.Consumables
                .Include(c => c.Ward)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Ward.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewBag.Consumables = allConsumables;
            ViewBag.WardId = new SelectList(
                await _context.Wards.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(),
                "Id", "Name", stockTake.WardId);
            return View(stockTake);
        }

        // GET: ConsumableScript/StockTakeHistory - View stock take history
    
        public async Task<IActionResult> StockTakeHistory()
        {
            var stockTakes = await _context.StockTakes
                .Include(st => st.Ward)
                .Include(st => st.StockManager)
                .Where(st => st.IsActive)
                .OrderByDescending(st => st.StockTakeDate)
                .ToListAsync();

            return View(stockTakes);
        }

        // GET: ConsumableScript/StockTakeDetails/5
        public async Task<IActionResult> StockTakeDetails(int? id)
        {
            if (id == null) return NotFound();

            var stockTake = await _context.StockTakes
                .Include(st => st.Ward)
                .Include(st => st.StockManager)
                .FirstOrDefaultAsync(st => st.Id == id);

            if (stockTake == null) return NotFound();

            var details = await _context.StockTakeDetails
                .Include(d => d.Consumable)
                .Where(d => d.StockTakeId == id)
                .OrderBy(d => d.Consumable.Name)
                .ToListAsync();

            ViewBag.StockTake = stockTake;
            return View(details);
        }

        // GET: ConsumableScript/PrescribtionOrderDetails
        public async Task<IActionResult> PrescribtionOrderDetails(int? id)
        {
            if (id == null) return NotFound();

            var prescriptionOrder = await _context.PrescriptionOrders
                .Include(po => po.Prescription)
                    .ThenInclude(p => p.Patient)
                .Include(po => po.Prescription)
                    .ThenInclude(p => p.Medication)
                .Include(po => po.ScriptManager)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (prescriptionOrder == null) return NotFound();

            return View(prescriptionOrder);
        }

        // Resolves the current user's Staff.Id.
        // Primary: Staff.IdentityUserId == IdentityUser.Id (single indexed lookup).
        // Fallback: email match for Staff records created before IdentityUserId was added.
        private async Task<int?> GetCurrentStaffIdAsync()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return null;

            var identityUser = await _userManager.FindByNameAsync(userName);
            if (identityUser == null) return null;

            var staff = await _context.Staff
                            .FirstOrDefaultAsync(s => s.IdentityUserId == identityUser.Id && s.IsActive)
                        ?? await _context.Staff
                            .FirstOrDefaultAsync(s => s.Email == identityUser.Email && s.IsActive);

            return staff?.Id;
        }

        // Existing methods...
        private bool PrescriptionOrderExists(int id)
        {
            return _context.PrescriptionOrders.Any(e => e.Id == id);
        }

        private bool ConsumableOrderExists(int id)
        {
            return _context.ConsumableOrders.Any(e => e.Id == id);
        }

        private bool ConsumableExists(int id)
        {
            return _context.Consumables.Any(e => e.Id == id);
        }
    }

}
