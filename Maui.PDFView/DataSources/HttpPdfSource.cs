namespace Maui.PDFView.DataSources;

public class HttpPdfSource: IPdfSource
{
    private readonly string _url;

    public HttpPdfSource(string url)
    {
        _url = url;
    }

    public async Task<string> GetFilePathAsync()
    {
        var tempFile = PdfTempFileHelper.CreateTempPdfFilePath();
        using var client = new HttpClient();
        var stream = await client.GetStreamAsync(_url);
        await using var fileStream = File.Create(tempFile);
        await stream.CopyToAsync(fileStream);
        return tempFile;
    }
}