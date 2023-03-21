
// TxtExtractor implements the IExtractor interface. Its file identifier is "txt". 

class TxtExtractor : IExtractor 
{
    public string FileIdentifier => ".txt";

    public string Extract(string path) {
        return File.ReadAllText(path);
    }
}