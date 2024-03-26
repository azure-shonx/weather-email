using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace net.shonx.weather.email
{
    public class TimerTrigger
    {
        private readonly ILogger _logger;

        private readonly HttpHandler httpHandler;

        public TimerTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTrigger>();
            httpHandler = new HttpHandler();
        }

        [Function("TimerTrigger")]
        public void Run([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
        {
            List<Email> emails = httpHandler.GetEmails();
            foreach (Email email in emails)
            {
                WeatherForecast forecast = httpHandler.GetWeather(email.zipcode);
                Task task = httpHandler.SendAlert(email.email, forecast);
                task.Wait();
            }
        }


    }
}
