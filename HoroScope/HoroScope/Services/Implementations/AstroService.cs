using HoroScope.Models;
using System.Text.Json;

public class AstroService
{
    private readonly HttpClient _client;

    public AstroService(HttpClient client)
    {
        _client = client;
    }
    public async Task<AstroResponse?> GetAstroDataAsync(DateTime birthDateTime, double lat, double lon)

    {
        string date = birthDateTime.ToString("yyyy-MM-dd");
        string time = birthDateTime.ToString("HH:mm");

        string url = $"api/astro?date={date}&time={time}&lat={lat}&lon={lon}";

        var response = await _client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<AstroResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
