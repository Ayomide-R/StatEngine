using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;

namespace StatEngine.Infrastructure.Providers;

public class PollinationsImageGenerator : IImageGenerator
{
    private readonly HttpClient _httpClient;

    public PollinationsImageGenerator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GenerateAsync(StatUpdate update)
    {
        try
        {
            // Create a clean prompt from the refined content
            var prompt = update.RefinedContent.Length > 200 
                ? update.RefinedContent.Substring(0, 200) 
                : update.RefinedContent;
            
            // Encode the prompt for the URL
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
