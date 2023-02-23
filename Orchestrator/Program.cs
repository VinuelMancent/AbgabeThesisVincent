using NLog.Targets;
using Orchestrator.Controllers;


// NLog config
EventLogTarget target = new EventLogTarget();
target.Name = "OrchestrationLogger";
target.Source = "TrumpfOrchestrator Service";
target.Log = "Application";
target.MachineName = ".";
target.Layout = "${logger}: ${message}";
NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "TrumpfOrchestrator";
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHostedService<ServiceOrchestrator>();
builder.Services.AddHostedService<WindowsEventListener>();

var app = builder.Build();
app.UseRouting();
app.MapControllers();
var port = Environment.GetEnvironmentVariable("TrumpfOrchestratorPort");
if (port is null)
{
    port = "4097";
}
NLog.Logger _logger = NLog.LogManager.GetLogger("OrchestrationLogger");
app.Urls.Add($"http://*:{port}");
app.Run();