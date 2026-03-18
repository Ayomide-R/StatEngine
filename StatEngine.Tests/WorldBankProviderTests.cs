using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using StatEngine.Infrastructure.Providers;

namespace StatEngine.Tests;

public class WorldBankProviderTests
{
    [Fact]
    public async Task GetLatestAsync_ReturnsValidStatUpdate()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var jsonResponse = @"[
            { ""page"": 1, ""pages"": 1, ""per_page"": 1, ""total"": 1, ""sourceid"": ""2"", ""lastupdated"": ""2023-12-18"" },
            [
                {
                    ""indicator"": { ""id"": ""NY.GDP.MKTP.CD"", ""value"": ""GDP (current US$)"" },
                    ""country"": { ""id"": ""1W"", ""value"": ""World"" },
                    ""countryiso3code"": ""WLD"",
                    ""date"": ""2022"",
                    ""value"": 100562000000000,
                    ""unit"": """",
                    ""obs_status"": """",
                    ""decimal"": 0
                }
            ]
        ]";

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new WorldBankProvider(httpClient);

        // Act
        var result = await provider.GetLatestAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WorldBank", result.Source);
        Assert.Contains("NY.GDP.MKTP.CD", result.Id);
        Assert.Contains("1W", result.Id);
        Assert.Contains("2022", result.Id);
        Assert.Contains("Country: World", result.RawContent);
    }
}
