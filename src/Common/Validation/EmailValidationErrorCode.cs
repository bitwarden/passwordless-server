namespace Passwordless.Common.Validation;

/// <summary>
/// An enumeration of possible email address validation error codes.
/// </summary>
/// <remarks>
/// An enumeration of possible email address validation error codes.
/// </remarks>
public enum EmailValidationErrorCode
{
    /// <summary>
    /// No error.
    /// </summary>
    None,

    /// <summary>
    /// The email address is empty.
    /// </summary>
    EmptyAddress,

    /// <summary>
    /// The email address exceeds the maximum length of 254 characters.
    /// </summary>
    AddressTooLong,

    /// <summary>
    /// The local-part of the email address contains an unterminated quoted-string.
    /// </summary>
    UnterminatedQuotedString,

    /// <summary>
    /// The local-part of the email address contains an invalid character.
    /// </summary>
    InvalidLocalPartCharacter,

    /// <summary>
    /// The local-part of the email address is incomplete.
    /// </summary>
    IncompleteLocalPart,

    /// <summary>
    /// The local-part of the email address exceeds the maximum length of 64 characters.
    /// </summary>
    LocalPartTooLong,

    /// <summary>
    /// The domain of the email address exceeds the maximum length of 253 characters.
    /// </summary>
    DomainTooLong,

    /// <summary>
    /// A domain label (subdomain) of the email address exceeds the maximum length of 63 characters.
    /// </summary>
    DomainLabelTooLong,

    /// <summary>
    /// The domain of the email address contains an invalid character.
    /// </summary>
    InvalidDomainCharacter,

    /// <summary>
    /// The domain of the email address is incomplete.
    /// </summary>
    IncompleteDomain,

    /// <summary>
    /// A domain label (subdomain) of the email address is incomplete.
    /// </summary>
    IncompleteDomainLabel,

    /// <summary>
    /// The IP address literal is incomplete.
    /// </summary>
    InvalidIPAddress,

    /// <summary>
    /// The IP address literal of the email address is missing the closing ']'.
    /// </summary>
    UnterminatedIPAddressLiteral,

    /// <summary>
    /// The email address contains unexpected characters after the end of the domain.
    /// </summary>
    UnexpectedCharactersAfterDomain,
}