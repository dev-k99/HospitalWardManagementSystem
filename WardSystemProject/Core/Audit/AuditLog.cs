namespace WardSystemProject.Core.Audit
{
    /// <summary>
    /// Immutable audit record written by <see cref="AuditInterceptor"/> on every
    /// Create / Update / Delete operation.  Critical for POPIA compliance —
    /// healthcare systems must be able to prove who accessed or modified patient data.
    /// </summary>
    public class AuditLog
    {
        public int      Id         { get; init; }

        /// <summary>EF entity / DB table name (e.g. "Patients", "Prescriptions").</summary>
        public string   TableName  { get; init; } = string.Empty;

        /// <summary>INSERT, UPDATE, or DELETE.</summary>
        public string   Action     { get; init; } = string.Empty;

        /// <summary>Primary key value of the affected row.</summary>
        public string   EntityId   { get; init; } = string.Empty;

        /// <summary>ASP.NET Core Identity username of the authenticated user.</summary>
        public string   ChangedBy  { get; init; } = "System";

        public DateTime ChangedAt  { get; init; } = DateTime.UtcNow;

        /// <summary>JSON snapshot of values before the change (null for INSERT).</summary>
        public string?  OldValues  { get; init; }

        /// <summary>JSON snapshot of values after the change (null for DELETE).</summary>
        public string?  NewValues  { get; init; }
    }
}
