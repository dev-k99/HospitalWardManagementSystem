namespace WardSystemProject.Models
{
    public class DoctorDashboardViewModel
    {
        public int TotalPatients { get; set; }
        public List<WardSystemProject.Models.DoctorVisit> RecentVisits { get; set; }
        public List<WardSystemProject.Models.DoctorInstruction> PendingInstructions { get; set; }
    }
}
