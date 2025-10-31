using System;
using System.Collections.Generic;

namespace WardSystemProject.Models
{
    public class StaffDashboardViewModel
    {
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public int OnDutyStaff { get; set; }
        public int OffDutyStaff { get; set; }
        public int TotalNurses { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalSupportStaff { get; set; }
        public int TotalAdmissions { get; set; }
        public int TotalDischarges { get; set; }
        public int TotalPatientMovements { get; set; }
        
        public List<StaffRoleInfo> StaffByRole { get; set; } = new List<StaffRoleInfo>();
        public List<StaffScheduleInfo> TodaysSchedule { get; set; } = new List<StaffScheduleInfo>();
        public List<StaffPerformanceInfo> PerformanceMetrics { get; set; } = new List<StaffPerformanceInfo>();
    }

    public class StaffRoleInfo
    {
        public string Role { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class StaffScheduleInfo
    {
        public string StaffName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class StaffPerformanceInfo
    {
        public string StaffName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int PatientsAttended { get; set; }
        public int MedicationsAdministered { get; set; }
        public double SatisfactionRating { get; set; }
        public string PerformanceStatus { get; set; } = "Good";
    }
}
