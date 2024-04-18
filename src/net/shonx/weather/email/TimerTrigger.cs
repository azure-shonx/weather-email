namespace net.shonx.weather.email;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using net.shonx.weather.backend;

public class TimerTrigger(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimerTrigger>();

    private readonly HttpHandler httpHandler = new();

    [Function("TimerTrigger")]
    public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
    {
        List<Email>? emails = await httpHandler.GetEmails();
        if (emails is null) // bruh
            return;
        foreach (Email email in emails)
        {
            WeatherForecast? forecast = await httpHandler.GetWeather(email.Zipcode);
            if (forecast is null) // bruh
                return;
            Task task = httpHandler.SendAlert(email.Value, forecast);
            task.Wait();
        }
    }
}
