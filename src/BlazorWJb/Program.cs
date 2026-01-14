using BlazorWJb.Components;
using BlazorWJb.Logging;
using BlazorWJb.Services;
using System.Text.Json;
using WJb;
using WJb.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// ✅ Add the file provider (from config)
builder.Logging.AddDailyFile(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- Load actions from actions.json ---
var actionsPath = Path.Combine(builder.Environment.WebRootPath, "WJb", "actions.json");
if (!File.Exists(actionsPath))
    throw new FileNotFoundException("actions.json was not found in the content root.", actionsPath);

var json = await File.ReadAllTextAsync(actionsPath);
var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

var actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, jsonOptions)
              ?? throw new InvalidOperationException("Failed to deserialize actions.json into ActionItem dictionary.");

// --- Supply actions to WJb ---
builder.Services.AddWJb(actions, addScheduler: true);

builder.Services.AddSingleton<ILogFileReader, LogFileReader>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
