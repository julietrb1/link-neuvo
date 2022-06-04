using System.Collections.Concurrent;
using System.Globalization;
using CsvHelper;
using LinkNeuvo.Config;
using Microsoft.Extensions.Options;

namespace LinkNeuvo.Storage;

public class CsvStorage : IStorage
{
    private readonly CsvWriter _csvWriter;
    private readonly ConcurrentBag<StoredResponse> _responses;

    public CsvStorage(IOptions<CrawlerOptions> options)
    {
        var outputPath = options.Value.CsvOutputPath ??
                         throw new InvalidOperationException(
                             "CSV output path required, but not provided.");
        var streamWriter = new StreamWriter(outputPath, false);
        _csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        _csvWriter.WriteHeader<StoredResponse>();
        _csvWriter.NextRecord();
        _responses = new ConcurrentBag<StoredResponse>();
    }

    public void RecordResponse(StoredResponse clientResponse)
    {
        _responses.Add(clientResponse);
    }

    public void WriteResponses()
    {
        _csvWriter.WriteRecords(_responses);
    }
}