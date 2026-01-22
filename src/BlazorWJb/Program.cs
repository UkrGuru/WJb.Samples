using BlazorWJb.Components;
using BlazorWJb.Logging;
using BlazorWJb.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using WJb;
using WJb.Extensions;
using WJb.Helpers;

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

var actions = ActionMapLoader.CreateFromPath(actionsPath);

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
