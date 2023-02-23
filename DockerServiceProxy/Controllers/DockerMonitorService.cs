using Docker.DotNet;
using Docker.DotNet.Models;
using DockerServiceProxy.Models;

namespace DockerServiceProxy.Controllers;

public class DockerMonitorService : BackgroundService
{
    private DockerClient _client;
    public DockerMonitorService()
    {
        this._client = new DockerClientConfiguration()
            .CreateClient();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IProgress<Message> progress = new DockerProgressReporter();
        await _client.System.MonitorEventsAsync(new ContainerEventsParameters(), progress, stoppingToken);
    }
}