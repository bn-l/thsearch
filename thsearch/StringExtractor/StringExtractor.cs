namespace thsearch;


class StringExtractor
{
    private readonly IExtractor[] extractors;

    private readonly IExtractor defaultExtractor;


    public StringExtractor(IExtractor[] extractors, IExtractor defaultExtractor)
    {
        this.extractors = extractors;
        this.defaultExtractor = defaultExtractor;
    }


    public string Extract(string path, string fileIdentifier)
    {

        // Iterates over this.extractors calling the extractor's extract method where fileIdentifier == extractor.FileIdentifier
        foreach (var extractor in this.extractors)
        {
            if (extractor.FileIdentifier == fileIdentifier)
            {
                return extractor.Extract(path);
            }
        }

        // try defaultExtractor, and if that's null throw an exception 
        return this.defaultExtractor?.Extract(path) ?? throw new System.Exception("No extractor found for file type: " + fileIdentifier);
    
    }

}