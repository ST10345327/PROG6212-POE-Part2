using FluentValidation;
using CMCS.WebApp.Models;

namespace CMCS.WebApp.Models.Validators
{
    public class ClaimValidator : AbstractValidator<Claim>
    {
        public ClaimValidator()
        {
            RuleFor(x => x.HoursWorked)
                .GreaterThan(0).WithMessage("Hours worked must be greater than 0")
                .LessThanOrEqualTo(200).WithMessage("Hours worked cannot exceed 200 per month");

            RuleFor(x => x.Rate)
                .GreaterThan(0).WithMessage("Hourly rate must be greater than 0")
                .LessThanOrEqualTo(500).WithMessage("Hourly rate cannot exceed R500");

            RuleFor(x => x.Period)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Claim period cannot be in the future")
                .GreaterThanOrEqualTo(DateTime.Now.AddMonths(-3)).WithMessage("Claims older than 3 months are not accepted");

            RuleFor(x => x.SupportingFiles)
                .Must(files => files == null || files.Length <= 5)
                .WithMessage("Maximum 5 supporting documents allowed");
        }
    }
}