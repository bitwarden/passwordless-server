namespace Passwordless.AdminConsole.Controller;

public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Reporting");

        group.MapGet("/Example", async () =>
        {
            // We call our API instead using the api secret
            var result = new
            {
                Values = new[] { 30, 40, 35, 50, 49, 60, 70, 91, 125 },
                Categories = new[] { 1991, 1992, 1993, 1994, 1995, 1996, 1997, 1998, 1999 }
            };
            return result;
        }).RequireAuthorization();
        return endpoints;
    }
}