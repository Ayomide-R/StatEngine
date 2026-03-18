using StatEngine.Infrastructure.Persistence;

namespace StatEngine.Tests;

public class LiteDbCacheTests : IDisposable
{
    private readonly string _dbName;
    private readonly LiteDbCache _cache;

    public LiteDbCacheTests()
    {
        _dbName = $"test_stats_{Guid.NewGuid()}.db";
        _cache = new LiteDbCache(_dbName);
    }

    [Fact]
    public async Task IsNewAsync_ReturnsTrueForNewId()
    {
        var id = "test_id_1";
        var isNew = await _cache.IsNewAsync(id);
        Assert.True(isNew);
    }

    [Fact]
    public async Task MarkAsPosted_ThenIsNew_ReturnsFalse()
    {
        var id = "test_id_2";
        await _cache.MarkAsPostedAsync(id);
        var isNew = await _cache.IsNewAsync(id);
        Assert.False(isNew);
    }

    public void Dispose()
    {
        if (File.Exists(_dbName))
        {
            try
            {
                File.Delete(_dbName);
            }
            catch
            {
                // Ignored in test cleanup
            }
        }
    }
}
