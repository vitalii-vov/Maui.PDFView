namespace Maui.PDFView.DataSources;

public class PdfTempFileHelper
{
    /// <summary>
    /// Creates a unique temporary file path for a PDF file.
    /// </summary>
    public static string CreateTempPdfFilePath()
    {
        return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".pdf");
    }
}