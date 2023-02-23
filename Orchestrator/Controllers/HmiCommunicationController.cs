using System.Text;
using Microsoft.AspNetCore.Mvc;
using Orchestrator.Library;
using Orchestrator.Models;

namespace Orchestrator.Controllers;

[ApiController]
public class HmiCommunicationController : ControllerBase
{
    
    private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("OrchestrationLogger");
    
    [HttpGet]
    [Route("VerifyServices")]
    public string VerifyServices()
    {
        var allServices = OrchestratorMethods.GetInstance().GetAllServices();
        IList<IService> unsuccessfulVerifiedServices = new List<IService>();
        foreach (var service in allServices)
        {
            var success = service.Verify();
            if (!success)
            {
                unsuccessfulVerifiedServices.Add(service);
            }
        }
        if (unsuccessfulVerifiedServices.Count == 0)
        {
            this._logger.Info("All services are verified and running on startup");
            return "";
        }
        else
        {
            foreach (var service in unsuccessfulVerifiedServices)
            {
                this._logger.Info($"Service {service.GetName()} was not up on startup and gets restarted");
                service.Restart();
                var success = service.Verify();
                if (success)
                {
                    this._logger.Info($"Restart of {service.GetName()} was a success");
                    unsuccessfulVerifiedServices.Remove(service);
                }
                else
                {
                    this._logger.Info($"Restart of {service.GetName()} was a unsuccessful");
                }
            }
        }
        if (unsuccessfulVerifiedServices.Count == 0)
        {
            return "";
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            foreach (var service in unsuccessfulVerifiedServices)
            {
                sb.Append(service.GetName());
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }

    [HttpGet]
    [Route("GetAllServices")]
    public string GetAllServices()
    {
        var allServices = OrchestratorMethods.GetInstance().GetAllServices();
        StringBuilder sb = new StringBuilder();
        foreach (var service in allServices)
        {
            sb.Append(service.GetName());
            sb.Append('\n');
        }
        return sb.ToString();
    }
    
    [HttpGet]
    [Route("GetAllServicesWithDependencies")]
    public string GetAllServicesWithDependencies()
    {
        var allServices = OrchestratorMethods.GetInstance().GetAllServicesWithDependencies();
        StringBuilder sb = new StringBuilder();
        foreach (var kv in allServices)
        {
            sb.Append($"{kv.Key}: \n");
            foreach (var dependency in kv.Value)
            {
                sb.Append($"\t {dependency} \n");
            }
        }
        return sb.ToString();
    }
}