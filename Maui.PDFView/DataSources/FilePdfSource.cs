namespace Maui.PDFView.DataSources;

public class FilePdfSource : IPdfSource
{
    private readonly string _filePath;

    public FilePdfSource(string filePath)
    {
        _filePath = filePath;
    }

    public Task<string> GetFilePathAsync()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("File not found", _filePath);
        return Task.FromResult(_filePath);
    }
}