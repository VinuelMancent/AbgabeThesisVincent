using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;
using NLog.Fluent;
using Orchestrator.Library;
using Orchestrator.Models;

namespace Orchestrator.Controllers;


[SuppressMessage("Interoperability", "CA1416:Plattformkompatibilität überprüfen")]
public class WindowsEventListener : BackgroundService
{
    
    public event EventHandler<string>? ServiceStatusReached;

    private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("OrchestrationLogger");
    private IList<ServiceController> _servicesToListen = new List<ServiceController>();
    public static bool PauseListening = false;
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var allWindowsServices = OrchestratorMethods.GetInstance().GetAllWindowsServices();
        foreach (var service in allWindowsServices)
        {
            AddServiceController(service);
        }
        ServiceStatusReached += OnStatusReached;
        
        foreach (var service in _servicesToListen)
        {
            WaitForStatus(service, ServiceControllerStatus.Stopped, stoppingToken);
        }
        return Task.CompletedTask;
    }
    private async void OnStatusReached(object? sender, string name)
    {
        if(!PauseListening)
        {
            _logger.Info($"WindowsService {name} just stopped");
            var orchestratorMethods = OrchestratorMethods.GetInstance();
            IService svc = orchestratorMethods.GetWinServiceByName(name)!;
            await orchestratorMethods.RestartDependentServices(svc);
        }
    }
    private ServiceController GetServiceByName(string name)
    {
        var services = ServiceController.GetServices();
        foreach (var service in services)
        {
            if (service.ServiceName == name || service.DisplayName == name)
            {
                return service;
            }
        }
        throw new Exception($"Can't find service {name}");
    }
    private void AddServiceController(IService service)
    {
        var serviceController = GetServiceByName(service.GetName());
        _servicesToListen.Add(serviceController);
    }

    protected virtual async void WaitForStatus(ServiceController service, ServiceControllerStatus status, CancellationToken stoppingToken)
    {
        ServiceControllerStatus lastStatus = ServiceControllerStatus.Running;
        ServiceControllerStatus statusToWaitFor = ServiceControllerStatus.Stopped;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (lastStatus == ServiceControllerStatus.Running)
                statusToWaitFor = ServiceControllerStatus.Stopped;
            if (lastStatus == ServiceControllerStatus.Stopped)
                statusToWaitFor = ServiceControllerStatus.Running;
            await Task.Run(() => service.WaitForStatus(statusToWaitFor));
            if(statusToWaitFor == ServiceControllerStatus.Stopped)
                ServiceStatusReached?.Invoke(this, service.ServiceName);
            lastStatus = statusToWaitFor;
        }
    }

    

}