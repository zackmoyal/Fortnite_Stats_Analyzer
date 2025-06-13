using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FortniteStatsAnalyzer.Configuration;
using FortniteStatsAnalyzer.Models;
using FortniteStatsAnalyzer.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FortniteStatsAnalyzer.Tests;

public class FortniteApiServiceTests
{
    private class TestMessageHandler : HttpMessageHandler
    {
        public bool WasCalled { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task GetStatsForUser_EmptyUsername_ReturnsFailure()
    {
        var options = Options.Create(new FortniteApiSettings { ApiKey = "dummy" });
        var handler = new TestMessageHandler();
        var client = new HttpClient(handler);
        var service = new FortniteApiService(options, new NullLogger<FortniteApiService>(), client);

        var result = await service.GetStatsForUser(string.Empty);

        Assert.False(result?.Result);
        Assert.Equal("Username cannot be empty", result?.Error);
        Assert.False(handler.WasCalled);
    }
}
