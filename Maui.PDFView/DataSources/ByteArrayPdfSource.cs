namespace Maui.PDFView.DataSources;

public class ByteArrayPdfSource : IPdfSource
{
    private readonly byte[] _pdfBytes;

    public ByteArrayPdfSource(byte[] pdfBytes)
    {
        _pdfBytes = pdfBytes;
    }

    public async Task<string> GetFilePathAsync()
    {
        var tempFile = PdfTempFileHelper.CreateTempPdfFilePath();
        await File.WriteAllBytesAsync(tempFile, _pdfBytes);
        return tempFile;
    }
}