using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json.Linq;

namespace SalesForceFunctionApp;

public class SalesforceToServiceBus
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public SalesforceToServiceBus(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<SalesforceToServiceBus> logger)
    {
        _logger = logger;
        _config = config;
        _httpClient = httpClientFactory.CreateClient();
    }

    [Function("SalesforceToServiceBus")]
    public async Task RunAsync([TimerTrigger("0 */30 * * * *", RunOnStartup = true)] TimerInfo timer)
    {
        _logger.LogInformation($"Timer triggered at: {DateTime.UtcNow}");
        try
        {
            Token accessToken = await GetSalesforceAccessTokenAsync();
            var jsonData = await QuerySalesforceAsync(accessToken);
            await SendToServiceBusAsync(jsonData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute function");
        }
    }

    private async Task<Token> GetSalesforceAccessTokenAsync()
    {
        var clientId = _config["clientId"];
        var clientSecret = _config["clientSecret"];
        var username = _config["Salesforce:Username"];
        var password = _config["Password"];
        var token = _config["SecurityToken"];
        var loginUrl = "https://login.salesforce.com";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password + token)
        });

        var response = await _httpClient.PostAsync($"{loginUrl}/services/oauth2/token", content);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Salesforce authentication failed with status code {response.StatusCode}. Response: {errorContent}");
            throw new HttpRequestException($"Failed to authenticate with Salesforce: {errorContent}");
        }
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JObject.Parse(responseContent);
        Token tokenObj = new();
        tokenObj.AccessToken = result["access_token"]?.ToString();
        tokenObj.InstanceUrl = result["instance_url"]?.ToString();
        return tokenObj;
    }

    private async Task<string> QuerySalesforceAsync(Token token)
    {
        var instanceUrl = token.InstanceUrl;
        var query = "SELECT Id, Name FROM Account LIMIT 5";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{instanceUrl}/services/data/v58.0/query?q={Uri.EscapeDataString(query)}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
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

        var sbMessage = new ServiceBusMessage(message);
        await sender.SendMessageAsync(sbMessage);

        _logger.LogInformation("Sent message to Service Bus.");
    }
}

internal class Token
{
    public string? AccessToken { get; set; }
    public string? InstanceUrl { get; set; }
}
