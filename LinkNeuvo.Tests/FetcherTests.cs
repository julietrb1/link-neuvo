using LinkNeuvo.Client;
using LinkNeuvo.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LinkNeuvo.Tests;

public class FetcherTests
{
    private Fetcher _fetcher = null!;

    [SetUp]
    public void Setup()
    {
        var mockCrawlerOptions = new Mock<IOptions<CrawlerOptions>>();
        mockCrawlerOptions.SetupGet(o => o.Value)
            .Returns(new CrawlerOptions { BaseUrl = new Uri("https://example.com") });
        var mockLogger = new Mock<ILogger<Fetcher>>();
        _fetcher = new Fetcher(mockCrawlerOptions.Object, mockLogger.Object);
    }

    [TestCase("http://example.com/hi", "https://example.com", "https://example.com/hi")]
    [TestCase("http://example.com/hi", "http://example.com", "https://example.com/hi")]
    [TestCase("//example.com/hi", "https://example.com", "https://example.com/hi")]
    [TestCase("/some-other-page", "https://example.com/other-again", "https://example.com/some-other-page")]
    [TestCase("/secure-me", "http://example.com/insec-referrer", "https://example.com/secure-me")]
    [TestCase("./im-relative", "http://example.com/dir/insec-referrer", "https://example.com/dir/im-relative")]
    [TestCase("./outside-relative", "http://outside.com/dir/insec-referrer", "http://outside.com/dir/outside-relative")]
    [TestCase("./outside-relative", "https://outside.com/dir/insec-referrer",
        "https://outside.com/dir/outside-relative")]
    [TestCase("../im-relative", "http://example.com/dir/insec-referrer", "https://example.com/im-relative")]
    [TestCase("https://another.com/hi", "https://example.com", "https://another.com/hi")]
    [TestCase("http://another.com/insec", "https://example.com", "http://another.com/insec")]
    public void ParseUri_InputValidString_ReturnsCorrectUri(string uriInputString, string referrerInputString,
        string expectedUriString)
    {
        Assert.That(_fetcher.ParseUri(uriInputString, new Uri(referrerInputString))?.ToString(),
            Is.EqualTo(expectedUriString));
    }
}