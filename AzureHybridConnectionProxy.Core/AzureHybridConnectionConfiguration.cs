using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AzureHybridConnectionProxy.Core
{
    public static class AzureHybridConnectionConfiguration
    {
        public static IServiceCollection AddAzureHybridConnectionListener(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<HCOptions>()
                .Bind(config.GetSection(HCOptions.Name));

            services.AddHttpClient<IHCListener, HCListener>();
            return services;
        }
    }
}
