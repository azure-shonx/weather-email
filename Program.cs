
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
public class Program
{
    public const string WEATHER_BACKEND_PROVIDER = "https://backend.weather.shonx.net/";

    public static void Main(string[] args)
    {
        HostBuilder host = new HostBuilder()
        .ConfigureFunctionsWebApplication()
        .ConfigureServices(services =>
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();
        })
        .Build();

        host.Run();
    }

}
