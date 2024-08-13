namespace Passwordless.Common.Validation;

/// <summary>
/// An email validation error.
/// </summary>
/// <remarks>
/// Represents an email validation error, containing information about the type of error
/// and the index of the offending character.
/// </remarks>
public class EmailValidationError
{
    /// <summary>
    /// A special instance of <see cref="EmailValidationError"/> that indicates no error.
    /// </summary>
    /// <remarks>
    /// A special instance of <see cref="EmailValidationError"/> that indicates no error.
    /// </remarks>
    public static readonly EmailValidationError None = new(EmailValidationErrorCode.None);

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailValidationError"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="EmailValidationError"/>.
    /// </remarks>
    /// <param name="code">The error code.</param>
    /// <param name="tokenIndex">The character index indicating the starting position of the token that had the syntax error.</param>
    /// <param name="errorIndex">The character index indicating the position of the syntax error.</param>
    public EmailValidationError(EmailValidationErrorCode code, int? tokenIndex = null, int? errorIndex = null)
    {
        Code = code;
        TokenIndex = tokenIndex;
        ErrorIndex = errorIndex ?? tokenIndex;
    }

    /// <summary>
    /// Get the email validation error code.
    /// </summary>
    /// <remarks>
    /// Gets the email validation error code.
    /// </remarks>
    /// <value>The email validation error code.</value>
    public EmailValidationErrorCode Code
    {
        get;
        private set;
    }

    /// <summary>
    /// Get the character index indicating the starting position of the token that had the syntax error.
    /// </summary>
    /// <remarks>
    /// Gets the character index indicating the starting position of the token that had the syntax error.
    /// </remarks>
    /// <value>The character index indicating the starting position of the token that had the syntax error.</value>
    public int? TokenIndex
    {
        get;
        private set;
    }

    /// <summary>
    /// Get the character index indicating the position of the syntax error.
    /// </summary>
    /// <remarks>
    /// Gets the character index indicating the position of the syntax error.
    /// </remarks>
    /// <value>The character index indicating the position of the syntax error.</value>
    public int? ErrorIndex
    {
        get;
        private set;
    }
}