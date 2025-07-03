using HoroScope.Models;
using System.Text.Json;

namespace HoroScope.Services
{
    public class NominatimService
    {
        private readonly HttpClient _client;

        public NominatimService(HttpClient client)
        {
            _client = client;
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("HoroScopeApp/1.0 (contact@example.com)");
        }

        public async Task<GeoLocation?> GetCoordinatesAsync(string cityName)
        {
            string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(cityName)}&format=json&limit=1";

            var response = await _client.GetStringAsync(url);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var results = JsonSerializer.Deserialize<NominatimResult[]>(response, options);

            if (results != null && results.Length > 0)
            {
                return new GeoLocation
                {
                    Latitude = double.Parse(results[0].Lat),
                    Longitude = double.Parse(results[0].Lon)
                };
            }
            return null;
        }
    }
}
