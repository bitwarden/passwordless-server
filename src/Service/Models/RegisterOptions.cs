namespace Passwordless.Service.Models;

public class SessionResponse<T>
{
    public T Data { get; set; }
    public string Session { get; set; }

    public SessionResponse()
    {

    }
    public SessionResponse(T data)
    {
        Data = data;
    }
}