using System.Net.Http.Json;
using System.Text.Json;
using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;

namespace StatEngine.Infrastructure.Providers;

public class CryptoProvider : IStatProvider
{
    private readonly HttpClient _httpClient;
    public string Name => "CoinGecko_Crypto";

    public CryptoProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // CoinGecko requires a User-Agent or it might block
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "StatEngine/2.0");
        }
    }

    public async Task<StatUpdate> GetLatestAsync()
    {
        // Simple public endpoint for top 3 coins: Bitcoin, Ethereum, Solana
        var url = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin,ethereum,solana&vs_currencies=usd&include_24hr_change=true";
        
        var data = await _httpClient.GetFromJsonAsync<Dictionary<string, Dictionary<string, double>>>(url);

        if (data == null || !data.Any())
        {
            throw new Exception("Failed to fetch crypto data from CoinGecko.");
        }

        // Randomly pick one of the coins to report on
        var coins = data.Keys.ToList();
        var selectedId = coins[new Random().Next(coins.Count)];
        var stats = data[selectedId];
        
        var price = stats["usd"];
        var change = stats["usd_24h_change"];
        var direction = change >= 0 ? "up" : "down";

        return new StatUpdate
        {
            Id = $"CG_{selectedId}_{DateTime.UtcNow:yyyyMMdd_HHmm}",
            Source = Name,
            RawContent = $"{selectedId.ToUpper()} is currently trading at ${price:N2} USD, {direction} {Math.Abs(change):F2}% in the last 24 hours.",
            Metadata = new Dictionary<string, object>
            {
                { "Asset", selectedId },
                { "Price", price },
                { "Change24h", change },
                { "Timestamp", DateTime.UtcNow }
            }
        };
    }
}
