using LiteDB;
using StatEngine.Core.Interfaces;

namespace StatEngine.Infrastructure.Persistence;

public class LiteDbCache : ICache
{
    private readonly string _connectionString;
    private const string CollectionName = "posted_stats";

    public LiteDbCache(string dbName = "stats.db")
    {
        _connectionString = dbName;
    }

    public async Task<bool> IsNewAsync(string id)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<PostedStat>(CollectionName);
            return !collection.Exists(x => x.Id == id);
        });
    }

    public async Task MarkAsPostedAsync(string id)
    {
        await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<PostedStat>(CollectionName);
            collection.Insert(new PostedStat { Id = id, PostedAt = DateTime.UtcNow });
            collection.EnsureIndex(x => x.Id);
        });
    }

    private class PostedStat
    {
        public string Id { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
    }
}
