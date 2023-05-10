namespace Passwordless.Api.Models;

public static class ListResponse
{
    public static ListResponse<T> Create<T>(IEnumerable<T> value)
    {
        return new ListResponse<T>
        {
            Values = value
        };
    }
}

public class ListResponse<T> : ResponseBase
{
    public IEnumerable<T> Values { get; set; }
}