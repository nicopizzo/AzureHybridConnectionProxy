using Microsoft.Azure.Relay;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;

namespace AzureHybridConnectionProxy.Core
{
    public interface IHCListener
    {
        Task StartListening(CancellationToken cancellationToken = default);
    }

    internal class HCListener : IHCListener
    {
        private readonly ILogger _Logger;
        private readonly HCOptions _Options;
        private readonly HttpClient _ForwardClient;

        private CancellationToken _CancellationToken;

        public HCListener(HttpClient forwardClient, IOptions<HCOptions> options, ILogger<HCListener> logger)
        {
            _Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ForwardClient = forwardClient ?? throw new ArgumentNullException(nameof(forwardClient));
        }

        public async Task StartListening(CancellationToken cancellationToken = default)
        {
            _CancellationToken = cancellationToken;
            var hcmConnection = $"{_Options.RelayNamespace}/{_Options.RelayConnectionName}";
            _Logger.LogInformation("Starting to listen at {0}", hcmConnection);
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_Options.KeyName, _Options.Key);
            var listener = new HybridConnectionListener(new Uri(hcmConnection), tokenProvider);

            listener.RequestHandler = OnNewRequest;
            _Logger.LogDebug("Opening the hybrid connection");
            await listener.OpenAsync(_CancellationToken);
            _Logger.LogInformation($"Now Listening...");
        }

        private async void OnNewRequest(RelayedHttpListenerContext context)
        {
            _Logger.LogInformation("New Request from {0}", context.Request.RemoteEndPoint.ToString());
            var newRequest = CreateForwardRequest(context);
            var response = await _ForwardClient.SendAsync(newRequest, _CancellationToken);
            await ConvertForwardResponse(context, response);
         
            await context.Response.CloseAsync();

            _Logger.LogInformation("Handled Request from {0}", context.Request.RemoteEndPoint.ToString());
        }

        private HttpRequestMessage CreateForwardRequest(RelayedHttpListenerContext context)
        {
            var newRequest = new HttpRequestMessage();

            newRequest.Method = ConvertHttpMethod(context.Request.HttpMethod);
            newRequest.RequestUri = new Uri($"{_Options.ForwaredHost}{context.Request.Url.AbsolutePath.Replace($"/{_Options.RelayConnectionName}", "")}");
            newRequest.Content = new StreamContent(context.Request.InputStream);
            ConvertHeaders(newRequest.Headers, context.Request.Headers);

            return newRequest;
        }

        private async Task ConvertForwardResponse(RelayedHttpListenerContext context, HttpResponseMessage response)
        {
            context.Response.StatusCode = response.StatusCode;
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                await responseStream.CopyToAsync(context.Response.OutputStream);
            }
        }

        private HttpMethod ConvertHttpMethod(string method)
        {
            return method switch
            {
                "GET" => HttpMethod.Get,
                "POST" => HttpMethod.Post,
                "PUT" => HttpMethod.Put,
                "DELETE" => HttpMethod.Delete,
                _ => HttpMethod.Post
            };
        }

        private void ConvertHeaders(HttpRequestHeaders newHeaders, WebHeaderCollection headers)
        {
            foreach(string key in headers)
            {
                var value = headers[key];
                newHeaders.Add(key, value);
            }
        }
    }
}