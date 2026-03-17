using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;
using Microsoft.Extensions.Logging;

namespace StatEngine.Infrastructure.Broadcasters;

public class FileDraftBroadcaster : IBroadcaster
{
    private readonly ILogger<FileDraftBroadcaster> _logger;
    private readonly string _draftsFolder;

    public FileDraftBroadcaster(ILogger<FileDraftBroadcaster> logger)
    {
        _logger = logger;
        _draftsFolder = Path.Combine(Directory.GetCurrentDirectory(), "drafts");
    }

    public async Task PostAsync(StatUpdate update)
    {
        var draftPath = Path.Combine(_draftsFolder, update.Id);
        if (!Directory.Exists(draftPath))
        {
            Directory.CreateDirectory(draftPath);
        }

        // Save Tweet Text
        var textPath = Path.Combine(draftPath, "tweet.txt");
        await File.WriteAllTextAsync(textPath, update.RefinedContent);

        // Save Image if exists
        if (!string.IsNullOrEmpty(update.ImagePath) && File.Exists(update.ImagePath))
        {
            var targetImagePath = Path.Combine(draftPath, "image.jpg");
            File.Copy(update.ImagePath, targetImagePath, true);
            _logger.LogInformation("Draft saved with image at: {Path}", draftPath);
        }
        else
        {
            _logger.LogInformation("Draft saved (text only) at: {Path}", draftPath);
        }

        _logger.LogInformation("Manual Action Required: Check {Path} to post your tweet!", draftPath);
    }
}
