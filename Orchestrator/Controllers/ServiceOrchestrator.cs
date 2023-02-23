using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Orchestrator.Library;
using Orchestrator.Models;

namespace Orchestrator.Controllers;

public class ServiceOrchestrator : BackgroundService
{
    private OrchestratorMethods _methods;
    public ServiceOrchestrator(int delayInSeconds = 0, IDictionary<string, IList<string>>? servicesWithDependencies = null, IList<IService>? services = null)
    {
        this._methods = OrchestratorMethods.GetInstance(servicesWithDependencies, services);
    }
    
    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
    }
}