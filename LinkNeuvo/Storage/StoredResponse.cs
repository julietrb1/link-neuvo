#pragma warning disable CS8618
namespace LinkNeuvo.Storage;

// ReSharper disable once ClassNeverInstantiated.Global
public class StoredResponse
{
    public string Uri { get; set; }
    public string? Referrer { get; set; }
    public string? StatusCode { get; set; }
    public bool IsInternal { get; set; }
    public string Method { get; set; }
}