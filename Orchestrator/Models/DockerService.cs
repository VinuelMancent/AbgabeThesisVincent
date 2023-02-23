using Docker.DotNet.Models;
using Orchestrator.Library;

namespace Orchestrator.Models
{
    public class DockerService : IService
    {
        private string _name;
        private string? _id;
        private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("OrchestrationLogger");
        private string? _vhostIP;
        private string? _dockerProxyPort;

        public DockerService(string name)
        {
            this._name = name;
            this._dockerProxyPort = Environment.GetEnvironmentVariable("DockerProxyPort");
            if (this._dockerProxyPort is null)
            {
                this._dockerProxyPort = "4096";
            }
            this._vhostIP = Environment.GetEnvironmentVariable("VHOST_IP");
            if (this._vhostIP is null)
            {
                throw new Exception("Can't reach VHOST, because env var \"VHOST_IP\" isn't set");
            }
        }

        public void Restart()
        {
            string dockerProxyUri = $"{this._vhostIP}:{this._dockerProxyPort}/RestartContainer/{this._id}";
            this._logger.Info($"Restarting DockerService {this._name} via GetRequest to {dockerProxyUri}");
            HttpClientService.Instance.SendHttpGet(dockerProxyUri);
        }

        public bool Verify()
        {
            HttpClientService service = HttpClientService.Instance;
            string dockerProxyUri = $"{this._vhostIP}:{this._dockerProxyPort}/GetContainerId/{this._name}";
            var task = Task.Run(() => service.SendHttpGet(dockerProxyUri)); 
            task.Wait();
            var response = task.Result;
            if (response.IsSuccessStatusCode)
            {
                this._logger.Info($"Success: DockerService {this._name} is verified");
                this._id = response.Content.ReadAsStringAsync().Result;
                return true;
            }
            this._logger.Info($"Failure: DockerService {this._name} could not get verified");
            return false;
        }

        public string GetName()
        {
            return this._name;
        }

        public string? GetId()
        {
            return this._id;
        }
    }
}
