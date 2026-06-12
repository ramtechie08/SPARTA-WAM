using FluentValidation;
using FluentValidation.Results;
using SPARTA_WAM.Models;
using SPARTA_WAM.Validators;

namespace SPARTA_WAM.Services;

public interface IInputValidationService
{
    Task<ValidationResult> ValidateWamRowsAsync(List<WalkAwayMarginRow> rows);
}

public class InputValidationService : IInputValidationService
{
    private readonly IValidator<WalkAwayMarginRequest> _validator;

    public InputValidationService(IValidator<WalkAwayMarginRequest> validator)
    {
        _validator = validator;
    }

    public async Task<ValidationResult> ValidateWamRowsAsync(List<WalkAwayMarginRow> rows)
    {
        var results = new List<ValidationFailure>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var request = new WalkAwayMarginRequest
            {
                ShortCode = row.CustomerShortCode,
                WalkAwayMargin = row.MarginValue,
                SalesOrganization = row.SalesOrg,
                Type = row.PricingType
            };

            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                foreach (var failure in validationResult.Errors)
                {
                    results.Add(new ValidationFailure(
                        $"Row {i + 2}: {failure.PropertyName}",
                        failure.ErrorMessage));
                }
            }
        }

        return new ValidationResult(results);
    }
}