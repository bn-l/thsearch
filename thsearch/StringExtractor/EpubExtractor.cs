namespace thsearch;

using EpubSharp;

class EpubExtractor : IExtractor 
{
    public string FileIdentifier => ".epub";

    public string Extract(string path) {
        
        return EpubReader.Read(path).ToPlainText();

    }
}