namespace MarketDataDomain.Tests;
using WireMock.Server;
using Xunit;

public class API.Tests : IDisposable
{
    private readonly WireMockServer _wireMockServer;

    public YourApiTests()
    {
        _wireMockServer = WireMockServer.Start();
    }

    [Fact]
    public async Task Test_YourApi_WithWireMock()
    {
        // Configure WireMock response and assertions
    }

    public void Dispose()
    {
        _wireMockServer.Stop();
    }
}
