using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;
using Tweetinvi;

namespace StatEngine.Infrastructure.Broadcasters;

public class TwitterBroadcaster : IBroadcaster
{
    private readonly ITwitterClient _twitterClient;

    public TwitterBroadcaster(ITwitterClient twitterClient)
    {
        _twitterClient = twitterClient;
    }

    public async Task PostAsync(StatUpdate update)
    {
        // Reverting to v1.1 stable call for verification of credentials
        await _twitterClient.Tweets.PublishTweetAsync(update.RefinedContent);
    }
}
