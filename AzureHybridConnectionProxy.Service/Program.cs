using AzureHybridConnectionProxy.Core;
using AzureHybridConnectionProxy.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddAzureHybridConnectionListener(builder.Configuration);
builder.Services.AddHostedService<ServiceListener>();

var app = builder.Build();

app.UseHealthChecks("/hc");

app.Run();
