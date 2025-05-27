namespace Maui.PDFView.DataSources;

public class ByteArrayPdfDataSource : IPdfSource
{
    private readonly byte[] _pdfBytes;

    public ByteArrayPdfDataSource(byte[] pdfBytes)
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