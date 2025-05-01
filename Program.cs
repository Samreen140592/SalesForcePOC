using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SalesForceFunctionApp;

public class Program
{
    public static void Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Register HttpClientFactory
                services.AddHttpClient();

                // Add any other service registrations here
            })
            .Build();

        host.Run();
    }
}