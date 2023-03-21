namespace thsearch;

using HtmlAgilityPack;

class HtmlExtractor : IExtractor 
{

    public string FileIdentifier => ".html";

    public string Extract(string path) {

        // Load the HTML document from a file or a string
        HtmlDocument doc = new HtmlDocument();
        doc.Load(path);

        // Note: HtmlAgilityPack returns a lot of unnecessary white space and html escape sequences like "&nbsp;"
        return doc.DocumentNode.InnerText;
    }


}