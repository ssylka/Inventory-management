using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Inventory_Managment.Services
{
    public class SalesforceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public SalesforceService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }
        private async Task<(string accessToken, string instanceUrl)> GetTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _config["Salesforce:ClientId"]!,
                ["client_secret"] = _config["Salesforce:ClientSecret"]!
            });

            var response = await client.PostAsync(
                $"{_config["Salesforce:LoginUrl"]}/services/oauth2/token", body);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Salesforce auth failed: {json}");

            var doc = JsonDocument.Parse(json);
            var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
            var instanceUrl = doc.RootElement.GetProperty("instance_url").GetString()!;

            return (accessToken, instanceUrl);
        }

        public async Task CreateAccountAndContactAsync(
            string name, string email, string phone, string title)
        {
            var (token, instanceUrl) = await GetTokenAsync();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var apiBase = $"{instanceUrl}/services/data/v57.0/sobjects";

            // Create Account
            var accountBody = JsonSerializer.Serialize(new { Name = $"{name}'s Account" });
            var accountResponse = await client.PostAsync(
                $"{apiBase}/Account/",
                new StringContent(accountBody, Encoding.UTF8, "application/json"));

            var accountJson = await accountResponse.Content.ReadAsStringAsync();
            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception($"Account creation failed: {accountJson}");

            var accountId = JsonDocument.Parse(accountJson)
                .RootElement.GetProperty("id").GetString()!;

            var contactBody = JsonSerializer.Serialize(new
            {
                LastName = name,
                Email = email,
                Phone = phone,
                Title = title,
                AccountId = accountId
            });

            var contactResponse = await client.PostAsync(
                $"{apiBase}/Contact/",
                new StringContent(contactBody, Encoding.UTF8, "application/json"));

            var contactJson = await contactResponse.Content.ReadAsStringAsync();
            if (!contactResponse.IsSuccessStatusCode)
                throw new Exception($"Contact creation failed: {contactJson}");
        }
    }
}