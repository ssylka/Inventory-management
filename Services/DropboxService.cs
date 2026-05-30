using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Inventory_Managment.Services
{
    public class DropboxService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public DropboxService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task UploadTicketAsync(object ticketData)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _config["Dropbox:AccessToken"];
            var folder = _config["Dropbox:UploadFolder"];

            var fileName = $"ticket_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = $"{folder}/{fileName}";
            var json = JsonSerializer.Serialize(ticketData, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // for cyrillic and special chars
            });

            var apiArg = JsonSerializer.Serialize(new
            {
                path = filePath,
                mode = "add",
                autorename = true
            });

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://content.dropboxapi.com/2/files/upload");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("Dropbox-API-Arg", apiArg);
            request.Content = new StringContent(json, Encoding.UTF8);
            request.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Dropbox upload failed: {result}");
        }
    }
}