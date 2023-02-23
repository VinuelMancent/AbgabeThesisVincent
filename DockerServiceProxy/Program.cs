using DockerServiceProxy.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Define NLog
var config = new NLog.Config.LoggingConfiguration();

// Targets where to log to: File and Console
var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

// Rules for mapping loggers to targets            
config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);

// Apply config           
NLog.LogManager.Configuration = config;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHostedService<DockerMonitorService>();
var app = builder.Build();

app.MapControllers();
app.UseRouting();

// Configure the HTTP request pipeline.
/*
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.Run();