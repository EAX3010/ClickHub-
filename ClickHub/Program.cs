using ClickHub.Services;
using ClickHub.Components;
using ClickHub.Data;
using ClickHub.Interfaces;
using ClickHub.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using ClickHub.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});

builder.Services.AddMvc();

// Configure logging
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

// Add custom services
builder.Services.AddSingleton<IDomainDatabase>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var logger = sp.GetRequiredService<ILogger<MySqlDomainDatabase>>();
    return new MySqlDomainDatabase(connectionString, logger);
});

builder.Services.AddSingleton<IDomainService, DomainService>();

// Configure ClickTrackingService with a channel
builder.Services.AddSingleton<Channel<ClickData>>((_) => Channel.CreateUnbounded<ClickData>(new UnboundedChannelOptions { SingleReader = true }));
builder.Services.AddSingleton<IClickTrackingService, ClickTrackingService>();

// Add background service for processing clicks
builder.Services.AddHostedService<ClickProcessingService>();

var app = builder.Build();

// Initialize DomainService
using (var scope = app.Services.CreateScope())
{
    var domainService = scope.ServiceProvider.GetRequiredService<IDomainService>();
    await domainService.InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Optimize the track endpoint
app.MapGet("/track", (HttpContext context, IClickTrackingService trackingService) =>
{
    return trackingService.TrackClickAsync(context);
})
.WithName("TrackClick")
.WithDisplayName("Track Click")
.DisableAntiforgery();

app.Run();