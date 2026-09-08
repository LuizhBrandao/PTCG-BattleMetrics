using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PTCGBattleMetrics.Application.Services;
using Client;
using PTCGBattleMetrics.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// App Services
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<BattleMetricsClientService>();

var host = builder.Build();

// Initialize local sync queue state
var clientService = host.Services.GetRequiredService<BattleMetricsClientService>();
await clientService.InitializeAsync();

await host.RunAsync();
