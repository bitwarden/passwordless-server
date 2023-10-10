namespace Passwordless.AdminConsole.Db;

public static class AddDatabaseExtensionMethod
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, WebApplicationBuilder builder)
    {
        // if not present, try use sqlite
        var sqlite = builder.Configuration.GetConnectionString("sqlite:admin");
        var mssql = builder.Configuration.GetConnectionString("mssql:admin");

        // read "migrate_db" from env
        var migrating = builder.Configuration.GetValue<string>("ef_migrate");
        if (migrating == "1")
        {
            services.AddDbContext<MssqlConsoleDbContext>();
            services.AddDbContext<SqliteConsoleDbContext>();
        }

        // if name starts with sqlite, use sqlite, else use mssql
        if (!String.IsNullOrEmpty(sqlite))
        {
            //Console.WriteLine("Using sqlite");
            services.AddDbContext<ConsoleDbContext, SqliteConsoleDbContext>();
        }
        else if (!string.IsNullOrEmpty(mssql))
        {
            //Console.WriteLine("Using mssql");
            services.AddDbContext<ConsoleDbContext, MssqlConsoleDbContext>();
        }
        else
        {
            throw new Exception("Unknown database type");
        }

        return services;
    }
}