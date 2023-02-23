using Microsoft.AspNetCore.Mvc;
using Orchestrator.Library;
using Orchestrator.Models;

namespace Orchestrator.Controllers;

[ApiController]
[Route("/")]
public class DockerProxyListenerController : ControllerBase
{
    private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("OrchestrationLogger");
    
    [HttpPost]
    [Route("ReceiveUnhealthyService")]
    public void ReceiveUnhealthyService([FromBody] string serviceId)
    {
        this._logger.Info($"Message from Proxy: Service {OrchestratorMethods.GetInstance().GetDockerServiceByContainerId(serviceId)} with id {serviceId} became unhealthy or stopped");
        DockerProxyListener.ReceiveUnhealthyService(serviceId);
    }
}