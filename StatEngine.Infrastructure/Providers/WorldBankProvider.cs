using System.Text.Json;
using System.Net.Http.Json;
using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;

namespace StatEngine.Infrastructure.Providers;

public class WorldBankProvider : IStatProvider
{
    private readonly HttpClient _httpClient;
    public string Name => "WorldBank";

    public WorldBankProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StatUpdate> GetLatestAsync()
    {
        var url = "https://api.worldbank.org/v2/country/WLD/indicator/NY.GDP.MKTP.CD?format=json&per_page=1";
        var jsonDoc = await _httpClient.GetFromJsonAsync<JsonDocument>(url);

        if (jsonDoc == null || jsonDoc.RootElement.ValueKind != JsonValueKind.Array || jsonDoc.RootElement.GetArrayLength() < 2)
        {
            throw new Exception("Failed to fetch data from World Bank API or invalid response format.");
        }

        // World Bank API returns [metadata, [data]]
        var dataArray = jsonDoc.RootElement[1];
        if (dataArray.ValueKind != JsonValueKind.Array || dataArray.GetArrayLength() == 0)
        {
            throw new Exception("No data found in World Bank response.");
        }

        var data = dataArray[0];
        
        string countryValue = data.GetProperty("country").GetProperty("value").GetString() ?? "Unknown";
        string indicatorValue = data.GetProperty("indicator").GetProperty("value").GetString() ?? "Unknown";
        string indicatorId = data.GetProperty("indicator").GetProperty("id").GetString() ?? "Unknown";
        string countryId = data.GetProperty("country").GetProperty("id").GetString() ?? "Unknown";
        string date = data.GetProperty("date").GetString() ?? "Unknown";
        double? val = data.GetProperty("value").ValueKind == JsonValueKind.Number ? data.GetProperty("value").GetDouble() : null;

        return new StatUpdate
        {
            Id = $"WB_{indicatorId}_{countryId}_{date}",
            Source = Name,
            RawContent = $"Country: {countryValue}, Indicator: {indicatorValue}, Year: {date}, Value: {val}",
            Metadata = new Dictionary<string, object>
            {
                { "Country", countryValue },
                { "Indicator", indicatorValue },
                { "Year", date },
                { "Value", val ?? 0 }
            }
        };
    }
}
