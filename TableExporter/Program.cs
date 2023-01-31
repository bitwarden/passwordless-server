// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Service.Storage;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

IConfiguration Configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: false)
    .Build();

async Task GetAllTenants()
{
    var storage = new TableStorage("table-exporter", Configuration);
    var client = storage.GetTableClient();

    var tables = client.ListTables();

    Console.WriteLine($"Count: {tables.Count()}");

    var summary = new ConcurrentDictionary<string, int>();
    var summaryDetails = new ConcurrentDictionary<string, Details>();

    // use tab separator
    var separator = ",";
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm");


    // make parallel
    //,new ParallelOptions() { MaxDegreeOfParallelism = 1}
    Parallel.ForEach(tables, (table) =>  {
        var entities = table.ExecuteQuery(new TableQuery<DynamicTableEntity>()).ToList();

        Console.WriteLine($"{table.Name}: {entities.Count}");
        summary.TryAdd(table.Name, entities.Count);

        // create csv file
        var csv = new StringBuilder();

        var allKeys = new List<string>();

        foreach (var entity in entities)
        {
            // add all properties to dictionary

            allKeys.Add("PartitionKey");
            allKeys.Add("RowKey");
            allKeys.Add("Timestamp");
            //allKeys.Add("Etag");

            foreach (var kvp in entity.Properties)
            {
                allKeys.Add(kvp.Key);
                allKeys.Add($"{kvp.Key}@type");
            }
        }

        allKeys = allKeys.Distinct().ToList();

        var header = string.Join(separator, allKeys);
        csv.AppendLine(header);

        foreach (var entity in entities)
        {

            var row = new List<string>();
            row.Add(entity.PartitionKey);
            row.Add(entity.RowKey);
            row.Add(entity.Timestamp.UtcDateTime.ToString("o"));
            //row.Add(entity.ETag);

            foreach (var key in allKeys.Except(new[] { "PartitionKey", "RowKey", "Timestamp" }).Where(k => !k.Contains("@")))          
            {
                if (entity.Properties.ContainsKey(key))
                {
                    var value = entity.Properties[key].ToString();

                    // if type is datetime
                    if (entity.Properties[key].PropertyType == EdmType.DateTime)
                    {
                        value = entity.Properties[key].DateTimeOffsetValue.Value.UtcDateTime.ToString("o");
                    }

                    // escape quoutes
                    value = value.Replace("\"", "\"\"");
                    // escape for csv
                    //value = value.Replace(",", "\\,");

                    // if contains comma, add quotes
                    if (value.Contains(",") || value.Contains("\""))
                    {
                        value = $"\"{value}\"";
                    }
                    
                    row.Add(value);
                    row.Add(entity.Properties[key].PropertyType.ToString());
                }
                else
                {
                    row.Add("");
                    row.Add("");
                }
            }
            
            var line = string.Join(separator, row);
            csv.AppendLine(line);

            if(entity.PartitionKey == "M-ACCOUNTINFO")
            {
                summaryDetails.TryAdd(table.Name, new Details()
                {
                    TenantName = table.Name,
                    Email = entity.Properties["AdminEmailsSerialized"].StringValue,
                    Count = entities.Count
                });
            }
        }

        // save csv to local file
        
        var fileName = $"{timestamp}/{table.Name}.csv";
        Directory.CreateDirectory(timestamp);
        
        File.WriteAllText(fileName, csv.ToString());
    });

    // write summary to csv
    var summaryCsv = new StringBuilder();
    summaryCsv.AppendLine("Tenant,Count");
    ;
    foreach (var item in summary.OrderByDescending(x => x.Value))
    {
        summaryCsv.AppendLine($"{item.Key},{item.Value}");
    }

    var safedate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
    var filename = $"{timestamp}/summary{safedate}.csv";

    File.WriteAllText(filename, summaryCsv.ToString());


    // write summaryDetails to csv
    var summaryDetailsCsv = new StringBuilder();
    summaryDetailsCsv.AppendLine("Tenant,Count,Email");
    ;
    foreach (var item in summaryDetails.OrderByDescending(x => x.Value.Count))
    {
        var email = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(item.Value.Email);
        summaryDetailsCsv.AppendLine($"{item.Key},{item.Value.Count},{email?.FirstOrDefault()}");
    }

    
    filename = $"{timestamp}/summaryDetails{safedate}.csv";

    File.WriteAllText(filename, summaryDetailsCsv.ToString());


    Console.WriteLine($"Count: {tables.Count()}");

    await Task.Delay(0);
}


await GetAllTenants();

class Details
{
    public string TenantName { get; set; }
    public string Email { get; set; }
    public int Count { get; set; }
}