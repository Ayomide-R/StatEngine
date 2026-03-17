using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;

namespace StatEngine.Infrastructure.Providers;

/// <summary>
/// A flexible provider that handles diverse topics like Education, Fashion, Lifestyle, etc.
/// This can be configured with specific API endpoints or used as a template for new domains.
/// </summary>
public class MultiDomainProvider : IStatProvider
{
    private readonly string _topic;
    private readonly HttpClient _httpClient;

    public string Name => $"DomainPod_{_topic}";

    public MultiDomainProvider(string topic, HttpClient httpClient)
    {
        _topic = topic;
        _httpClient = httpClient;
    }

    public async Task<StatUpdate> GetLatestAsync()
    {
        // For demonstration, we simulate some data for diverse domains.
        // In a production scenario, you would add specialized endpoints for each topic.
        
        var randomValue = new Random().Next(10, 500);
        var content = _topic switch
        {
            "Education" => $"Global literacy rate in youth reaches {randomValue}% according to UNESCO projections.",
            "Fashion" => $"Sustainability in fashion: {randomValue}% of luxury brands now use recycled fabrics.",
            "Lifestyle" => $"Health trends: {randomValue} million people are now practicing daily mindfulness globally.",
            "Religion" => $"Cultural stats: {randomValue} historical sites preserved under global heritage funds this year.",
            "Migration" => $"Global mobility: {randomValue} specialized visas issued for tech workers this quarter.",
            _ => $"General Stat for {_topic}: {randomValue} units recorded."
        };

        return new StatUpdate
        {
            Id = $"MTP_{_topic}_{DateTime.UtcNow:yyyyMMdd_HHmm}",
            Source = Name,
            RawContent = content,
            Metadata = new Dictionary<string, object>
            {
                { "Topic", _topic },
                { "Timestamp", DateTime.UtcNow },
                { "Value", randomValue }
            }
        };
    }
}
