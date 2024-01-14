using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;

namespace StressApp;

class Program
{
    private static string _address = "http://localhost:5335";

    static async Task Main(string[] args)
    {
        var p = new Program();
        var serviceProvider = await p.Start();
        serviceProvider.Dispose();
    }

    private async Task<ServiceProvider> Start()
    {
        var serviceProvider = Initialize();
        var menu = new Menu(serviceProvider, _address);
        await menu.Start();
        return serviceProvider;
    }

    private ServiceProvider Initialize()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("stress-client", c =>
        {
            c.Timeout = Timeout.InfiniteTimeSpan;
            c.BaseAddress = new Uri(_address);
            c.DefaultRequestHeaders.Add("User-Agent", "Raf Http Client");
            //c.DefaultRequestHeaders.Accept.Add("application/json");
            c.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MaxAge = new TimeSpan(0),
                MustRevalidate = true
            };
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            MaxConnectionsPerServer = 10000,
        })
        .AddPolicyHandler(policy =>
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));
        })
        .AddTypedClient<StressClient>();

        return services.BuildServiceProvider();
    }

}
