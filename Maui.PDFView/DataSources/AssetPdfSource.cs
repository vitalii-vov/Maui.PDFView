using System.Reflection;

namespace Maui.PDFView.DataSources;

public class AssetPdfSource : IPdfSource
{
    private readonly string _resourcePath;

    public AssetPdfSource(string resourcePath)
    {
        _resourcePath = resourcePath;
    }

    public async Task<string> GetFilePathAsync()
    {
        var assembly = Assembly.GetEntryAssembly();
        byte[] bytes;
        await using (Stream stream = assembly.GetManifestResourceStream(_resourcePath))
        {
            bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
        }

        var tempFile = PdfTempFileHelper.CreateTempPdfFilePath();
        await File.WriteAllBytesAsync(tempFile, bytes);

        return tempFile;
    }
}