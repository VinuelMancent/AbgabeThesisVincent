using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Orchestrator.Models;

public class ServiceWithDependencies
{
    public ServiceWithDependencies(string name, ServiceType type, List<string> dependsOn)
    {
        Name = name;
        Type = type;
        DependsOn = dependsOn;
    }

    [JsonPropertyName("name")]
    public string Name {get; set; }

    [JsonPropertyName("type")]
    public ServiceType Type { get; set; }

    [JsonPropertyName("dependsOn")]
    public List<string> DependsOn { get; set; }
    
    
}