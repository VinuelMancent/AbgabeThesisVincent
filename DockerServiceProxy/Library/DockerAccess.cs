using Docker.DotNet;
using Docker.DotNet.Models;

namespace DockerServiceProxy.Library;

public class DockerAccess
{
    private readonly IDockerClient _client;
    private readonly NLog.Logger _logger;

    public DockerAccess()
    {
        this._client = new DockerClientConfiguration()
            .CreateClient();
        this._logger = NLog.LogManager.GetCurrentClassLogger();
    }
    
    private async Task<IList<ContainerListResponse>> GetAllContainers()
    {
        IList<ContainerListResponse> containers = await _client.Containers.ListContainersAsync(
            new ContainersListParameters(){});

        return containers;
    }

    public ContainerListResponse GetContainerByName(string name)
    {
        var allContainers = GetAllContainers();
        var fullContainerName = ContainerNameHasLeadingSlash(name);
        foreach (var container in allContainers.Result)
        {
            if (container.Names.Contains(fullContainerName))
            {
                return container;
            }
        }
        throw new Exception($"Could not find the given container name {name}");
    }
    
    public async Task RestartContainerAsync(string containerId)
    {
        await _client.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters());
    }

    private string ContainerNameHasLeadingSlash(string containerName)
    {
        if (containerName.StartsWith("/"))
            return containerName;
        return $"/{containerName}";
    }
}