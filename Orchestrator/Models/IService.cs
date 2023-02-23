namespace Orchestrator.Models
{
    public interface IService
    {
        public void Restart();

        public bool Verify();

        public string GetName();
    }
}
