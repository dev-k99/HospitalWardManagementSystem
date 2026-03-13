# WardCare+ — Ward Management System

### ONT3010 · ASP.NET Core 8 MVC · SQL Server · Portfolio Project

A production-quality hospital ward management system built for South African healthcare IT practice.
Implements the full ONT3010 specification covering patient admissions, nursing care, doctor visits,
medication administration, and inventory management — secured with role-based access control and
a full audit trail for POPIA compliance.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        WardCare+ MVC App                        │
│                                                                  │
│  Controllers (thin)          Features (business logic)           │
│  ┌──────────────────┐        ┌───────────────────────────────┐  │
│  │PatientManagement │───────►│ PatientService                │  │
│  │PatientCare       │───────►│ VitalSignService              │  │
│  │DoctorPatient     │        │ PrescriptionService           │  │
│  │Administration    │        │ PatientFolderPdfService       │  │
│  │ConsumableScript  │        └───────────────────────────────┘  │
│  └──────────────────┘                    │                       │
│                                          ▼                       │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │               WardSystemDBContext (EF Core 8)            │   │
│  │   + AuditInterceptor (SaveChangesInterceptor)            │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                  ASP.NET Core Identity                    │   │
│  │   7 roles · Policy-based authorization · Cookie auth     │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    SQL Server (EF Code-First)
```

### Feature Folder Structure

```
WardSystemProject/
├── Controllers/              # Thin controllers — validate, call service, return view
├── Core/
│   ├── Audit/                # AuditLog entity + EF SaveChangesInterceptor
│   └── Interfaces/           # IPatientService, IVitalSignService
├── Data/                     # WardSystemDBContext
├── Features/
│   ├── PatientManagement/    # PatientService, PatientFolderPdfService
│   └── PatientCare/          # VitalSignService
├── Models/                   # Domain entities (EF Code-First)
├── Validators/               # FluentValidation validators
├── ViewModels/               # One ViewModel per form — no domain entity on forms
├── Views/                    # Razor views organized by controller
└── wwwroot/                  # Bootstrap 5, site.css, static assets
WardSystemProject.Tests/
├── Helpers/                  # TestDbContextFactory (EF InMemory)
├── PatientServiceTests.cs    # Admission + discharge business rules
└── MedicationScheduleTests.cs # Schedule permission enforcement
```

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 (Code-First, SQL Server) |
| Auth | ASP.NET Core Identity + Cookie auth |
| Validation | FluentValidation 11 (async cross-entity validators) |
| PDF | QuestPDF 2024 (Community licence, MIT) |
| UI | Bootstrap 5.3 + Bootstrap Icons + DataTables + Select2 + Chart.js |
| Testing | xUnit 2.6 + Moq 4.20 + EF InMemory |
| Database | SQL Server (SQLEXPRESS dev / SQL Server prod) |

---

## Subsystems and Roles

### A — Administration (Administrator)
- Ward, Room, and Bed CRUD
- Staff management
- Medication catalogue (Name, Dosage, **Schedule 1–8**)
- Allergy and medical condition lookups
- User account creation (restricted to Administrator)

### B — Patient Management (Ward Admin)
- Admit patient to a specific bed (validates bed availability in a transaction)
- Edit patient demographics
- Discharge patient with mandatory discharge summary
- Patient movements (Ward Transfer, Theatre, X-Ray, Return)
- Download patient folder PDF / discharge summary PDF

### C — Nursing Care (Nurse + Nursing Sister)
- Record vital signs (temperature, pulse, BP, O₂ saturation, respiratory rate)
- Administer medications — **schedule enforcement** (see Business Rules)
- View and acknowledge doctor instructions

### D — Doctor Patient (Doctor)
- View my patients
- Record doctor visits with clinical notes
- Issue prescriptions → notifies Script Manager
- Add doctor instructions for nursing staff

### E — Inventory (Script Manager + Consumables Manager)
- Process prescription orders
- Record consumable stock and orders
- Weekly stock-takes
- Low-stock alerts on dashboard

---

## Business Rules

### Medication Schedule Restrictions (ONT3010 Spec)

> "A nurse is only allowed to dispense medication up to schedule 4.
>  Only a Nursing Sister may dispense any schedule 5 (or higher) medication."

This rule is enforced in `VitalSignService.AdministerAsync()` in the service layer —
independent of the HTTP pipeline. The unit tests in `MedicationScheduleTests` prove it:

```
AdministerAsync_Nurse_CanAdminister_Schedule4               ✓ PASS
AdministerAsync_Nurse_CannotAdminister_Schedule5_Throws     ✓ PASS
AdministerAsync_NursingSister_CanAdminister_Schedule5       ✓ PASS
AdministerAsync_Nurse_Schedule5_ExceptionContainsName       ✓ PASS
```

### Bed Assignment (Transactional)

Admitting a patient executes a DB transaction that:
1. Saves the patient record
2. Validates the target bed is unoccupied
3. Marks the bed as occupied and links it to the patient
4. Creates the initial `PatientMovement` record
5. Creates the `PatientFolder` record

If any step fails the entire admission is rolled back.

### Authorization Policies

```csharp
"CanManageWard"          → Administrator
"CanAdmitPatients"       → Ward Admin
"CanPrescribe"           → Doctor
"CanAdministerMeds"      → Nurse, Nursing Sister
"CanDispenseHighSchedule"→ Nursing Sister
"CanProcessScripts"      → Script Manager
"CanManageStock"         → Consumables Manager
```

---

## POPIA Compliance (South African POPIA Act)

The system applies several POPIA-aligned practices for personal health information:

- **Audit trail** — every INSERT / UPDATE / DELETE to any entity is captured in `AuditLogs`
  with the username, timestamp, table name, entity PK, and a JSON snapshot of old/new values.
  This satisfies the POPIA accountability principle (Section 8).
- **Access control** — patient data is only accessible to authenticated users with an
  appropriate role. Role separation ensures a Consumables Manager cannot see patient records.
- **Minimum necessary** — each role's views expose only the data required for that function.
- **Soft delete** — patient records are never hard-deleted (`IsActive = false`).
  Data is retained for the legally required retention period.
- **No test data in production** — seed credentials are read from `appsettings.Development.json`
  (git-ignored in production deployments) not hardcoded.

---

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server or SQL Server Express
- Visual Studio 2022 / VS Code / Rider

### 1 — Clone and restore packages

```bash
git clone <repo-url>
cd HospitalManagement
dotnet restore
```

### 2 — Configure the database connection

Edit `WardSystemProject/appsettings.json`:

```json
"ConnectionStrings": {
  "WardConn": "Server=.\\SQLEXPRESS;Database=WardSystemDB;Integrated Security=True;TrustServerCertificate=True;"
}
```

### 3 — Configure seed credentials (development only)

Edit `WardSystemProject/appsettings.Development.json`:

```json
{
  "SeedAdmin": {
    "Username": "admin",
    "Email": "admin@wardsystem.com",
    "Password": "Admin123!"
  }
}
```

> In production, supply `SeedAdmin__Password` as an environment variable or Azure Key Vault secret.
> The application will skip admin seeding if the password is absent.

### 4 — Run migrations

```bash
cd WardSystemProject
dotnet ef migrations add InitialSchema
dotnet ef database update
```

### 5 — Run the application

```bash
dotnet run
```

Navigate to `https://localhost:5001`. Log in with the seed admin credentials.

### 6 — Run unit tests

```bash
cd WardSystemProject.Tests
dotnet test
```

---

## Default Roles — Quick Reference

| Role | Primary Responsibility |
|------|----------------------|
| Administrator | System configuration, ward/bed/staff management, user accounts |
| Ward Admin | Patient admissions and discharges |
| Doctor | Patient consultations, prescriptions, instructions |
| Nurse | Vital signs, medication administration (Schedule ≤ 4) |
| Nursing Sister | Vital signs, medication administration (all schedules) |
| Script Manager | Process prescription orders from pharmacy |
| Consumables Manager | Stock management, consumable orders, stock-takes |

---

## Key Design Decisions

**Service layer over repository pattern**
The codebase uses a feature-folder service layer (`PatientService`, `VitalSignService`) rather
than the generic repository + unit-of-work pattern. For a project of this size, services are
more readable and avoid the abstraction overhead of generic repositories while still being
fully injectable and testable via interfaces.

**FluentValidation over DataAnnotations**
DataAnnotations are UI-only and cannot perform async cross-entity checks (e.g. "is this bed
available?"). FluentValidation validators integrate cleanly with ModelState and can query the
database, making them the right tool for admission and medication validation.

**QuestPDF over SSRS / iTextSharp**
QuestPDF uses a composable C# fluent API, requires no external designer, and its Community
licence is free for open-source and student projects. The resulting PDFs are fully
programmatic and version-controllable — no binary `.rdl` files.

**EF SaveChangesInterceptor for audit trail**
Placing audit logic in an EF interceptor means every save — regardless of which controller
or service triggered it — is automatically audited without any call-site changes. This is
preferable to manual audit calls scattered across controllers.

---

## Screenshots

> _(Add screenshots to `wwwroot/images/screenshots/` and reference them here)_

---

Built with ASP.NET Core 8 · Entity Framework Core 8 · Bootstrap 5
ONT3010 — 3rd Year Project · 2025
