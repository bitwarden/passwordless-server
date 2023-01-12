using Service.Helpers;

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
        return Results.Json(new HttpError(e.ErrorCode, e.Message), Json.Options, statusCode: e.ErrorCode);
    }
}

