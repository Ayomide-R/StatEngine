using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;
using Microsoft.Extensions.Logging;

namespace StatEngine.Infrastructure.Providers;

public class PollinationsImageGenerator : IImageGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PollinationsImageGenerator> _logger;

    public PollinationsImageGenerator(HttpClient httpClient, ILogger<PollinationsImageGenerator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GenerateAsync(StatUpdate update)
    {
        try
        {
            // Create a clean, concise prompt for the image generator
            // Remove hashtags, mentions, and keep it under 100 chars
            var cleanPrompt = System.Text.RegularExpressions.Regex.Replace(update.RefinedContent, @"[#@]\w+", "").Trim();
            cleanPrompt = System.Text.RegularExpressions.Regex.Replace(cleanPrompt, @"[^\w\s]", "");
            
            var prompt = cleanPrompt.Length > 100 
                ? cleanPrompt.Substring(0, 100) 
                : cleanPrompt;
            
            _logger.LogInformation("Generating image with prompt: {Prompt}", prompt);
            
            // Encode for URL
            var encodedPrompt = Uri.EscapeDataString(prompt);
            var imageUrl = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width=1024&height=512&nologo=true";

            var response = await _httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode) return null;

            var fileName = $"img_{update.Id}.jpg";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "temp_media", fileName);
            
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(filePath, bytes);

            return filePath;
        }
        catch (Exception)
        {
            return null; // Fail gracefully
        }
    }
}
