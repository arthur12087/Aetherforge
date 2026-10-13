using Aetherforge.Domain;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public sealed class IntegrationClient
{
    private readonly string _url;
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(2) };
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public IntegrationClient(string url) { _url = url.TrimEnd('/'); }
    public async Task<IntegrationSnapshot> Read(bool heartbeat = false)
    {
        using var response = heartbeat ? await Http.PostAsync(_url + "/integration/idle/heartbeat", new StringContent("{}", Encoding.UTF8, "application/json")) : await Http.GetAsync(_url + "/integration/state");
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<IntegrationSnapshot>(await response.Content.ReadAsStringAsync(), Json)!;
    }
    public async Task<CommandReceipt> Command(string action, string itemId)
    {
        var command = new ItemCommand { OperationId = Guid.NewGuid().ToString("N"), ItemId = itemId };
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var response = await Http.PostAsync(_url + "/integration/" + action, new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json"));
                var text = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(JsonDocument.Parse(text).RootElement.GetProperty("message").GetString());
                return JsonSerializer.Deserialize<CommandReceipt>(text, Json)!;
            }
            catch (HttpRequestException) when (attempt == 0) { }
            catch (TaskCanceledException) when (attempt == 0) { }
        }
    }
}
