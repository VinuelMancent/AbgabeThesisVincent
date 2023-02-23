using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;

namespace Orchestrator.Models
{
    [SuppressMessage("Interoperability", "CA1416:Plattformkompatibilität überprüfen")]
    public class WindowsService : IService
    {
        private string id;
        private ServiceController? _serviceInstance;
        private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("OrchestrationLogger");

        public WindowsService(string id)
        {
            this.id = id;
        }

        public void Restart()
        {

            if (this._serviceInstance is null)
            {
                this._logger.Error($"While restarting: WindowsServce {id} can't be validated nor found");
                return;
            }
           
            try
            {
                this._logger.Info($"Stopping WindowsService {this.id}");
                this._serviceInstance!.Stop();
            }catch(Exception e)
            {
                this._logger.Info($"Can't stop service {this.id}. Reason: {e.Message}");
            }

            try
            {
                this._serviceInstance.WaitForStatus(ServiceControllerStatus.Stopped);
                this._logger.Info($"Restarting WindowsService {this.id}");
                this._serviceInstance.Start();
            }
            catch(Exception e)
            {
                this._logger.Info($"Can't start service {this.id}. Reason: {e.Message}");
            }
            
        }

        public bool Verify()
        {
            this._serviceInstance = GetServiceByName();
            if(this._serviceInstance is null)
            {
                this._logger.Info($"Failure: WindowsService {this.id} could not get verified");
                return false;
            }
            else if(this._serviceInstance.Status == ServiceControllerStatus.Running)
            {
                this._logger.Info($"Success: WindowsService {this.id} is verified");
                return true;
            }
            else
            {
                this._logger.Info($"Failure: WindowsService {this.id} was found, but not started");
                return false;
            }
        }

        public string GetName()
        {
            return this.id;
        }

        private ServiceController? GetServiceByName()
        {
            var allServices = ServiceController.GetServices();
            foreach (var service in allServices)
            {
                if (service.DisplayName == id || service.ServiceName == id)
                {
                    return service;
                }
            }
            return null;
        }
    }
}
