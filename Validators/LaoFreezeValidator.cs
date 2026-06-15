using FluentValidation;
using SPARTA_WAM.Models;

namespace SPARTA_WAM.Validators;

/// <summary>
/// Validator for LAO Freeze/Block PA requests.
/// </summary>
public class LaoFreezeValidator : AbstractValidator<LaoFreezeRequest>
{
    public LaoFreezeValidator()
    {
        RuleFor(x => x.ContractNumber)
            .NotEmpty().WithMessage("Contract Number is mandatory")
            .Length(1, 50).WithMessage("Contract Number must be between 1 and 50 characters")
            .Matches(@"^\d+$").WithMessage("Contract Number must contain only digits");

        RuleFor(x => x.BlockDate)
            .NotEmpty().WithMessage("Block Date is mandatory")
            .LessThan(x => x.ReleaseDate).WithMessage("Block Date must be before Release Date");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty().WithMessage("Release Date is mandatory")
            .GreaterThan(DateTime.MinValue).WithMessage("Release Date must be a valid date");
    }
}

/// <summary>
/// Validator for LAO Product Freeze requests.
/// </summary>
public class LaoProductFreezeValidator : AbstractValidator<LaoProductFreezeRequest>
{
    public LaoProductFreezeValidator()
    {
        RuleFor(x => x.SalesOrganization)
            .NotEmpty().WithMessage("Sales Organization is mandatory")
            .Length(1, 50).WithMessage("Sales Organization must be between 1 and 50 characters");

        RuleFor(x => x.BlockDate)
            .NotEmpty().WithMessage("Block Date is mandatory")
            .LessThan(x => x.ReleaseDate).WithMessage("Block Date must be before Release Date");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty().WithMessage("Release Date is mandatory")
            .GreaterThan(DateTime.MinValue).WithMessage("Release Date must be a valid date");

        RuleFor(x => x.ShortCode)
            .NotEmpty().WithMessage("Short Code is mandatory")
            .Length(1, 50).WithMessage("Short Code must be between 1 and 50 characters");
    }
}