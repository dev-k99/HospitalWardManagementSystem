namespace WardSystemProject.Models
{
    public class DashboardViewModel
    {
        // Administrator stats
        public int TotalWards { get; set; }
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public int TotalStaff { get; set; }
        public int TotalMedications { get; set; }
        public int TotalAllergies { get; set; }
        public int TotalMedicalConditions { get; set; }

        // Ward Admin stats
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int TotalPatientMovements { get; set; }
        public List<PatientMovement> RecentMovements { get; set; } = new List<PatientMovement>();

        // Doctor stats
        public int TotalVisits { get; set; }
        public int TotalPrescriptions { get; set; }
        public List<DoctorVisit> RecentVisits { get; set; } = new List<DoctorVisit>();

        // Nurse/Nursing Sister stats
        public int TotalVitalSigns { get; set; }
        public int TotalMedicationAdministrations { get; set; }
        public int TotalDoctorInstructions { get; set; }
        public List<VitalSign> RecentVitalSigns { get; set; } = new List<VitalSign>();

        // Script Manager stats
        public int TotalPrescriptionOrders { get; set; }
        public int PendingPrescriptionOrders { get; set; }
        public int ProcessedPrescriptionOrders { get; set; }
        public List<PrescriptionOrder> RecentPrescriptionOrders { get; set; } = new List<PrescriptionOrder>();

        // Consumables Manager stats
        public int TotalConsumables { get; set; }
        public int TotalConsumableOrders { get; set; }
        public int PendingConsumableOrders { get; set; }
        public int LowStockConsumables { get; set; }
        public List<ConsumableOrder> RecentConsumableOrders { get; set; } = new List<ConsumableOrder>();
    }
} 