using System.Net;
using HtmlAgilityPack;

namespace LinkNeuvo.Client;

public class ClientResponse
{
    public ClientResponse(Uri uri, Uri? referrer, HttpMethod method)
    {
        Uri = uri;
        Referrer = referrer;
        Method = method;
    }

    public HtmlDocument? Html { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public bool IsSuccess => Html != null && StatusCode == HttpStatusCode.OK;
    public Uri Uri { get; }
    public Uri? Referrer { get; }
    public bool IsInternal { get; init; }
    public HttpMethod Method { get; }
}