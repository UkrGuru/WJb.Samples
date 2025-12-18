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
        // Optional: replace default providers to have full control
        logging.ClearProviders();

        logging.AddSimpleConsole(opt =>
        {
            opt.SingleLine = true;
        });

        // Hide logs from this category
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        // Logging
        services.AddLogging(b => b.AddSimpleConsole(opt => { opt.SingleLine = true; opt.TimestampFormat = "HH:mm:ss "; }));

        // UkrGuru.Sql setup: register SqlClient with connection string
        var conn = ctx.Configuration.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        services.AddSql(conn, true);

        // Register action
        services.AddTransient<MySqlAction>();

        // Register WJb with actions map directly in DI (Dictionary<string, ActionItem>)
        var actions = new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
        {
            ["MySqlAction"] = new ActionItem(
                type: typeof(MySqlAction).AssemblyQualifiedName!,
                more: null
            )
        };
        services.AddSingleton(actions);
        services.AddWJb(actions: actions);
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

await jobs.EnqueueJobAsync("MySqlAction", new {
    tsql = """
    SELECT CAST(SUM(TRY_CONVERT(decimal(18,4), quantity) * TRY_CONVERT(decimal(18,4), price)) AS decimal(18,2))
    FROM OPENJSON(@Data, '$.items')
    WITH (
        quantity int    '$.quantity',
        price    decimal(18,4) '$.price'
    )
    """,
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
});

await host.RunAsync();

public class MySqlAction(IDbService db, ILogger<MySqlAction> logger) : IAction
{
    private readonly IDbService _db = db;
    private readonly ILogger<MySqlAction> _logger = logger;

    public async Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        // Normalize payload
        var more = jobMore ?? new JsonObject();

        // Read inputs
        var tsql = more.GetString("tsql") ?? throw new InvalidOperationException("TSql is required.");
        var data = more.GetString("data");

        var total = await _db.ExecAsync<decimal>(tsql, data);
        _logger.LogInformation("Order Total: {Result}", total);
    }
}
