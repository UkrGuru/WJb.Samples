using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using UkrGuru.Sql;
using WJb;
using WJb.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(opt => { opt.SingleLine = true; opt.TimestampFormat = "HH:mm:ss.fff "; });

        // Hide logs from this category
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        // UkrGuru.Sql setup: register SqlClient with connection string
        var conn = ctx.Configuration.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        services.AddSql(conn, true);

        // Register action
        services.AddTransient<SqlAction>();

        // Register WJb with actions map directly in DI (Dictionary<string, ActionItem>)
        var actions = new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
        {
            ["CalcOrderTotalAction"] = new ActionItem(
                type: typeof(SqlAction).AssemblyQualifiedName!,
                more: new
                {
                    tsql = """
                        SELECT SUM(quantity * price)
                        FROM OPENJSON(@Data, '$.items')
                        WITH (
                            quantity int    '$.quantity',
                            price    decimal(8,2) '$.price'
                        )
                        """
                }
            )
        };
        services.AddWJb(actions);
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

var order = new
{
    data = """
    {
      "orderId": "12345",
      "restaurant": "McDonald''s",
      "items": [
        { "name": "Big Mac",       "quantity": 1, "price": 5.99 },
        { "name": "Medium Fries",  "quantity": 1, "price": 2.49 },
        { "name": "Coca-Cola",     "size": "Medium", "quantity": 1, "price": 1.99 }
      ]
    }
    """
};
await jobs.EnqueueJobAsync(await jobs.CompactAsync("CalcOrderTotalAction", order));

await host.RunAsync();

public class SqlAction(IDbService db, ILogger<SqlAction> logger) : IAction
{
    private readonly IDbService _db = db;
    private readonly ILogger<SqlAction> _logger = logger;

    public async Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        // Read inputs
        var tsql = jobMore.GetString("tsql") ?? throw new InvalidOperationException("TSql is required.");
        var data = jobMore.GetString("data");

        var total = await _db.ExecAsync<object>(tsql, data);
        _logger.LogInformation("Order Total: {Result}", total);
    }
}
