using HtmlAgilityPack;

namespace LinkNeuvo.Parsing;

public interface IPageParser
{
    IEnumerable<string> ExtractLinks(HtmlDocument doc);
}