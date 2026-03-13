using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using WardSystemProject.Core.Audit;

namespace WardSystemProject.Core.Audit
{
    /// <summary>
    /// EF Core <see cref="SaveChangesInterceptor"/> that automatically writes an
    /// <see cref="AuditLog"/> row for every tracked Create / Update / Delete.
    /// Registered via <c>DbContextOptionsBuilder.AddInterceptors()</c> in Program.cs.
    ///
    /// Design notes:
    ///  - Captures OLD values before SaveChanges (OriginalValues).
    ///  - Captures NEW values after SaveChanges (CurrentValues).
    ///  - Excludes the AuditLogs table itself to avoid infinite recursion.
    ///  - Uses IHttpContextAccessor to identify the current authenticated user.
    /// </summary>
    public sealed class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Entities to skip (audit of the audit table would cause recursion).
        private static readonly HashSet<string> _excluded =
            new(StringComparer.OrdinalIgnoreCase) { "AuditLog" };

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            // Snapshot old values before the save commits.
            if (eventData.Context is not null)
                _pendingEntries = SnapshotChanges(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null && _pendingEntries?.Count > 0)
            {
                var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
                var now         = DateTime.UtcNow;

                foreach (var entry in _pendingEntries)
                {
                    entry.ChangedBy = currentUser;
                    entry.ChangedAt = now;

                    // Resolve the auto-generated PK after INSERT.
                    if (entry.EntityId == "0" || string.IsNullOrEmpty(entry.EntityId))
                    {
                        var entityEntry = eventData.Context.Entry(entry.Entity!);
                        var pk = entityEntry.Metadata.FindPrimaryKey();
                        if (pk is not null)
                        {
                            var pkValue = entityEntry.Property(pk.Properties[0].Name).CurrentValue;
                            entry.EntityId = pkValue?.ToString() ?? string.Empty;
                        }
                    }

                    // Write the finalised AuditLog row directly via SQL-level insert
                    // to avoid triggering another round of change tracking.
                    var log = new AuditLog
                    {
                        TableName = entry.TableName,
                        Action    = entry.Action,
                        EntityId  = entry.EntityId,
                        ChangedBy = entry.ChangedBy,
                        ChangedAt = entry.ChangedAt,
                        OldValues = entry.OldValues,
                        NewValues = entry.NewValues
                    };
                    eventData.Context.Set<AuditLog>().Add(log);
                }

                await eventData.Context.SaveChangesAsync(cancellationToken);
                _pendingEntries = null;
            }

            return result;
        }

        // ── Snapshot helpers ─────────────────────────────────────────────────

        private List<PendingAuditEntry>? _pendingEntries;

        private static List<PendingAuditEntry> SnapshotChanges(DbContext context)
        {
            var entries = new List<PendingAuditEntry>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                    continue;

                var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name;
                if (_excluded.Contains(tableName))
                    continue;

                var action = entry.State switch
                {
                    EntityState.Added    => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted  => "DELETE",
                    _                    => "UNKNOWN"
                };

                // Primary key (may be 0 for new entities — resolved after save)
                var pk    = entry.Metadata.FindPrimaryKey();
                var pkVal = pk is not null
                    ? entry.Property(pk.Properties[0].Name).CurrentValue?.ToString() ?? string.Empty
                    : string.Empty;

                string? oldValues = null;
                string? newValues = null;

                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var old = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                        old[prop.Metadata.Name] = prop.OriginalValue;
                    oldValues = JsonSerializer.Serialize(old);
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var newDict = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                        newDict[prop.Metadata.Name] = prop.CurrentValue;
                    newValues = JsonSerializer.Serialize(newDict);
                }

                entries.Add(new PendingAuditEntry
                {
                    Entity    = entry.Entity,
                    TableName = tableName,
                    Action    = action,
                    EntityId  = pkVal,
                    OldValues = oldValues,
                    NewValues = newValues
                });
            }

            return entries;
        }

        // Mutable staging record (not persisted directly).
        private sealed class PendingAuditEntry
        {
            public object?  Entity    { get; init; }
            public string   TableName { get; set; } = string.Empty;
            public string   Action    { get; set; } = string.Empty;
            public string   EntityId  { get; set; } = string.Empty;
            public string   ChangedBy { get; set; } = "System";
            public DateTime ChangedAt { get; set; }
            public string?  OldValues { get; set; }
            public string?  NewValues { get; set; }
        }
    }
}
