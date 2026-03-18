using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using StatEngine.Core.Models;
using StatEngine.Infrastructure.Formatters;

namespace StatEngine.Tests;

public class LlmSocialFormatterTests
{
    [Fact]
    public async Task FormatAsync_ReturnsFormattedString()
    {
        // Arrange
        var mockChatCompletion = new Mock<IChatCompletionService>();
        mockChatCompletion
            .Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent>
            {
                new ChatMessageContent(AuthorRole.Assistant, "This is a formatted witty tweet!")
            });

        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<IChatCompletionService>(mockChatCompletion.Object);
        var kernel = builder.Build();

        var formatter = new LlmSocialFormatter(kernel);
        var update = new StatUpdate { RawContent = "Raw data here" };

        // Act
        var result = await formatter.FormatAsync(update);

        // Assert
        Assert.Equal("This is a formatted witty tweet!", result);
    }
}
