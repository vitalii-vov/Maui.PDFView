namespace Maui.PDFView.DataSources;

public interface IPdfSource
{
    Task<string> GetFilePathAsync();
}