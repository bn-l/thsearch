<img src="thsearch/Assets/logo2-1024.png" alt="logo" />

# thsearch 

### 👉NEW!!!

Multithreaded, just in time indexed, content search of files in specified folders. Like grep but faster and with the search results ranked.

### Why?

I have a lot of text documents on my computer in various places (notes, bookmarks, pdfs, docxs, html, etc). I wanted *one* **fast** ⚡ CLI command that would just grep (content search) the locations and extensions I specify (like downloads, documents, *.txt).

### Usage

Create a txt file in the same location as thsearch.exe. This file can be name "thsearch.txt", which is the default search configuration and/or a custom name, like "books.txt"

In this file, on separate lines:

- \~ will specifiy the location of the sqlite database file created for the index (optional)
- \+ will include paths (searches all sub directories) ,
- \- to exclude,
- \# to exlcude a path if it contains this word
- \> to specify the extensions you want.

#### .\thsearch.txt 

```
+C:\User\Documents
-E:\Exclude\me\please
#node_modules
>.txt
>.md
>.pdf
>.docx
>.html
```

CLI:

```powershell
thsearch.exe <search string> [config file] [all]
```

- `search string`: the search query (enclosed in qoutes if more than one word)
- `config file`: (optional) If specified looks for a txt file with this name in the same directory as the executable
- `all`: (optional) Show all search results (shows 10 by default)



For example to search in books:

```powershell
thsearch wooster books
```



### Supported Formats

thSearch uses various dotnet libraries to extract just the text from various formats. It supports:

- epub, pdfs, html, plain text, mark down, mhtml



Tip 1: Add it to your path and any time you want to search your notes, etc, just open a terminal and type "thsearch searchterm"

Tip 2: Add it to windows antimalware exclusions (can build it from source—no trust needed) to increase performance.

### Details

Grep / content searching is expensive so the first step is to not waste time on irrelevant files. The power of thSearch is that you search only specific extensions and and locations.

The just-in-time indexing uses multiple threads, and a producer and consumer design to extract and stem documents so that, even for many large files, the indexing performance is acceptable. It also means that a background indexing task doesn't need to be maintained. The drawbacks are that initial searches and searches after many changes will be slow. Versus the speed of grep this is acceptable.

The stemming logic is written from scratch and uses no libraries. It makes use of spans to increase performance. 

### Platform

Windows.

Soon: Linux (for now need to build it on Linux manually)



