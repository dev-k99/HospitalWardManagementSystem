using FluentValidation;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Validators
{
    /// <summary>
    /// Clinical range validation for vital sign recordings.
    /// Ranges are based on standard clinical alert thresholds:
    ///   - Temperature: 20–45°C (allows hypothermia and fever edge cases)
    ///   - Pulse: 30–220 bpm (allows bradycardia and tachycardia)
    ///   - O₂ saturation: 50–100% (allows critical low readings to be recorded)
    /// </summary>
    public sealed class RecordVitalSignValidator : AbstractValidator<RecordVitalSignViewModel>
    {
        public RecordVitalSignValidator()
        {
            RuleFor(x => x.PatientId)
                .GreaterThan(0).WithMessage("Please select a patient.");

            RuleFor(x => x.Temperature)
                .InclusiveBetween(20.0, 45.0)
                .WithMessage("Temperature must be between 20°C and 45°C.");

            RuleFor(x => x.Pulse)
                .InclusiveBetween(30, 220)
                .WithMessage("Pulse must be between 30 and 220 bpm.");

            When(x => x.BloodPressure != null, () =>
            {
                RuleFor(x => x.BloodPressure)
                    .Matches(@"^\d{2,3}\/\d{2,3}$")
                    .WithMessage("Blood pressure must be in the format 120/80.");
            });

            When(x => x.HeartRate.HasValue, () =>
            {
                RuleFor(x => x.HeartRate!.Value)
                    .InclusiveBetween(30, 220)
                    .WithMessage("Heart rate must be between 30 and 220 bpm.");
            });

            When(x => x.RespiratoryRate.HasValue, () =>
            {
                RuleFor(x => x.RespiratoryRate!.Value)
                    .InclusiveBetween(4, 60)
                    .WithMessage("Respiratory rate must be between 4 and 60 breaths/min.");
            });

            When(x => x.OxygenSaturation.HasValue, () =>
            {
                RuleFor(x => x.OxygenSaturation!.Value)
                    .InclusiveBetween(50, 100)
                    .WithMessage("Oxygen saturation must be between 50% and 100%.");
            });

            RuleFor(x => x.Notes)
                .MaximumLength(500);
        }
    }
}
