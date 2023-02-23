using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Orchestrator.Controllers;
using Orchestrator.Models;

namespace Orchestrator.Library;

[SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
public class OrchestratorMethods
{
    
    private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("OrchestrationLogger");
    
    private static readonly Lazy<OrchestratorMethods> Lazy =
        new Lazy<OrchestratorMethods>(() => new OrchestratorMethods());

    public static OrchestratorMethods GetInstance(IDictionary<string, IList<string>>? servicesWithDependencies = null,
        IList<IService>? services = null)
    {
        if (servicesWithDependencies is not null)
        {
            _servicesWithDependencies = servicesWithDependencies;
        }
        if (services is not null)
        {
            _services = services;
        }
        return Lazy.Value;
    }
    
    
    private static IDictionary<string, IList<string>>? _servicesWithDependencies;
    private static IList<IService>? _services;
    private readonly string? _configDirectoryPath;
    private readonly string? _dockerProxyPort;
    private string? _vhostIP;
    private OrchestratorMethods()
    {
        this._vhostIP = Environment.GetEnvironmentVariable("VHOST_IP");
        if (this._vhostIP is null)
        {
            this._vhostIP = "http://169.254.11.0";
            this._logger.Info($"Env var 'VHOST_IP' was not set, defaulted to {this._vhostIP}");
        }

        if (!this._vhostIP.StartsWith("http://"))
        {
            this._vhostIP = $"http://{this._vhostIP}";
        }
        this._dockerProxyPort = Environment.GetEnvironmentVariable("DockerProxyPort");
        if (this._dockerProxyPort is null)
        {
            this._dockerProxyPort = "4096";
        }
        this._configDirectoryPath = Environment.GetEnvironmentVariable("OrchestratorConfigDirectory");
        if(this._configDirectoryPath is null)
        {
            this._configDirectoryPath = "C:\\TRUMPF\\Service\\Orchestrator\\Config";
        }
        bool bothNull = true;
        if (_servicesWithDependencies is null)
        {
            _servicesWithDependencies = new Dictionary<string, IList<string>>();
        }
        else
        {
            bothNull = false;
        }

        if (_services is null)
        {
            _services = new List<IService>();
        }
        else
        {
            bothNull = false;
        }

        if (bothNull)
        {
            var files = Directory.GetFiles(_configDirectoryPath);
            
            foreach (var file in files)
            {
                _servicesWithDependencies[Path.GetFileNameWithoutExtension(file)] = new List<string>();
            }
            
            IList<ServiceWithDependencies> servicesWithDependencies = new List<ServiceWithDependencies>();
            foreach (var file in files)
            {
                var fileAsString = File.ReadAllText(file);
                ServiceWithDependencies svc = JsonSerializer.Deserialize<ServiceWithDependencies>(fileAsString)!;
                servicesWithDependencies.Add(svc);
                var serviceObject = ServiceFactory.GetService(svc);
                _services.Add(serviceObject);
                foreach (var dependency in svc.DependsOn)
                {
                    _servicesWithDependencies[dependency].Add(svc.Name);
                }
            }
        }
        
    }

    private IList<IService> GetDependentServices(IService service, IDictionary<string, IList<string>>? dependencyMap = null, IList<string>? alreadyRemovedServices = null)
    {
        HashSet<IService> servicesToRestart = new HashSet<IService>();
        servicesToRestart.Add(service);
        if (dependencyMap is null)
            dependencyMap = new Dictionary<string, IList<string>>(_servicesWithDependencies!);
        if (alreadyRemovedServices is null)
            alreadyRemovedServices = new List<string>();
        
        var dependencies = dependencyMap[service.GetName()];
        dependencyMap.Remove(service.GetName());
        alreadyRemovedServices.Add(service.GetName());
        foreach (var depService in dependencies)
        {
            if(!alreadyRemovedServices.Contains(depService))
                servicesToRestart.UnionWith(GetDependentServices(GetServiceByName(depService)!, dependencyMap, alreadyRemovedServices));
        }
        return servicesToRestart.ToImmutableList();
    }

    public async Task RestartDependentServices(IService service)
    {
        this._logger.Info($"Starting the restartprocess because of {service.GetName()}");
        var dependentServices = GetDependentServices(service);
        this._logger.Info($"Dependent Services: \n {this.DependentServicesToString(dependentServices)}");
        int index = 0;
        foreach (var svc in dependentServices)
        {
            if (index == 0)
            {
                string url = "";
                if (this._vhostIP.StartsWith("http://"))
                {
                    url = $"{this._vhostIP}:{this._dockerProxyPort}/stopListening";
                }
                else
                {
                    url = $"http://{this._vhostIP}:{this._dockerProxyPort}/stopListening";
                }
                
                await HttpClientService.Instance.SendHttpGet(url);
                WindowsEventListener.PauseListening = true;
            }
            
            this._logger.Info($"Restarting service {svc.GetName()} because of service {service.GetName()}");
            svc.Restart();
            
            if (index == dependentServices.Count-1)
            {
                string url = "";
                if (this._vhostIP.StartsWith("http://"))
                {
                    url = $"{this._vhostIP}:{this._dockerProxyPort}/returnListening";
                }
                else
                {
                    url = $"http://{this._vhostIP}:{this._dockerProxyPort}/returnListening";
                }
                
                await HttpClientService.Instance.SendHttpGet(url);
                WindowsEventListener.PauseListening = false;
            }
            
            index++;
            
        }
    }

    public IService? GetDockerServiceByContainerId(string containerId)
    {
        foreach (var service in _services!)
        {
            if (service is DockerService)
            {
                var tempDockerService = (DockerService)service;
                this._logger.Info($"Service {service.GetName()} has id {tempDockerService.GetId()}");
                if (tempDockerService.GetId() == containerId)
                {
                    return service;
                }
            }
        }
        return null;
    }

    public IService? GetWinServiceByName(string name)
    {
        foreach (var service in _services!)
        {
            if (service is WindowsService)
            {
                var tempWinService = (WindowsService)service;
                if (tempWinService.GetName() == name)
                {
                    return service;
                }
            }
        }
        return null;
    }

    public IList<IService> GetAllWindowsServices()
    {
        IList<IService> allWindowsServices = new List<IService>();
        foreach (var service in _services!)
        {
            if (service is WindowsService)
            {
                allWindowsServices.Add(service);
            }
        }
        return allWindowsServices;
    }

    private IService? GetServiceByName(string name)
    {
        foreach (var service in _services!)
        {
            if (service.GetName().Equals(name))
            {
                return service;
            }
        }
        return null;
    }

    public IList<IService> GetAllServices()
    {
        return _services!;
    }

    public IDictionary<string, IList<string>> GetAllServicesWithDependencies()
    {
        return _servicesWithDependencies!;
    }

    private string DependentServicesToString(IList<IService> dependentServices)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var service in dependentServices)
        {
            sb.Append($"{service.GetName()} \n");
        }

        return sb.ToString();
    }

}