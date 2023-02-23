using Orchestrator.Models;

namespace Orchestrator.Library;

public class DockerProxyListener
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetLogger("OrchestrationLogger");
    public static async void ReceiveUnhealthyService(string serviceId)
    {
        IService? unhealthyService = OrchestratorMethods.GetInstance().GetDockerServiceByContainerId(serviceId);
        if (unhealthyService is null)
            return;
        await OrchestratorMethods.GetInstance().RestartDependentServices(unhealthyService);
    }
}