using System.Net;

namespace Passwordless.Fido2.MetadataService.Exceptions;

public class Fido2Exception : Exception
{
    public Fido2Exception(string message, HttpStatusCode? statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }
}