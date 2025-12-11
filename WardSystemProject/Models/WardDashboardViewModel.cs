using System;
using System.Collections.Generic;

namespace WardSystemProject.Models
{
    public class WardDashboardViewModel
    {
        public int TotalWards { get; set; }
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public int TotalStaff { get; set; }
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int TotalPatientMovements { get; set; }
        public int TotalMedications { get; set; }
        public int TotalAllergies { get; set; }
        public int TotalMedicalConditions { get; set; }
        
        public List<WardStatusInfo> WardStatuses { get; set; } = new List<WardStatusInfo>();
        public List<RecentAdmissionInfo> RecentAdmissions { get; set; } = new List<RecentAdmissionInfo>();
        public List<AlertInfo> Alerts { get; set; } = new List<AlertInfo>();
    }

    public class WardStatusInfo
    {
        public int? WardId { get; set; }
        public string WardName { get; set; } = string.Empty;
        public int? TotalRooms { get; set; }
        public int? TotalBeds { get; set; }
        public int? OccupiedBeds { get; set; }
        public int? AvailableBeds { get; set; }
        public string Status { get; set; } = "Inactive";
        public double Trend { get; set; }
    }

    public class RecentAdmissionInfo
    {
        public string PatientName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public DateTime? AdmissionDate { get; set; }
    }

    public class AlertInfo
    {
        public string Message { get; set; } = string.Empty;
        public string AlertType { get; set; } = "info";
        public string WardName { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
    }
}
