using FluentValidation;
using FluentValidation.Results;
using SPARTA_WAM.Models;
using SPARTA_WAM.Validators;

namespace SPARTA_WAM.Services;

/// <summary>
/// Validates input data for LAO Freeze/Block PA and Product Freeze operations.
/// </summary>
public interface ILaoFreezeInputValidationService
{
    Task<ValidationResult> ValidateLaoFreezeRowsAsync(List<LaoFreezeRow> rows);
    Task<ValidationResult> ValidateLaoProductFreezeRowsAsync(List<LaoProductFreezeRow> rows);
}

public class LaoFreezeInputValidationService : ILaoFreezeInputValidationService
{
    private readonly IValidator<LaoFreezeRequest> _freezeValidator;
    private readonly IValidator<LaoProductFreezeRequest> _productFreezeValidator;
    private readonly ILogger<LaoFreezeInputValidationService> _logger;

    public LaoFreezeInputValidationService(
        IValidator<LaoFreezeRequest> freezeValidator,
        IValidator<LaoProductFreezeRequest> productFreezeValidator,
        ILogger<LaoFreezeInputValidationService> logger)
    {
        _freezeValidator = freezeValidator;
        _productFreezeValidator = productFreezeValidator;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateLaoFreezeRowsAsync(List<LaoFreezeRow> rows)
    {
        var results = new List<ValidationFailure>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var request = new LaoFreezeRequest
            {
                ContractNumber = row.ContractNumber,
                BlockDate = row.BlockDate,
                ReleaseDate = row.ReleaseDate
            };

            var validationResult = await _freezeValidator.ValidateAsync(request);

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

        _logger.LogInformation($"Validated {rows.Count} LAO Freeze rows. Errors: {results.Count}");
        return new ValidationResult(results);
    }

    public async Task<ValidationResult> ValidateLaoProductFreezeRowsAsync(List<LaoProductFreezeRow> rows)
    {
        var results = new List<ValidationFailure>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var request = new LaoProductFreezeRequest
            {
                SalesOrganization = row.SalesOrganization,
                BlockDate = row.BlockDate,
                ReleaseDate = row.ReleaseDate,
                ShortCode = row.ShortCode
            };

            var validationResult = await _productFreezeValidator.ValidateAsync(request);

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

        _logger.LogInformation($"Validated {rows.Count} LAO Product Freeze rows. Errors: {results.Count}");
        return new ValidationResult(results);
    }
}