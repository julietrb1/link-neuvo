using HtmlAgilityPack;

namespace LinkNeuvo.Parsing;

public class PageParser : IPageParser
{
    public IEnumerable<string> ExtractLinks(HtmlDocument doc)
    {
        return doc.DocumentNode
            .Descendants("a")
            .Select(n => n.GetAttributeValue("href", ""))
            .Where(n => !string.IsNullOrWhiteSpace(n));
    }
}