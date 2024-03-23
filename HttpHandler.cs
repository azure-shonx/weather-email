using Azure;
using Azure.Communication.Email;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

public class HttpHandler
{

    private readonly HttpClient httpClient = new HttpClient();
    private static readonly string CONNECTION_STRING = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
    private readonly EmailClient emailClient = new EmailClient(CONNECTION_STRING);

    public HttpHandler()
    {
    }

    public List<Email> GetEmails()
    {
        return GetEmails0().Result;
    }

    public WeatherForecast GetWeather(int zipcode)
    {
        return GetWeather0(zipcode).Result;
    }

    public async Task SendAlert(string email, WeatherForecast forecast)
    {
        try
        {
            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Completed,
                senderAddress: "DoNotReply@weather.shonx.net",
                recipientAddress: email,
                subject: "Current Weather",
                htmlContent: BuildHTMLMessage(forecast),
                plainTextContent: BuildPlainTextMessage(forecast));

            EmailSendResult statusMonitor = emailSendOperation.Value;

            Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

            /// Get the OperationId so that it can be used for tracking the message for troubleshooting
            string operationId = emailSendOperation.Id;
            Console.WriteLine($"Email operation id = {operationId}");
        }
        catch (RequestFailedException ex)
        {
            /// OperationID is contained in the exception message and can be used for troubleshooting purposes
            Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
        }

    }

    private string BuildHTMLMessage(WeatherForecast forecast)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("<p>It is currently " + forecast.summary + "</p>" + Environment.NewLine);
        builder.Append("<p>The temperature is " + forecast.temperature + "C</p>" + Environment.NewLine);
        if (forecast.isRainy)
            builder.Append("<p>There is percipitation.</p>" + Environment.NewLine);
        else
            builder.Append("<p>There is no percipitation.</p>" + Environment.NewLine);
        return builder.ToString();
    }
    private string BuildPlainTextMessage(WeatherForecast forecast)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("It is currently " + forecast.summary + Environment.NewLine);
        builder.Append("The temperature is " + forecast.temperature + "C" + Environment.NewLine);
        if (forecast.isRainy)
            builder.Append("There is percipitation." + Environment.NewLine);
        else
            builder.Append("There is no percipitation." + Environment.NewLine);
        return builder.ToString();
    }

    private async Task<WeatherForecast> GetWeather0(int zipcode)
    {
        string URL = Program.WEATHER_BACKEND_PROVIDER + "weather/get/" + zipcode;

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URL);

        using (HttpResponseMessage response = await httpClient.SendAsync(request))
        {
            var statusCode = response.StatusCode;
            if ((int)statusCode != 200)
            {
                return null;
            }

            string apiResponse = await response.Content.ReadAsStringAsync();
            if (String.IsNullOrEmpty(apiResponse))
                throw new NullReferenceException("Provider response is empty. Is it online?");
            return JsonConvert.DeserializeObject<WeatherForecast>(apiResponse);
        }
    }
    private async Task<List<Email>> GetEmails0()
    {
        string URL = Program.WEATHER_BACKEND_PROVIDER + "emails/get/";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URL);

        using (HttpResponseMessage response = await httpClient.SendAsync(request))
        {
            var statusCode = response.StatusCode;
            if ((int)statusCode != 200)
            {
                return null;
            }
            string apiResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Email>>(apiResponse);
        }
    }
}