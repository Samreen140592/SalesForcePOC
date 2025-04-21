using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json.Linq;


namespace SalesForceFunctionApp
{
    public class SalesforceToServiceBus
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private static readonly HttpClient _httpClient = new();
        public SalesforceToServiceBus(ILoggerFactory loggerFactory, IConfiguration config)
        {
            _logger = loggerFactory.CreateLogger<SalesforceToServiceBus>();
            _config = config;
        }

        [Function("SalesforceToServiceBus")]
        public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo timer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            try
            {
                var accessToken = await GetSalesforceAccessTokenAsync();
                var jsonData = await QuerySalesforceAsync(accessToken);
                await SendToServiceBusAsync(jsonData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}", ex);
            }
        }
        
        private async Task<string> GetSalesforceAccessTokenAsync()
        {
            string clientId = _config["ClientId"];
            string clientSecret = _config["ClientSecret"];
            string username = _config["Username"];
            string password = _config["Password"];
            string token = _config["SecurityToken"];
            string loginUrl = _config["LoginUrl"];

            // Validate config values
            if (string.IsNullOrWhiteSpace(clientId))
                throw new InvalidOperationException("Missing configuration: ClientId");
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new InvalidOperationException("Missing configuration: ClientSecret");
            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Missing configuration: Username");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Missing configuration: Password");
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Missing configuration: SecurityToken");
            if (string.IsNullOrWhiteSpace(loginUrl))
                throw new InvalidOperationException("Missing configuration: LoginUrl");

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password + token)
        });

            var response = await _httpClient.PostAsync($"{loginUrl}/services/oauth2/token", content);
            response.EnsureSuccessStatusCode();

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            return result["access_token"]?.ToString();
        }

        private async Task<string> QuerySalesforceAsync(string accessToken)
        {
            var instanceUrl = _config["InstanceUrl"];
            var query = "SELECT Id, Name FROM Account"; // Customize your SOQL query here

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{instanceUrl}/services/data/v58.0/query?q={Uri.EscapeDataString(query)}");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private async Task SendToServiceBusAsync(string message)
        {
            var connectionString = _config["ServiceBusConnectionString"];
            var queueName = "DevTest";

            await using var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(queueName);

            var serviceBusMessage = new ServiceBusMessage(message);
            await sender.SendMessageAsync(serviceBusMessage);

            _logger.LogInformation("Message sent to Service Bus queue: DevTest");
        }
    }
}
