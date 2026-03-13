using Microsoft.EntityFrameworkCore;
using WardSystemProject.Models;
using WardSystemProject.Core.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WardSystemProject.Data
{
    public class WardSystemDBContext : IdentityDbContext<IdentityUser>
    {
        public WardSystemDBContext(DbContextOptions<WardSystemDBContext> options)
            : base(options)
        {
        }

        // ── Audit ──────────────────────────────────────────────────────────
        public DbSet<AuditLog> AuditLogs { get; set; }

        // ── Administration ─────────────────────────────────────────────────
        public DbSet<Ward>             Wards             { get; set; }
        public DbSet<Room>             Rooms             { get; set; }
        public DbSet<Bed>              Beds              { get; set; }
        public DbSet<Staff>            Staff             { get; set; }
        public DbSet<Medication>       Medications       { get; set; }
        public DbSet<Consumable>       Consumables       { get; set; }
        public DbSet<Allergy>          Allergies         { get; set; }
        public DbSet<MedicalCondition> MedicalConditions { get; set; }
        public DbSet<DoctorVisit>      DoctorVisits      { get; set; }

        // ── Patient Management ─────────────────────────────────────────────
        public DbSet<Patient>         Patients         { get; set; }
        public DbSet<PatientMovement> PatientMovements { get; set; }

        // ── Patient Care ───────────────────────────────────────────────────
        public DbSet<VitalSign>               VitalSigns               { get; set; }
        public DbSet<MedicationAdministration> MedicationAdministrations { get; set; }
        public DbSet<DoctorInstruction>        DoctorInstructions        { get; set; }

        // ── Doctor-Patient ─────────────────────────────────────────────────
        public DbSet<Prescription> Prescriptions { get; set; }

        // ── Consumables & Script Management ───────────────────────────────
        public DbSet<PrescriptionOrder> PrescriptionOrders { get; set; }
        public DbSet<ConsumableOrder>   ConsumableOrders   { get; set; }
        public DbSet<StockTake>         StockTakes         { get; set; }
        public DbSet<StockTakeDetail>   StockTakeDetails   { get; set; }
        public DbSet<PatientFolder>     PatientFolders     { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Disable cascade delete for PatientMovement.FromWardId
            modelBuilder.Entity<PatientMovement>()
                .HasOne(pm => pm.FromWard)
                .WithMany() // No inverse navigation property defined
                .HasForeignKey(pm => pm.FromWardId)
                .OnDelete(DeleteBehavior.NoAction);

            // Disable cascade delete for PatientMovement.ToWardId
            modelBuilder.Entity<PatientMovement>()
                .HasOne(pm => pm.ToWard)
                .WithMany() // No inverse navigation property defined
                .HasForeignKey(pm => pm.ToWardId)
                .OnDelete(DeleteBehavior.NoAction);

            // Existing configurations from previous fix (e.g., PrescriptionOrder, Prescription)
            modelBuilder.Entity<PrescriptionOrder>()
                .HasOne(po => po.ScriptManager)
                .WithMany()
                .HasForeignKey(po => po.ScriptManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PrescriptionOrder>()
                .HasOne(po => po.Prescription)
                .WithMany()
                .HasForeignKey(po => po.PrescriptionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Staff.WardId — nullable FK; use SetNull so deleting a ward
            // un-assigns nurses rather than blocking the delete.
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Ward)
                .WithMany()
                .HasForeignKey(s => s.WardId)
                .OnDelete(DeleteBehavior.SetNull);

            // Query Filters
            modelBuilder.Entity<Ward>().HasQueryFilter(w => w.IsActive);
            modelBuilder.Entity<Bed>().HasQueryFilter(b => b.IsActive);
            modelBuilder.Entity<PrescriptionOrder>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<ConsumableOrder>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<DoctorVisit>().HasQueryFilter(d => d.IsActive);
            modelBuilder.Entity<Prescription>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<VitalSign>().HasQueryFilter(v => v.IsActive);
            modelBuilder.Entity<MedicationAdministration>().HasQueryFilter(m => m.IsActive);
            modelBuilder.Entity<DoctorInstruction>().HasQueryFilter(d => d.IsActive);
            modelBuilder.Entity<Patient>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<Room>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Staff>().HasQueryFilter(s => s.IsActive);

            base.OnModelCreating(modelBuilder);
        }

        

    }
}
