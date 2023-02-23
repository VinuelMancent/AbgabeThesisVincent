using Orchestrator.Models;

namespace TestOrchestrator.Models;

public class IServiceStub : IService
{
    private string _name;

    public IServiceStub(string name)
    {
        this._name = name;
    }
    public void Restart()
    {
        Console.WriteLine($"Restarting service {_name}");
    }

    public bool Verify()
    {
        Console.WriteLine($"Service {_name} is verified");
        return true;
    }

    public string GetName()
    {
        return _name;
    }
}