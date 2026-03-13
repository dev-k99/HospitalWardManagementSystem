using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Validators
{
    /// <summary>
    /// FluentValidation validator for patient admission.
    /// Business rules that don't fit in DataAnnotations live here:
    ///   - Bed must exist and be unoccupied at the time of admission.
    ///   - Patient cannot be younger than 0 or older than 130 years.
    ///   - Admission date cannot be in the future.
    /// </summary>
    public sealed class AdmitPatientValidator : AbstractValidator<AdmitPatientViewModel>
    {
        private readonly WardSystemDBContext _db;

        public AdmitPatientValidator(WardSystemDBContext db)
        {
            _db = db;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50);

            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .Must(dob => dob <= DateTime.Today)
                .WithMessage("Date of birth cannot be in the future.")
                .Must(dob => DateTime.Today.Year - dob.Year <= 130)
                .WithMessage("Date of birth appears invalid (age over 130 years).");

            RuleFor(x => x.ContactNumber)
                .NotEmpty()
                .Matches(@"^[\+\d\s\-\(\)]{7,20}$")
                .WithMessage("Please enter a valid South African phone number.");

            RuleFor(x => x.AdmissionDate)
                .NotEmpty()
                .Must(date => date <= DateTime.Now.AddMinutes(5))
                .WithMessage("Admission date cannot be set in the future.");

            RuleFor(x => x.WardId)
                .GreaterThan(0).WithMessage("Please select a ward.")
                .MustAsync(WardExistsAsync).WithMessage("Selected ward does not exist or is inactive.");

            RuleFor(x => x.BedId)
                .GreaterThan(0).WithMessage("Please select a bed.")
                .MustAsync(BedIsAvailableAsync)
                .WithMessage("The selected bed is already occupied. Please choose another bed.");

            RuleFor(x => x.AssignedDoctorId)
                .GreaterThan(0).WithMessage("Please assign an attending doctor.")
                .MustAsync(DoctorExistsAsync).WithMessage("Selected doctor does not exist or is inactive.");
        }

        private async Task<bool> WardExistsAsync(int wardId, CancellationToken ct) =>
            await _db.Wards.AnyAsync(w => w.Id == wardId && w.IsActive, ct);

        private async Task<bool> BedIsAvailableAsync(int bedId, CancellationToken ct)
        {
            var bed = await _db.Beds.FindAsync(new object[] { bedId }, ct);
            return bed != null && bed.IsActive && !bed.PatientId.HasValue;
        }

        private async Task<bool> DoctorExistsAsync(int doctorId, CancellationToken ct) =>
            await _db.Staff.AnyAsync(s => s.Id == doctorId && s.Role == "Doctor" && s.IsActive, ct);
    }
}
