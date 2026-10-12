using System.Net;

namespace JobFinder.Tests;

/// <summary>
/// Placeholder proving the red-green gate end to end: the
/// real HTTP pipeline answers through the application
/// boundary. T3+ replaces this with the full suite.
/// </summary>
public class SmokeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public SmokeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_Index_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
