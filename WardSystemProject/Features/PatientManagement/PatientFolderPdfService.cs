using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WardSystemProject.Models;

namespace WardSystemProject.Features.PatientManagement
{
    /// <summary>
    /// Generates a patient folder PDF using QuestPDF (MIT licence).
    /// Call <see cref="Generate"/> from the controller; returns raw bytes
    /// to be served as application/pdf.
    ///
    /// QuestPDF fluent API: Containers → Rows → Columns → Text / Tables.
    /// No external processes (wkhtmltopdf) or file-system dependencies needed.
    /// </summary>
    public sealed class PatientFolderPdfService
    {
        /// <summary>
        /// Generates the patient admission folder PDF.
        /// Requires a fully-loaded <see cref="Patient"/> with all Includes.
        /// </summary>
        public byte[] Generate(Patient patient)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);

                    page.Content().Element(c => ComposeContent(c, patient));

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("WardCare+ Management System  |  Generated: ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span("  |  Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.TotalPages().FontSize(8);
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>Generates a compact discharge summary PDF.</summary>
        public byte[] GenerateDischargeSummary(Patient patient)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeDischargeSummary(c, patient));

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("DISCHARGE SUMMARY  |  ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(8);
                    });
                });
            }).GeneratePdf();
        }

        // ── Header ────────────────────────────────────────────────────────────

        private static void ComposeHeader(IContainer c)
        {
            c.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("WardCare+ Management System")
                        .Bold().FontSize(16).FontColor(Colors.Blue.Darken3);
                    col.Item().Text("Patient Admission Folder")
                        .FontSize(12).FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(120).AlignRight().Text(DateTime.Now.ToString("dd MMM yyyy"))
                    .FontSize(9).FontColor(Colors.Grey.Medium);
            });

            c.PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Medium);
        }

        // ── Patient Folder Content ────────────────────────────────────────────

        private static void ComposeContent(IContainer c, Patient patient)
        {
            c.Column(col =>
            {
                col.Spacing(10);

                // ── Patient Demographics ──────────────────────────────────
                col.Item().Element(x => SectionHeader(x, "Patient Information"));
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(2);
                    });

                    void Row(string label, string? value, string label2, string? value2)
                    {
                        table.Cell().LabelCell(label);
                        table.Cell().ValueCell(value);
                        table.Cell().LabelCell(label2);
                        table.Cell().ValueCell(value2);
                    }

                    Row("Full Name:",    patient.FullName,               "Patient ID:", patient.Id.ToString());
                    Row("DOB:",          patient.DateOfBirth.ToString("dd MMM yyyy"),
                        "Age:",          patient.Age.ToString());
                    Row("Gender:",       patient.Gender,                 "Blood Type:", patient.BloodType ?? "—");
                    Row("Contact:",      patient.ContactNumber,          "Ward:",       patient.Ward?.Name ?? "—");
                    Row("Address:",      patient.Address,                "Bed:",        patient.Bed?.BedNumber ?? "—");
                    Row("Emergency:",    patient.EmergencyContact,       "Emerg. No.:", patient.EmergencyContactNumber);
                    Row("Next of Kin:",  patient.NextOfKin,              "NOK Contact:", patient.NextOfKinContact);
                    Row("Attending Dr:", patient.AssignedDoctor?.FullName ?? "—",
                        "Status:",       patient.PatientStatus);
                    Row("Admitted:",     patient.AdmissionDate?.ToString("dd MMM yyyy HH:mm") ?? "—",
                        "Discharged:",   patient.DischargeDate?.ToString("dd MMM yyyy HH:mm") ?? "—");
                });

                // ── Admission Reason ──────────────────────────────────────
                if (!string.IsNullOrWhiteSpace(patient.AdmissionReason))
                {
                    col.Item().Element(x => SectionHeader(x, "Admission Reason"));
                    col.Item().Padding(6).Background(Colors.Grey.Lighten3)
                        .Text(patient.AdmissionReason).FontSize(10);
                }

                // ── Allergies ─────────────────────────────────────────────
                var allergies = patient.PatientAllergies?.Where(a => a.IsActive).ToList() ?? [];
                col.Item().Element(x => SectionHeader(x, "Known Allergies"));
                if (allergies.Count == 0)
                {
                    col.Item().Text("No allergies recorded.").Italic().FontColor(Colors.Grey.Medium);
                }
                else
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols => { cols.RelativeColumn(); });
                        foreach (var a in allergies)
                            table.Cell().Padding(3).Text($"• {a.AllergyName}");
                    });
                }

                // ── Medical Conditions ────────────────────────────────────
                var conditions = patient.MedicalConditions?.Where(c => c.IsActive).ToList() ?? [];
                col.Item().Element(x => SectionHeader(x, "Medical Conditions / Chronic Illnesses"));
                if (conditions.Count == 0)
                {
                    col.Item().Text("No chronic conditions recorded.").Italic().FontColor(Colors.Grey.Medium);
                }
                else
                {
                    foreach (var cond in conditions)
                        col.Item().Text($"• {cond.ConditionName}").FontSize(10);
                }

                // ── Vital Signs (last 5) ──────────────────────────────────
                var vitals = (patient.VitalSigns ?? [])
                    .Where(v => v.IsActive)
                    .OrderByDescending(v => v.RecordDate)
                    .Take(5)
                    .ToList();

                col.Item().Element(x => SectionHeader(x, "Recent Vital Signs"));
                if (vitals.Count == 0)
                {
                    col.Item().Text("No vital signs recorded.").Italic().FontColor(Colors.Grey.Medium);
                }
                else
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(90);
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.ConstantColumn(80);
                        });

                        foreach (var h in new[] { "Date", "Temp °C", "Pulse", "BP", "O₂ Sat", "Resp Rate", "By" })
                            table.Cell().HeaderCell(h);

                        foreach (var v in vitals)
                        {
                            table.Cell().DataCell(v.RecordDate.ToString("dd/MM/yy HH:mm"));
                            table.Cell().DataCell(v.Temperature.ToString("F1"));
                            table.Cell().DataCell(v.Pulse.ToString());
                            table.Cell().DataCell(v.BloodPressure ?? "—");
                            table.Cell().DataCell(v.OxygenSaturation?.ToString() ?? "—");
                            table.Cell().DataCell(v.RespiratoryRate?.ToString() ?? "—");
                            table.Cell().DataCell(v.RecordedBy ?? "—");
                        }
                    });
                }

                // ── Medications / Prescriptions ───────────────────────────
                var prescriptions = (patient.Prescriptions ?? [])
                    .Where(p => p.IsActive)
                    .ToList();

                col.Item().Element(x => SectionHeader(x, "Active Prescriptions"));
                if (prescriptions.Count == 0)
                {
                    col.Item().Text("No active prescriptions.").Italic().FontColor(Colors.Grey.Medium);
                }
                else
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.ConstantColumn(75);
                        });

                        foreach (var h in new[] { "Medication", "Schedule", "Dosage", "Duration", "Prescribed By" })
                            table.Cell().HeaderCell(h);

                        foreach (var p in prescriptions)
                        {
                            table.Cell().DataCell(p.Medication?.Name ?? "—");
                            table.Cell().DataCell($"Sch {p.Medication?.Schedule}");
                            table.Cell().DataCell(p.DosageInstructions);
                            table.Cell().DataCell(p.Duration);
                            table.Cell().DataCell(p.Doctor?.FullName ?? "—");
                        }
                    });
                }

                // ── Chronic Medications / History (free text) ─────────────
                if (!string.IsNullOrWhiteSpace(patient.ChronicMedications))
                {
                    col.Item().Element(x => SectionHeader(x, "Chronic Medications (on admission)"));
                    col.Item().Padding(6).Background(Colors.Grey.Lighten3)
                        .Text(patient.ChronicMedications).FontSize(10);
                }

                if (!string.IsNullOrWhiteSpace(patient.MedicalHistory))
                {
                    col.Item().Element(x => SectionHeader(x, "Medical History"));
                    col.Item().Padding(6).Background(Colors.Grey.Lighten3)
                        .Text(patient.MedicalHistory).FontSize(10);
                }

                // ── Signature Block ───────────────────────────────────────
                col.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c2 =>
                    {
                        c2.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        c2.Item().Text("Authorising Clinician Signature").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    row.ConstantItem(40);
                    row.RelativeItem().Column(c2 =>
                    {
                        c2.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        c2.Item().Text("Date").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }

        // ── Discharge Summary ─────────────────────────────────────────────────

        private static void ComposeDischargeSummary(IContainer c, Patient patient)
        {
            c.Column(col =>
            {
                col.Spacing(10);

                col.Item().Text("DISCHARGE SUMMARY")
                    .Bold().FontSize(14).FontColor(Colors.Red.Darken3);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1); cols.RelativeColumn(2);
                        cols.RelativeColumn(1); cols.RelativeColumn(2);
                    });

                    void Row(string l, string? v, string l2, string? v2)
                    {
                        table.Cell().LabelCell(l);
                        table.Cell().ValueCell(v);
                        table.Cell().LabelCell(l2);
                        table.Cell().ValueCell(v2);
                    }

                    Row("Patient:",     patient.FullName, "Patient ID:", patient.Id.ToString());
                    Row("DOB:",         patient.DateOfBirth.ToString("dd MMM yyyy"), "Age:", patient.Age.ToString());
                    Row("Admitted:",    patient.AdmissionDate?.ToString("dd MMM yyyy") ?? "—",
                        "Discharged:",  patient.DischargeDate?.ToString("dd MMM yyyy") ?? "—");
                    Row("Ward:",        patient.Ward?.Name ?? "—",
                        "Attending Dr:", patient.AssignedDoctor?.FullName ?? "—");
                });

                col.Item().Element(x => SectionHeader(x, "Discharge Summary / Clinical Notes"));
                col.Item().Padding(8).Background(Colors.Grey.Lighten3)
                    .Text(patient.DischargeSummary ?? "No summary provided.").FontSize(10);

                col.Item().PaddingTop(30).Row(row =>
                {
                    row.RelativeItem().Column(c2 =>
                    {
                        c2.Item().PaddingTop(30).LineHorizontal(1);
                        c2.Item().Text("Discharging Doctor Signature").FontSize(9);
                    });
                    row.ConstantItem(30);
                    row.RelativeItem().Column(c2 =>
                    {
                        c2.Item().PaddingTop(30).LineHorizontal(1);
                        c2.Item().Text("Date").FontSize(9);
                    });
                });
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void SectionHeader(IContainer c, string title)
        {
            c.PaddingTop(4)
             .Background(Colors.Blue.Lighten4)
             .Padding(4)
             .Text(title).Bold().FontSize(10).FontColor(Colors.Blue.Darken4);
        }
    }

    // ── QuestPDF extension helpers ─────────────────────────────────────────────

    internal static class PdfCellExtensions
    {
        internal static void LabelCell(this IContainer c, string text)
        {
            c.Background(Colors.Grey.Lighten3)
             .Padding(4)
             .Text(text).Bold().FontSize(9).FontColor(Colors.Grey.Darken2);
        }

        internal static void ValueCell(this IContainer c, string? text)
        {
            c.Padding(4)
             .Text(text ?? "—").FontSize(9);
        }

        internal static void HeaderCell(this IContainer c, string text)
        {
            c.Background(Colors.Blue.Darken3)
             .Padding(4)
             .Text(text).Bold().FontSize(9).FontColor(Colors.White);
        }

        internal static void DataCell(this IContainer c, string? text)
        {
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
             .Padding(3)
             .Text(text ?? "—").FontSize(9);
        }
    }
}
