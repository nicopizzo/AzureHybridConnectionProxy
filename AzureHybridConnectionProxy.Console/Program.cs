using AzureHybridConnectionProxy.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .Build();

var serviceProvider = new ServiceCollection()
    .AddLogging(l =>
    {
        l.AddConfiguration();
        l.AddConsole();
    })
    .AddAzureHybridConnectionListener(config)
    .BuildServiceProvider();

var listener = serviceProvider.GetRequiredService<IHCListener>();
await listener.StartListening();

Console.ReadLine();