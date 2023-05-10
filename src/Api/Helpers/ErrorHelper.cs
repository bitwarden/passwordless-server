using Passwordless.Service.Helpers;

namespace Passwordless.Api.Helpers;

public static class ErrorHelper
{
    private class HttpError
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }

        public HttpError(int errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }
    }
    public static IResult FromException(ApiException e)
    {
        return Results.Problem(e.Message, instance: null, statusCode: e.StatusCode);
    }
}