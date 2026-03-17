using StatEngine.Core.Interfaces;
using StatEngine.Infrastructure.Providers;

namespace StatEngine.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly StatProviderFactory _providerFactory;
    private readonly ICache _cache;
    private readonly IDisplayFormatter _formatter;
    private readonly IBroadcaster _broadcaster;
    private readonly IImageGenerator _imageGenerator;

    public Worker(
        ILogger<Worker> logger,
        StatProviderFactory providerFactory,
        ICache cache,
        IDisplayFormatter formatter,
        IBroadcaster broadcaster,
        IImageGenerator imageGenerator)
    {
        _logger = logger;
        _providerFactory = providerFactory;
        _cache = cache;
        _formatter = formatter;
        _broadcaster = broadcaster;
        _imageGenerator = imageGenerator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StatEngine Worker starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting ingestion cycle...");

                // 1. Select a random provider
                var provider = _providerFactory.GetRandomProvider();
                _logger.LogInformation("Selected provider: {ProviderName}", provider.Name);

                // 2. Fetch latest data (Polly guarded via HttpClient in DI)
                var update = await provider.GetLatestAsync();
                
                // 3. Check if already posted
                if (await _cache.IsNewAsync(update.Id))
                {
                    // 4. Refine content via LLM
                    _logger.LogInformation("New content detected. Refining via LLM...");
                    update.RefinedContent = await _formatter.FormatAsync(update);

                    // 5. Generate image
                    _logger.LogInformation("Generating relatable image...");
                    update.ImagePath = await _imageGenerator.GenerateAsync(update);

                    // 6. Post to Twitter
                    _logger.LogInformation("Broadcasting to social platforms...");
                    await _broadcaster.PostAsync(update);

                    // 7. Mark as posted
                    await _cache.MarkAsPostedAsync(update.Id);
                    _logger.LogInformation("Cycle completed successfully.");
                }
                else
                {
                    _logger.LogInformation("No new content from {ProviderName}.", provider.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during ingestion cycle.");
            }

            // Run every 4 hours
            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }
}
