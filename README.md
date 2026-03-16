# WardCare+

### A full-stack hospital ward management system for South African healthcare — built by a team of 4, deployed live on Azure.

[![Live Demo](https://img.shields.io/badge/Live%20Demo-wardcareplus.azurewebsites.net-0078D4?style=for-the-badge&logo=microsoftazure)](https://wardcareplus.azurewebsites.net/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/mvc/)
[![EF Core 8](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://learn.microsoft.com/en-us/ef/core/)
[![Azure](https://img.shields.io/badge/Deployed%20on-Azure-0078D4?style=for-the-badge&logo=microsoftazure)](https://azure.microsoft.com/)

---

## Live Demo

> **Try it now:** [https://wardcareplus.azurewebsites.net/](https://wardcareplus.azurewebsites.net/)

Log in as any role to explore the system. No sign-up required.

| Role | Username | Password | What you can explore |
|------|----------|----------|----------------------|
| Doctor | `doctor` | `VtrCor87!` | Patient visits, prescriptions, clinical instructions, patient folders |
| Nurse | `nurse` | `VtrCor87!` | Vital signs, medication administration (Sch 1–4), doctor instructions |
| Nursing Sister | `sister` | `VtrCor87!` | Everything a Nurse can do, plus Schedule 5+ medications |
| Script Manager | `scripts` | `VtrCor87!` | Prescription queue, pharmacy dispatch, delivery tracking |
| Consumables Manager | `consumables` | `VtrCor87!` | Stock levels, low-stock alerts, consumable orders, weekly stock takes |
| Ward Admin | `wardadmin` | `VtrCor87!` | Patient admissions, transfers, discharges, PDF patient folders |

---

## About the Project

WardCare+ is a production-quality ward management system built to the ONT3010 academic specification — a realistic South African hospital IT brief covering the full lifecycle of a hospital admission: from a Ward Admin checking a patient in, through nursing care and doctor visits, to pharmacy, stock management, and discharge.

The system is designed with **POPIA compliance** (South Africa's Protection of Personal Information Act) in mind: every data change is logged, records are soft-deleted rather than destroyed, and access is strictly scoped by role. Built as a final-year group project by a team of 4 — individual contributions are in the [Team & Contributions](#team--contributions) section below.

---

## Features by Role

<details>
<summary><strong>Administrator</strong> — full system control</summary>

- Manage wards, rooms, and bed assignments
- Staff management (create, edit, soft-delete)
- Medication and allergy/condition reference catalogues
- Role-aware dashboard: ward occupancy, bed availability, low-stock alerts
- Audit trail access for POPIA compliance

</details>

<details>
<summary><strong>Ward Admin</strong> — patient lifecycle</summary>

- Admit patients (validates bed availability and doctor assignment)
- Edit patient demographics and clinical history
- Record and view patient ward movements and transfers
- Initiate and confirm discharges
- Download full patient folder as PDF
- Dashboard: recent admissions, movement history

</details>

<details>
<summary><strong>Doctor</strong> — clinical management</summary>

- View assigned patients and their full clinical folders
- Record consultation visits with notes and next-visit scheduling
- Write prescriptions (queued for Script Manager)
- Issue nursing instructions (priority-flagged)
- Initiate patient discharge with clinical summary
- Dashboard: today's visits, pending instructions, active prescriptions

</details>

<details>
<summary><strong>Nurse</strong> — bedside care</summary>

- Record vital signs (temperature, pulse, BP, O₂ sat, respiratory rate)
- Administer medications — restricted to Schedule 1–4 by business rule
- View and acknowledge doctor instructions
- Ward-scoped: only sees patients in their assigned ward
- Dashboard: today's vitals, pending administrations, unread instructions

</details>

<details>
<summary><strong>Nursing Sister</strong> — senior nursing</summary>

- All Nurse capabilities
- Can administer Schedule 5+ (controlled) medications
- Dashboard includes Schedule 5 administration activity

</details>

<details>
<summary><strong>Script Manager</strong> — pharmacy workflow</summary>

- View incoming prescription queue (bell badge for unprocessed count)
- Process prescriptions: add notes, mark sent to pharmacy
- Mark medications as received in ward
- View full prescription order history
- Dashboard: pending prescriptions, orders in transit

</details>

<details>
<summary><strong>Consumables Manager</strong> — inventory</summary>

- View all ward consumables with current stock and reorder levels
- Low-stock alerts on dashboard
- Create and track consumable orders
- Receive stock (updates quantity on hand)
- Conduct and record weekly stock takes with variance tracking
- Stock take history and detail reports

</details>

---

## Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                         WardCare+ MVC App                        │
│                                                                   │
│  Controllers (thin)           Features (business logic)           │
│  ┌───────────────────┐        ┌───────────────────────────────┐  │
│  │ PatientManagement │───────►│ PatientService                │  │
│  │ PatientCare       │───────►│ VitalSignService              │  │
│  │ DoctorPatient     │        │ PrescriptionService           │  │
│  │ ConsumableScript  │        │ PatientFolderPdfService       │  │
│  │ Administration    │        └───────────────────────────────┘  │
│  └───────────────────┘                     │                      │
│                                            ▼                      │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │              WardSystemDBContext (EF Core 8)              │   │
│  │   + AuditInterceptor (SaveChangesInterceptor)             │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │                   ASP.NET Core Identity                   │   │
│  │   7 roles · Policy-based authorization · Cookie auth      │   │
│  └───────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
                               │
              ┌────────────────┼────────────────┐
              ▼                ▼                ▼
         Azure SQL        Azure App          GitHub
         Database         Service            Actions
                              ▲                │
                              └──── deploy ────┘
                              (push to main →
                               build → publish
                               → deploy)
```

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 (Code-First migrations) |
| Authentication | ASP.NET Core Identity · Cookie auth · 7 roles |
| Database | Azure SQL Database (prod) · SQL Server Express (dev) |
| PDF generation | QuestPDF 2024 (in-memory, no external process) |
| Input validation | FluentValidation 11 (auto-wired to model binding) |
| Frontend | Bootstrap 5.3 · custom CSS (layout and colour scheme) |
| CI/CD | GitHub Actions → Azure Web Apps (OIDC, no stored secrets) |
| Testing | xUnit 2.6 · Moq 4.20 · EF Core InMemory |

---

## Key Technical Highlights

- **EF Core Audit Interceptor** — a `SaveChangesInterceptor` captures every INSERT, UPDATE, and DELETE as before/after JSON snapshots stored in `AuditLog`. No data change goes unrecorded, as required for POPIA compliance.

- **Medication schedule enforcement** — medications carry a Schedule field (1–6). The rule "Nurses administer Sch ≤4 only; Nursing Sisters handle Sch 5+" is enforced at two layers: FluentValidation (fast-fail on form submit) and `VitalSignService.AdministerAsync()` (throws `UnauthorizedAccessException` at the service level).

- **Service layer behind interfaces** — all patient lifecycle logic lives in `IPatientService`/`PatientService` and `IVitalSignService`/`VitalSignService`. Controllers depend on the interfaces, staying thin and independently testable.

- **In-memory PDF generation** — `PatientFolderPdfService` uses QuestPDF's fluent API to render full admission folders and discharge summaries as `byte[]` in memory — no temp files, no external subprocess, no filesystem dependency in the cloud.

- **Soft-delete everywhere** — every entity has an `IsActive` flag. Deleting a staff member, patient, or bed sets `IsActive = false`; the record stays in the database for audit and recovery.

- **Ward-scoped access for nurses** — the `Staff` table stores a `WardId` per nursing staff member. `PatientCareController` resolves the nurse's ward via `IdentityUserId` and filters all queries to that ward only.

- **Role-specific dashboards** — `HomeController.Index()` has 7 distinct branches, each surfacing the KPIs relevant to that role (e.g. Script Manager sees pending prescription count with a bell badge; Consumables Manager sees items below reorder level; Doctor sees today's visit schedule).

---

## CI/CD Pipeline

Deployments are fully automated via GitHub Actions with zero stored secrets.

```
Push to main
    │
    ▼
[Build — windows-latest]
  dotnet build (Release) → dotnet publish → upload artifact
    │
    ▼
[Deploy — windows-latest]
  Azure login via OIDC workload identity federation (no passwords in GitHub)
  Deploy artifact → Azure Web App: wardcareplus (Production slot)
    │
    ▼
App startup
  EF Core applies pending migrations automatically
  Roles and demo accounts seeded if absent (idempotent)
```

The connection string lives in Azure App Service Configuration and overrides `appsettings.json` at runtime — no credentials are committed to the repository.

---

## Screenshots

_(Add screenshots here)_

---

## Local Development Setup

**Prerequisites:** .NET 8 SDK · SQL Server (Express or full)

```bash
# 1. Clone
git clone https://github.com/dev-k99/HospitalWardManagementSystem.git
cd HospitalWardManagementSystem/WardSystemProject

# 2. Restore dependencies
dotnet restore

# 3. Configure local settings — create appsettings.Development.json:
{
  "ConnectionStrings": {
    "WardConn": "Server=YOUR_SERVER;Database=WardSysemDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "SeedAdmin": {
    "Username": "admin",
    "Email": "admin@wardcare.co.za",
    "Password": "YourAdminPassword123!"
  }
}

# 4. Apply migrations
dotnet ef database update

# 5. Run — migrations and demo seed run automatically on first launch
dotnet run
```

Navigate to `https://localhost:PORT` — the login page loads first. The admin account and demo role accounts are created on startup.

To populate the database with realistic demo patients, wards, and clinical data, run `seed_demo_data.sql` against your SQL Server instance using SQL Server Management Studio or Azure Data Studio.

---

## Team & Contributions

Developed as a final-year group project by a team of 4.

| Contributor | Area of Ownership |
|-------------|-------------------|
| [Kwanele](https://github.com/dev-k99) | Azure cloud deployment · GitHub Actions CI/CD pipeline · Azure SQL configuration · UI layout and colour scheme · Doctor- Patient Subsytem |
| _Kamva_ | _[Patient_Management_SubSystem]_ |
| _Karabo_ | _[Consumables_SubSystem]_ |
| _Marcus_ | _[PatientCare_SubSystem]_ |

---

## Data Model Overview

25+ tables covering the full ward workflow:

```
Ward → Room → Bed ↔ Patient  (bidirectional bed assignment)

Patient → VitalSign
        → MedicationAdministration
        → DoctorInstruction
        → DoctorVisit
        → Prescription → PrescriptionOrder  (pharmacy workflow)
        → PatientMovement
        → Allergy · MedicalCondition · PatientFolder

Consumable → ConsumableOrder
           → StockTake → StockTakeDetail

Staff ↔ AspNetUsers  (via IdentityUserId — links Identity auth to clinical staff)

AuditLog  (append-only, every table mutation recorded)
```

---

## License

Academic portfolio project. Not licensed for production medical use.
