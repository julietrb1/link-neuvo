namespace LinkNeuvo.Client;

public interface IFetcher
{
    Uri? ParseUri(string uri, Uri referrer);
    Task<ClientResponse> GetPage(Uri uri, Uri? referrer);
    bool IsLinkInternal(Uri uri);
}