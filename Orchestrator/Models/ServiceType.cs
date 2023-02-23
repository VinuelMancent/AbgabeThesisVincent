using System.Text.Json.Serialization;

namespace Orchestrator.Models
{
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ServiceType
    {
        Docker,
        Windows
    }
}
