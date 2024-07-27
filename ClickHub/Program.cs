using ClickHub.Services;
using ClickHub.Components;
using ClickHub.Data;
using ClickHub.Interfaces;
using ClickHub.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});

builder.Services.AddMvc();

// Add custom services
builder.Services.AddSingleton<IDomainDatabase>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new MySqlDomainDatabase(connectionString);
});
builder.Services.AddSingleton<IDomainService, DomainService>();
builder.Services.AddSingleton<IClickTrackingService, ClickTrackingService>();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var domainService = scope.ServiceProvider.GetRequiredService<IDomainService>();
    await domainService.InitializeAsync();
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapGet("/track", async (HttpContext context, IClickTrackingService trackingService) =>
{
    await trackingService.TrackClickAsync(context).ConfigureAwait(false);
});
app.Run();
