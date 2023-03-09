

// IExtractor is an interface that defines an extractor. It has a file identifier (usually an extension) and a method Extract which takes a file path and returns a string

interface IExtractor {
    string FileIdentifier { get; }
    string Extract(string path);
}
