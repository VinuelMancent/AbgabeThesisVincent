using DockerServiceProxy.Library;
using DockerServiceProxy.Models;
using Microsoft.AspNetCore.Mvc;

namespace DockerServiceProxy.Controllers;

[ApiController]
[Route("/")]
public class DockerMonitorController : ControllerBase
{
    private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    [HttpGet]
    [Route("stopListening")]
    public void StopListening()
    {
        DockerProgressReporter.RestartOngoing = true;
        this._logger.Info("Stopped sending info about unhealthy/stopped containers");
    }

    [HttpGet]
    [Route("returnListening")]
    public void ReturnListening()
    {
        // waiting 3 seconds so all containers can get back to running. Otherwise it would detect a service as stopped before it restarted and do all over again.
        System.Threading.Thread.Sleep(3000);
        DockerProgressReporter.RestartOngoing = false;
        this._logger.Info("Returned to sending info about unhealthy/stopped containers");
    }

    [HttpGet]
    [Route("RestartContainer/{id}")]
    public async Task RestartService([FromRoute]string id)
    {
        this._logger.Info($"Restarting Container with id {id}");
        var dockerAccess = new DockerAccess();
        await dockerAccess.RestartContainerAsync(id);
    }

    [HttpGet]
    [Route("GetContainerId/{name}")]
    public string GetContainerId([FromRoute] string name)
    {
        var ipAdd = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4();
        DockerProgressReporter.OrchestratorAddress = $"{ipAdd}";
        _logger.Info($"Getting container with name {name}");
        var dockerAccess = new DockerAccess();
        var container = dockerAccess.GetContainerByName(name);
        
        //Getting the id of a container means it should get observed --> adding it to the list of containers to observe
        DockerProgressReporter.ServicesToListenOn.Add(container.ID);
        
        _logger.Info($"Returning container {container.Names[0]} with id {container.ID}");
        return container.ID;
    }
}