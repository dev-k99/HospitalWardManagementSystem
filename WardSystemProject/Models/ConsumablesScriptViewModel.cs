using System.Collections.Generic;

namespace WardSystemProject.Models
{
    public class ConsumablesScriptViewModel
    {
        // Role flags
        public bool IsScriptManager { get; set; }
        public bool IsConsumablesManager { get; set; }

        // Script Manager properties
        public List<Prescription> NewPrescriptions { get; set; } = new List<Prescription>();
        public List<PrescriptionOrder> PendingOrders { get; set; } = new List<PrescriptionOrder>();

        // Consumables Manager properties
        public List<Consumable> LowStockItems { get; set; } = new List<Consumable>();
        public List<ConsumableOrder> PendingConsumableOrders { get; set; } = new List<ConsumableOrder>();
        public StockTake? LastStockTake { get; set; }

        // Additional statistics for display
        public int TotalNewPrescriptions => NewPrescriptions?.Count ?? 0;
        public int TotalPendingOrders => PendingOrders?.Count ?? 0;
        public int TotalLowStockItems => LowStockItems?.Count ?? 0;
        public int TotalPendingConsumableOrders => PendingConsumableOrders?.Count ?? 0;
    }
}
