namespace Passwordless.Service.Helpers;

public class UnknownCredentialException : ApiException
{
    public UnknownCredentialException(string message) : base(message)
    {
    }

    public UnknownCredentialException(string message, int statusCode, Dictionary<string, object> extras = null) : base(message, statusCode, extras)
    {
    }

    public UnknownCredentialException(string errorCode, string message, int statusCode, Dictionary<string, object> extras = null) : base(errorCode, message, statusCode, extras)
    {
    }
}