// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace LinkNeuvo.Config;

public class CrawlerOptions
{
    public Uri? BaseUrl { get; set; }
    public string? CsvOutputPath { get; set; }
    public int FlushCsvAtLineCount { get; set; }
    public int MaxTasks { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool FetchExternally { get; set; }
    public string[]? CrawlExtensions { get; set; }
    public bool CrawlNoExtension { get; set; }
}