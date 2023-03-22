namespace thsearch;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

// PdfExtractor implements the IExtractor interface. Its file identifier is "pdf". 

class PdfExtractor : IExtractor 
{
    public string FileIdentifier => ".pdf";

    public string Extract(string path) {

        string fullText = "";

        using (var document = PdfDocument.Open(path))
        {
            
            foreach (var page in document.GetPages())
            {
                fullText += ContentOrderTextExtractor.GetText(page);
            }

            return fullText;
        }


    }
}