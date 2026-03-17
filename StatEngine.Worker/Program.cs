using Serilog;
using StatEngine.Core.Interfaces;
using StatEngine.Infrastructure.Broadcasters;
using StatEngine.Infrastructure.Formatters;
using StatEngine.Infrastructure.Persistence;
using StatEngine.Infrastructure.Providers;
using StatEngine.Worker;
using Microsoft.SemanticKernel;
using Tweetinvi;
using Polly;
using Polly.Extensions.Http;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddSerilog();

    // Infrastructure
    builder.Services.AddSingleton<ICache, LiteDbCache>(_ => new LiteDbCache("stats.db"));
    
    // Providers: WorldBank + Multi-Domain (Education, Fashion, etc.)
    builder.Services.AddSingleton<IStatProvider, WorldBankProvider>();
    builder.Services.AddSingleton<IStatProvider>(sp => new MultiDomainProvider("Education", sp.GetRequiredService<HttpClient>()));
    builder.Services.AddSingleton<IStatProvider>(sp => new MultiDomainProvider("Fashion", sp.GetRequiredService<HttpClient>()));
    builder.Services.AddSingleton<IStatProvider>(sp => new MultiDomainProvider("Lifestyle", sp.GetRequiredService<HttpClient>()));
    builder.Services.AddSingleton<IStatProvider>(sp => new MultiDomainProvider("Religion", sp.GetRequiredService<HttpClient>()));
    builder.Services.AddSingleton<IStatProvider>(sp => new MultiDomainProvider("Migration", sp.GetRequiredService<HttpClient>()));

    builder.Services.AddSingleton<StatProviderFactory>();
    builder.Services.AddSingleton<IDisplayFormatter, LlmSocialFormatter>();
    builder.Services.AddSingleton<IBroadcaster, TwitterBroadcaster>();

    // Semantic Kernel
    builder.Services.AddSingleton(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var apiKey = Environment.GetEnvironmentVariable("STATENGINE_LLM_API_KEY") ?? 
                     Environment.GetEnvironmentVariable("STATENGINE_OPENAI_API_KEY") ?? 
                     config["OpenAI:ApiKey"];
        
        var modelId = Environment.GetEnvironmentVariable("STATENGINE_LLM_MODEL_ID") ?? 
                      Environment.GetEnvironmentVariable("STATENGINE_OPENAI_MODEL_ID") ?? 
                      config["OpenAI:ModelId"] ?? "gpt-4o-mini";

        var endpoint = Environment.GetEnvironmentVariable("STATENGINE_LLM_ENDPOINT") ?? 
                       config["OpenAI:Endpoint"];

        var kernelBuilder = Kernel.CreateBuilder();
        if (!string.IsNullOrEmpty(apiKey))
        {
            if (!string.IsNullOrEmpty(endpoint))
            {
                // Support for OpenRouter, Groq, MiniMax, etc. via OpenAI connector
                kernelBuilder.AddOpenAIChatCompletion(modelId, apiKey, endpoint: new Uri(endpoint));
            }
            else
            {
                kernelBuilder.AddOpenAIChatCompletion(modelId, apiKey);
            }
        }
        return kernelBuilder.Build();
    });

    // Twitter Client (Tweetinvi)
    builder.Services.AddSingleton<ITwitterClient>(sp => 
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var consumerKey = Environment.GetEnvironmentVariable("STATENGINE_TWITTER_CONSUMER_KEY") ?? config["Twitter:ConsumerKey"];
        var consumerSecret = Environment.GetEnvironmentVariable("STATENGINE_TWITTER_CONSUMER_SECRET") ?? config["Twitter:ConsumerSecret"];
        var accessToken = Environment.GetEnvironmentVariable("STATENGINE_TWITTER_ACCESS_TOKEN") ?? config["Twitter:AccessToken"];
        var accessSecret = Environment.GetEnvironmentVariable("STATENGINE_TWITTER_ACCESS_SECRET") ?? config["Twitter:AccessSecret"];

        return new TwitterClient(
            consumerKey ?? string.Empty,
            consumerSecret ?? string.Empty,
            accessToken ?? string.Empty,
            accessSecret ?? string.Empty);
    });

    // HTTP Client with Polly
    void AddResilientClient<T>(IServiceCollection services) where T : class
    {
        services.AddHttpClient<T>()
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    }

    AddResilientClient<WorldBankProvider>(builder.Services);
    AddResilientClient<MultiDomainProvider>(builder.Services);

    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
