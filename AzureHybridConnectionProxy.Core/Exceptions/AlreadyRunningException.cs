
namespace AzureHybridConnectionProxy.Core
{
    public class AlreadyRunningException : Exception
    {
        public override string Message => "Listener already running";
    }
}
