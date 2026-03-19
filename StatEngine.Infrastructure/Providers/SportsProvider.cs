using System.ServiceModel.Syndication;
using System.Xml;
using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;

namespace StatEngine.Infrastructure.Providers;

public class SportsProvider : IStatProvider
{
    private readonly HttpClient _httpClient;
    public string Name => "ESPN_Sports";

    public SportsProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StatUpdate> GetLatestAsync()
    {
        // Public ESPN RSS Feed
        var url = "https://www.espn.com/espn/rss/news";
        
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = XmlReader.Create(stream);
        var feed = SyndicationFeed.Load(reader);
        
        var latestItem = feed.Items.FirstOrDefault();
        if (latestItem == null)
        {
            throw new Exception("No sports news found in ESPN RSS feed.");
        }

        return new StatUpdate
        {
            Id = $"ESPN_{latestItem.Id ?? latestItem.Title.Text.GetHashCode().ToString()}",
            Source = Name,
            RawContent = $"{latestItem.Title.Text}: {latestItem.Summary?.Text ?? "Check the latest highlights."}",
            Metadata = new Dictionary<string, object>
            {
                { "Title", latestItem.Title.Text },
                { "PublishDate", latestItem.PublishDate.DateTime },
                { "Link", latestItem.Links.FirstOrDefault()?.Uri.ToString() ?? "" }
            }
        };
    }
}
