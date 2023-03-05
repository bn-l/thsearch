namespace thsearch;

using System.IO;


// initialised with a path, using the path it gets the last modified time of the file

class FileModel {
    public DateTime LastModified;
    public string Path;

    public FileModel(string path) {
        this.Path = path;
        this.LastModified = File.GetLastWriteTime(path);
    }
}