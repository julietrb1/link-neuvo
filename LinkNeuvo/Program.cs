using LinkNeuvo.Client;
using LinkNeuvo.Config;
using LinkNeuvo.Parsing;
using LinkNeuvo.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, builder) => { builder.SetBasePath(Directory.GetCurrentDirectory()); })
    .ConfigureServices((context, services) =>
    {
        services.AddOptions();

        services
            .Configure<CrawlerOptions>(context.Configuration.GetRequiredSection(nameof(CrawlerOptions)))
            .AddTransient<IPageParser, PageParser>()
            .AddTransient<ICrawler, Crawler>()
            .AddTransient<IFetcher, Fetcher>()
            .AddSingleton<IStorage, CsvStorage>();
    })
    .Build();

Console.WriteLine("Starting LinkNeuvo.");
var options = host.Services.GetService<IOptions<CrawlerOptions>>()?.Value;
var baseUrl = options?.BaseUrl;
if (baseUrl == null)
    throw new Exception("Base URL required, but not provided in app settings.");

host.Services.GetService<ICrawler>()!.StartCrawling(baseUrl);