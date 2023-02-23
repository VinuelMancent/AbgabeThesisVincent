using Orchestrator.Library;

namespace Orchestrator.Models
{
    public class ServiceFactory
    {

        public static IService GetService(ServiceWithDependencies service)
        {
            switch (service.Type)
            {
                case ServiceType.Windows:
                    return new WindowsService(service.Name);
                case ServiceType.Docker:
                    return new DockerService(service.Name);
                default:
                    throw new Exception("Can't find given ServiceType");
            }
        }
    }
}
