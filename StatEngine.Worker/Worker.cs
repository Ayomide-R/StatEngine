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

    public Worker(
        ILogger<Worker> logger,
        StatProviderFactory providerFactory,
        ICache cache,
        IDisplayFormatter formatter,
        IBroadcaster broadcaster)
    {
        _logger = logger;
        _providerFactory = providerFactory;
        _cache = cache;
        _formatter = formatter;
        _broadcaster = broadcaster;
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
                _logger.LogInformation("Fetched data: {Id}", update.Id);

                // 3. Check if already posted
                if (await _cache.IsNewAsync(update.Id))
                {
                    // 4. Refine content via LLM
                    _logger.LogInformation("New content detected. Refining via LLM...");
                    update.RefinedContent = await _formatter.FormatAsync(update);

                    // 5. Post to Twitter
                    _logger.LogInformation("Broadcasting refined content...");
                    await _broadcaster.PostAsync(update);

                    // 6. Mark as posted
                    await _cache.MarkAsPostedAsync(update.Id);
                    _logger.LogInformation("Cycle completed successfully.");
                }
                else
                {
                    _logger.LogInformation("Content {Id} already posted. Skipping.", update.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during ingestion cycle.");
            }

            // Every 4 hours as requested, but for demo/test maybe shorter? 
            // I'll stick to the request: "every 4 hours tasks" -> 4 hours = 14400000 ms
            // For testing purposes, I'll use 1 minute if not overridden, but I'll put 4 hours here.
            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }
}
