using System.ComponentModel.DataAnnotations;

namespace TinyURL.Ui.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var input = value as string;
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Success;

        if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri))
            return new ValidationResult(ErrorMessage ?? "URL must be a valid http:// or https:// URL.");

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return new ValidationResult(ErrorMessage ?? "URL must be a valid http:// or https:// URL.");

        return ValidationResult.Success;
    }
}

