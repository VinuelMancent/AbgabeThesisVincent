using Docker.DotNet.Models;

namespace DockerServiceProxy.Models;

public class DockerProgressReporter : IProgress<Message>
{
    public static string OrchestratorAddress = "192.168.178.63";
    public static string UnhealthyStatus = "health_status: unhealthy";
    private const string StoppedStatus = "stop";

    public static bool RestartOngoing = false;
    public static IList<string> ServicesToListenOn = new List<string>();

    private HttpClient _httpClient;

    public DockerProgressReporter()
    {
        this._httpClient = new HttpClient();
    }
    public void Report(Message value)
    {
        if (ServicesToListenOn.Contains(value.Actor.ID) && (value.Action.Equals(UnhealthyStatus) || value.Action.Equals(StoppedStatus)))
        {
            var id = value.Actor.ID;
            if (!IsRestartOngoing())
            {
                Console.WriteLine($"Service {id} stopped working");
                var body = JsonContent.Create(id, typeof(string));

                var urlString = $"http://{OrchestratorAddress}:4097/ReceiveUnhealthyService";
                this._httpClient.PostAsync(urlString, body);
                Console.WriteLine($"Sending request about {id} to {urlString} with body {body.Value.ToString()}");
            }
        }
    }

    private static bool IsRestartOngoing()
    {
        return RestartOngoing;
    }
}