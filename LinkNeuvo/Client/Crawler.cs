using System.Collections.Concurrent;
using LinkNeuvo.Config;
using LinkNeuvo.Parsing;
using LinkNeuvo.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkNeuvo.Client;

public class Crawler : ICrawler
{
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly IFetcher _fetcher;
    private readonly ConcurrentBag<Uri> _linkSet;
    private readonly ILogger<Crawler> _logger;
    private readonly CrawlerOptions _options;
    private readonly IPageParser _parser;
    private readonly IStorage _storage;
    private readonly ConcurrentBag<Task> _tasks;

    public Crawler(IPageParser parser, IFetcher fetcher, ILogger<Crawler> logger, IStorage storage,
        IOptions<CrawlerOptions> options)
    {
        _parser = parser;
        _fetcher = fetcher;
        _logger = logger;
        _storage = storage;
        _linkSet = new ConcurrentBag<Uri>();
        _tasks = new ConcurrentBag<Task>();
        _options = options.Value;
        _concurrencySemaphore = new SemaphoreSlim(_options.MaxTasks);
    }

    public void StartCrawling(Uri uri)
    {
        AddTask(new Tuple<Uri, Uri?>(uri, null));
        while (_tasks.Any(t => !t.IsCompleted)) Task.WhenAll(_tasks).Wait();
        _storage.WriteResponses();
        _logger.LogInformation("All tasks finished");
    }

    private void AddTask(Tuple<Uri, Uri?> msg)
    {
        _tasks.Add(Task.Run(async () =>
        {
            var results = await CrawlLink(msg.Item1, msg.Item2);
            if (results == null) return;
            foreach (var result in results)
                AddTask(result);
        }));
    }

    private async Task<IEnumerable<Tuple<Uri, Uri?>>?> CrawlLink(Uri uri, Uri? referrer = null)
    {
        await _concurrencySemaphore.WaitAsync();

        try
        {
            if (_linkSet.Contains(uri))
            {
                _logger.LogDebug("Skipping {Uri} as it was already scanned", uri);
                return null;
            }

            _linkSet.Add(uri);
            _logger.LogDebug("Found: {Uri}", uri);
            var clientResponse = await _fetcher.GetPage(uri, referrer);
            _storage.RecordResponse(new StoredResponse
            {
                Uri = uri.ToString(), Referrer = referrer?.ToString(),
                StatusCode = clientResponse.StatusCode.ToString(),
                IsInternal = clientResponse.IsInternal,
                Method = clientResponse.Method.ToString()
            });
            if (!clientResponse.IsSuccess)
                return null;

            _logger.LogDebug("Referrer: {Uri}", uri);

            if (!clientResponse.IsInternal)
            {
                _logger.LogDebug("Skipping scanning links on {Uri} as it is external", uri);
                return null;
            }

            var links = _parser.ExtractLinks(clientResponse.Html!);
            GC.Collect();

            var result = links.Select(l => _fetcher.ParseUri(l, uri))
                .Where(l => l != null && (_fetcher.IsLinkInternal(l) || _options.FetchExternally))
                .Select(l => new Tuple<Uri, Uri?>(l!, uri));

            return result;
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }
}