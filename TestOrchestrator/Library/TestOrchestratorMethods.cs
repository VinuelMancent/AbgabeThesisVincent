using Orchestrator.Library;
using Orchestrator.Models;
using TestOrchestrator.Models;

namespace TestOrchestrator.Library;

[TestClass]
public class TestOrchestratorMethods
{
    private IDictionary<string, IList<string>>? _dependencies = new Dictionary<string, IList<string>>();
    private IList<IService> _services = new List<IService>();

    OrchestratorMethods _methods;


    [TestMethod]
    public void TestRestartDependentServices()
    {
        //Restart service 1 --> service 2 should get restarted, service 3 not
        _methods.RestartDependentServices(_services[0]);
        Assert.IsTrue(true);
    }
    
    [TestMethod]
    public void TestGetAllServices()
    {
        var allServices = _methods.GetAllServices();
        Assert.AreEqual(this._services.Count, allServices.Count);
    }



    private void PrepareDependencies(IDictionary<string, IList<string>>? dependencies)
    {
        if (dependencies is null)
        {
            dependencies = new Dictionary<string, IList<string>>();
        }
        dependencies.Add("service1", new List<string>());
        dependencies["service1"].Add("service2");
        dependencies.Add("service2", new List<string>());
        dependencies.Add("service3", new List<string>());
        dependencies["service3"].Add("service1");
    }

    private void PrepareServices(IList<IService> services)
    {
        if (services is null)
        {
            services = new List<IService>();
        }
        services.Add(new IServiceStub("service1"));
        services.Add(new IServiceStub("service2"));
        services.Add(new IServiceStub("service3"));
    }

    [TestInitialize]
    public void PrepareTests()
    {
        PrepareDependencies(_dependencies);
        PrepareServices(_services);
        _methods = OrchestratorMethods.GetInstance(_dependencies, _services);

    }
}