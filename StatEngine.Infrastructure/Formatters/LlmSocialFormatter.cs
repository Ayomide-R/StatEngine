using Microsoft.SemanticKernel;
using StatEngine.Core.Interfaces;
using StatEngine.Core.Models;

namespace StatEngine.Infrastructure.Formatters;

public class LlmSocialFormatter : IDisplayFormatter
{
    private readonly Kernel _kernel;

    public LlmSocialFormatter(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<string> FormatAsync(StatUpdate update)
    {
        const string prompt = @"
        Take this statistics data and write a witty 280-character tweet.
        The audience likes tech and sports.
        Data: {{$input}}
        ";

        var result = await _kernel.InvokePromptAsync(prompt, new KernelArguments { ["input"] = update.RawContent });
        return result.ToString();
    }
}
