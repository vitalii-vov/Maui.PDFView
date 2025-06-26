using System.Reflection;

namespace Maui.PDFView.DataSources;

public class AssetPdfSource : IPdfSource
{
    private readonly string _assetFileName;

    public AssetPdfSource(string assetFileName)
    {
        _assetFileName = assetFileName;
    }

    public async Task<string> GetFilePathAsync()
    {
        await using Stream? stream = await FileSystem.OpenAppPackageFileAsync(_assetFileName);

        if (stream == null)
            throw new FileNotFoundException($"Asset file '{_assetFileName}' not found in app package.");

        var tempFile = PdfTempFileHelper.CreateTempPdfFilePath();
        await using var outStream = File.Create(tempFile);
        await stream.CopyToAsync(outStream);

        return tempFile;
    }
}