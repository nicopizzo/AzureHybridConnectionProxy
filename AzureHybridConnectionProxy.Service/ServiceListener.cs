using AzureHybridConnectionProxy.Core;

namespace AzureHybridConnectionProxy.Service
{
    public class ServiceListener : IHostedService
    {
        private readonly IHCListener _Listener;

        public ServiceListener(IHCListener listener)
        {
            _Listener = listener ?? throw new ArgumentNullException(nameof(listener));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _Listener.StartListening(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _Listener.StopListening(cancellationToken);
        }
    }
}
