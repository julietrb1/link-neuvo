using System.Net.Mime;
using HtmlAgilityPack;
using LinkNeuvo.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkNeuvo.Client;

public class Fetcher : IFetcher
{
    private readonly Uri _baseUrl;
    private readonly HttpClient _client;
    private readonly ILogger<Fetcher> _logger;
    private readonly CrawlerOptions _options;

    public Fetcher(IOptions<CrawlerOptions> options, ILogger<Fetcher> logger)
    {
        _client = new HttpClient();
        _options = options.Value;
        _client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:101.0) Gecko/20100101 Firefox/101.0");
        _logger = logger;
        _baseUrl = _options.BaseUrl ??
                   throw new InvalidOperationException("Base URL required, but was null.");
    }

    public async Task<ClientResponse> GetPage(Uri uri, Uri? referrer)
    {
        Func<string, bool> endsWithLowerPath = uri.AbsolutePath.ToLowerInvariant().EndsWith;
        var isCrawlExtension = _options.CrawlExtensions?.Any(endsWithLowerPath) == true;
        var hasExtension = uri.AbsolutePath.Length >= 5 && uri.AbsolutePath[^5..^2].Contains('.');
        var shouldGet = isCrawlExtension || (_options.CrawlNoExtension && !hasExtension);
        var method = shouldGet ? HttpMethod.Get : HttpMethod.Head;
        HttpResponseMessage response;
        try
        {
            response = await _client.SendAsync(new HttpRequestMessage(method, uri));
        }
        catch (Exception e)
        {
            _logger.LogError("Got {Type} for {Uri}: {Message}", e.GetType(), uri, e.Message);
            return new ClientResponse(uri, referrer, method);
        }

        var isInternal = IsLinkInternal(uri);
        _logger.Log(response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Warning,
            "{Method} => {StatusCode} from {Uri} (REF {Referrer}){WillFollow}", method, response.StatusCode, uri,
            referrer,
            isInternal ? " (FOL)" : string.Empty);
        if (!shouldGet || response.Content.Headers.ContentType?.MediaType != MediaTypeNames.Text.Html)
        {
            _logger.LogDebug("Skipped {Uri} as the response was ineligible", uri);
            return new ClientResponse(uri, referrer, method) { StatusCode = response.StatusCode };
        }

        var html = new HtmlDocument();
        html.LoadHtml(await response.Content.ReadAsStringAsync());
        return new ClientResponse(uri, referrer, method)
            { Html = html, StatusCode = response.StatusCode, IsInternal = isInternal };
    }

    public Uri? ParseUri(string uriString, Uri referrer)
    {
        if (string.IsNullOrWhiteSpace(uriString))
        {
            _logger.LogError("Got null or whitespace URI");
            return null;
        }

        try
        {
            if (uriString.StartsWith("//")) return new Uri($"{_baseUrl.Scheme}:{uriString}");
            if (uriString.StartsWith("/"))
                return new Uri(_baseUrl, uriString);

            if (uriString.StartsWith("./"))
            {
                var leftPart = IsLinkInternal(referrer)
                    ? _baseUrl
                    : new Uri(referrer.GetLeftPart(UriPartial.Authority));
                var rightPart = string.Join("", referrer.Segments.SkipLast(1)) + uriString[2..];
                return new Uri(leftPart, rightPart);
            }

            var uri = new Uri(referrer, uriString);

            if (IsLinkInternal(uri))
                return new Uri(_baseUrl, uri.AbsolutePath);

            return uriString.StartsWith("http") ? new Uri(uriString) : null;
        }
        catch (Exception)
        {
            _logger.LogError("Failed to parse URI {} from referrer {}", uriString, referrer);
            return null;
        }
    }

    public bool IsLinkInternal(Uri uri)
    {
        var replacedHost = uri.Host.StartsWith("www.") ? uri.Host[4..] : uri.Host;
        _logger.LogDebug("Comparing {Replaced} with {Base}", replacedHost, _baseUrl.Host);
        return replacedHost == _baseUrl.Host;
    }
}