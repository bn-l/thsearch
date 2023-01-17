<img src="Assets/icon.png" alt="logo" />

# thSearch 

### 👉NEW!!!

Multithreaded content search of files in specified folders. 

### Why?

I have a lot of text documents on my computer in various places (notes, bookmarks, pds, docxs, etc). I wanted *one* **blazing fast** ⚡ CLI command that would search just the locations and extensions I specify (like downloads, documents).

### Usage

Create a file called thsearch.txt in the same folder as thsearch.exe. In this file, on separate lines, use + to include paths (searches all sub directories) , - to exclude, and > to specify the extensions you want.

#### .\thsearch.txt 

```
+C:\User\Documents
-E:\Exclude\me\please
-node_modules
>.txt
>.md
>.pdf
>.docx
```

Then run 

```powershell
thsearch.exe <search term>
```

Tip: Add it to your path and anytime you want to search your notes, etc, just open a termnial and type "thsearch seatchterm"

### Perfomance

thSearch uses a producer consumer multithreaded approach where a single producer thread finds candidate files and passes these off to "consumer" threads which do the content searching. It will automatically use an ideal number of threads.

Grep / content searching is expensive so the first step is to not waste time on irrelevant files. The power of thSearch is that you search only specific extensions of documents and only in the locations where documents are likely to be.

Starting from the top and going line by line, the program will stop searching a file the moment it finds a match. So the deeper the term is in the file, the longer the time spent. 

I created ~30 gb of random words total across 10,000 files, nested three levels deep in 1,000 folders. The program thrashed my SSD, reading > 1.5 gb/s, and returned in seconds. Pretty cool

### Platform

Windows.

Soon: Linux (for now need to build it on linux manually)

