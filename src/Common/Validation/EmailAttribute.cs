using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Validation;

/// <summary>
/// An attribute that validates the syntax of an email address.
/// </summary>
/// <remarks>
/// An attribute that validates the syntax of an email address.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class EmailAttribute : ValidationAttribute
{
    /// <summary>
    /// Instantiates a new instance of <see cref="EmailAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="EmailAttribute"/>.
    /// </remarks>
    /// <param name="allowTopLevelDomains"><c>true</c> if the validator should allow addresses at top-level domains; otherwise, <c>false</c>.</param>
    /// <param name="allowInternational"><c>true</c> if the validator should allow international characters; otherwise, <c>false</c>.</param>
    public EmailAttribute(bool allowTopLevelDomains = false, bool allowInternational = false)
    {
        AllowTopLevelDomains = allowTopLevelDomains;
        AllowInternational = allowInternational;
    }

    /// <summary>
    /// Get or set whether or not the validator should allow top-level domains.
    /// </summary>
    /// <remarks>
    /// Gets or sets whether or not the validator should allow top-level domains.
    /// </remarks>
    /// <value><c>true</c> if top-level domains should be allowed; otherwise, <c>false</c>.</value>
    public bool AllowTopLevelDomains { get; set; }

    /// <summary>
    /// Get or set whether or not the validator should allow international characters.
    /// </summary>
    /// <remarks>
    /// Gets or sets whether or not the validator should allow international characters.
    /// </remarks>
    /// <value><c>true</c> if international characters should be allowed; otherwise, <c>false</c>.</value>
    public bool AllowInternational { get; set; }

    /// <summary>
    /// Validates the value.
    /// </summary>
    /// <remarks>
    /// Checks whether or not the email address provided is syntactically correct.
    /// </remarks>
    /// <returns>The validation result.</returns>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var memberNames = new[] { validationContext?.MemberName ?? nameof(value) };

        if (value == null || EmailValidator.Validate((string)value, AllowTopLevelDomains, AllowInternational))
            return ValidationResult.Success;

        return new ValidationResult("Email invalid", memberNames);
    }

    /// <summary>
    /// Validates the value.
    /// </summary>
    /// <remarks>
    /// Checks whether or not the email address provided is syntactically correct.
    /// </remarks>
    /// <returns><c>true</c> if the value is a valid email address; otherwise, <c>false</c>.</returns>
    /// <param name="value">The value to validate.</param>
    public override bool IsValid(object value)
    {
        return value == null || EmailValidator.Validate((string)value, AllowTopLevelDomains, AllowInternational);
    }
}