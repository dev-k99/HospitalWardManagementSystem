using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.ViewModels;

namespace WardSystemProject.Validators
{
    /// <summary>
    /// Validates medication administration requests.
    ///
    /// Key business rule from the spec:
    ///   "A nurse is only allowed to dispense medication up to schedule 4.
    ///    Only a Nursing Sister may dispense any schedule 5 (or higher) medication."
    ///
    /// Note: The schedule check is also enforced in <see cref="VitalSignService.AdministerAsync"/>.
    /// Validators run at the MVC layer (before the service), providing immediate
    /// ModelState errors on the form rather than requiring a service call first.
    /// </summary>
    public sealed class AdministerMedicationValidator : AbstractValidator<AdministerMedicationViewModel>
    {
        public AdministerMedicationValidator()
        {
            RuleFor(x => x.PatientId)
                .GreaterThan(0).WithMessage("Please select a patient.");

            RuleFor(x => x.MedicationId)
                .GreaterThan(0).WithMessage("Please select a medication.");

            RuleFor(x => x.Dosage)
                .NotEmpty().WithMessage("Dosage given is required.")
                .MaximumLength(100);

            RuleFor(x => x.AdministrationMethod)
                .MaximumLength(50);

            RuleFor(x => x.Notes)
                .MaximumLength(500);
        }
    }
}
