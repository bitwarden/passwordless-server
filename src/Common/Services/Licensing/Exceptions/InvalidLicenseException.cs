namespace Passwordless.Common.Services.Licensing.Exceptions;

public class InvalidLicenseException : Exception
{
    public InvalidLicenseException(
        string license,
        string message = "The license is invalid.") : base(message)
    {
        License = license;
    }
    
    public string License { get; }
}