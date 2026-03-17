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
        if (string.IsNullOrEmpty(update.ImagePath))
        {
            await _twitterClient.Tweets.PublishTweetAsync(update.RefinedContent);
        }
        else
        {
            var bytes = await File.ReadAllBytesAsync(update.ImagePath);
            var uploadedImage = await _twitterClient.Upload.UploadBinaryAsync(bytes);
            
            await _twitterClient.Tweets.PublishTweetAsync(new Tweetinvi.Parameters.PublishTweetParameters(update.RefinedContent)
            {
                Medias = { uploadedImage }
            });
            
            // Optional: Clean up temp image
            try { File.Delete(update.ImagePath); } catch { }
        }
    }
}
