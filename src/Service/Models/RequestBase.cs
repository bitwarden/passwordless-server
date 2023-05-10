namespace Passwordless.Service.Models;

public class RequestBase
{
    public string Origin { get; set; }
    public string RPID { get; set; }
    public string ServerName { get; set; }
}