using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WardSystemProject.Data;
using WardSystemProject.Models;

namespace WardSystemProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly WardSystemDBContext _context;

    public HomeController(ILogger<HomeController> logger, WardSystemDBContext context)
    {
        _logger = logger;
        _context = context;
    }
    public IActionResult FAQs()
    {
        return View();
    }
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var dashboardViewModel = new DashboardViewModel();

        if (User.IsInRole("Administrator"))
        {
            dashboardViewModel.TotalWards = await _context.Wards.Where(w => w.IsActive).CountAsync();
            dashboardViewModel.TotalRooms = await _context.Rooms.Where(r => r.IsActive).CountAsync();
            dashboardViewModel.TotalBeds = await _context.Beds.Where(b => b.IsActive).CountAsync();
            dashboardViewModel.TotalStaff = await _context.Staff.Where(s => s.IsActive).CountAsync();
            dashboardViewModel.TotalMedications = await _context.Medications.Where(m => m.IsActive).CountAsync();
            dashboardViewModel.TotalAllergies = await _context.Allergies.Where(a => a.IsActive).CountAsync();
            dashboardViewModel.TotalMedicalConditions = await _context.MedicalConditions.Where(m => m.IsActive).CountAsync();
        }
        else if (User.IsInRole("Ward Admin"))
        {
            dashboardViewModel.TotalPatients = await _context.Patients.Where(p => p.IsActive).CountAsync();
            dashboardViewModel.ActivePatients = await _context.Patients.Where(p => p.IsActive && p.WardId != null).CountAsync();
            dashboardViewModel.TotalPatientMovements = await _context.PatientMovements.Where(p => p.IsActive).CountAsync();
            dashboardViewModel.RecentMovements = await _context.PatientMovements
                .Include(p => p.Patient)
                .Include(p => p.FromWard)
                .Include(p => p.ToWard)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.MovementDate)
                .Take(5)
                .ToListAsync();
        }
        else if (User.IsInRole("Doctor"))
        {
            dashboardViewModel.TotalPatients = await _context.Patients.Where(p => p.IsActive).CountAsync();
            dashboardViewModel.TotalVisits = await _context.DoctorVisits.Where(d => d.IsActive).CountAsync();
            dashboardViewModel.TotalPrescriptions = await _context.Prescriptions.Where(p => p.IsActive).CountAsync();
            dashboardViewModel.RecentVisits = await _context.DoctorVisits
                .Include(d => d.Patient)
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.VisitDate)
                .Take(5)
                .ToListAsync();
        }
        else if (User.IsInRole("Nurse") || User.IsInRole("Nursing Sister"))
        {
            dashboardViewModel.TotalPatients = await _context.Patients.Where(p => p.IsActive).CountAsync();
            dashboardViewModel.TotalVitalSigns = await _context.VitalSigns.Where(v => v.IsActive).CountAsync();
            dashboardViewModel.TotalMedicationAdministrations = await _context.MedicationAdministrations.Where(m => m.IsActive).CountAsync();
            dashboardViewModel.TotalDoctorInstructions = await _context.DoctorInstructions.Where(d => d.IsActive).CountAsync();
            dashboardViewModel.RecentVitalSigns = await _context.VitalSigns
                .Include(v => v.Patient)
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.RecordDate)
                .Take(5)
                .ToListAsync();
        }
        else if (User.IsInRole("Script Manager"))
        {
            dashboardViewModel.TotalPrescriptionOrders = await _context.PrescriptionOrders.Where(p => p.IsActive).CountAsync();
            dashboardViewModel.PendingPrescriptionOrders = await _context.PrescriptionOrders
                .Where(p => p.IsActive && p.Status == "Pending")
                .CountAsync();
            dashboardViewModel.ProcessedPrescriptionOrders = await _context.PrescriptionOrders
                .Where(p => p.IsActive && p.Status == "Processed")
                .CountAsync();
            dashboardViewModel.RecentPrescriptionOrders = await _context.PrescriptionOrders
                .Include(p => p.Prescription)
                .ThenInclude(pr => pr.Patient)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.OrderDate)
                .Take(5)
                .ToListAsync();
        }
        else if (User.IsInRole("Consumables Manager"))
        {
            dashboardViewModel.TotalConsumables = await _context.Consumables.Where(c => c.IsActive).CountAsync();
            dashboardViewModel.TotalConsumableOrders = await _context.ConsumableOrders.Where(c => c.IsActive).CountAsync();
            dashboardViewModel.PendingConsumableOrders = await _context.ConsumableOrders
                .Where(c => c.IsActive && c.Status == "Pending")
                .CountAsync();
            dashboardViewModel.LowStockConsumables = await _context.Consumables
                .Where(c => c.IsActive && c.QuantityOnHand <= 10)
                .CountAsync();
            dashboardViewModel.RecentConsumableOrders = await _context.ConsumableOrders
                .Include(c => c.Consumable)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.OrderDate)
                .Take(5)
                .ToListAsync();
        }

        return View(dashboardViewModel);
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
}
