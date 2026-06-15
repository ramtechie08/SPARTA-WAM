using FluentValidation;
using SPARTA_WAM.Models;

namespace SPARTA_WAM.Validators;

public class WalkAwayMarginValidator : AbstractValidator<WalkAwayMarginRequest>
{
    public WalkAwayMarginValidator()
    {
        RuleFor(x => x.ShortCode)
            .NotEmpty().WithMessage("Short Code is mandatory")
            .Length(1, 20).WithMessage("Short Code must be between 1 and 20 characters");

        RuleFor(x => x.WalkAwayMargin)
            .GreaterThanOrEqualTo(0).WithMessage("Walk Away Margin must be greater than or equal to 0")
            .LessThanOrEqualTo(100).WithMessage("Walk Away Margin must be less than or equal to 100");

        RuleFor(x => x.SalesOrganization)
            .NotEmpty().WithMessage("Sales Organization is mandatory")
            .Length(1, 50).WithMessage("Sales Organization must be between 1 and 50 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is mandatory")
            .Must(x => x.Equals("D", StringComparison.OrdinalIgnoreCase) || x.Equals("I", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Type must be either 'D' (Direct) or 'I' (Indirect)");
    }
}

