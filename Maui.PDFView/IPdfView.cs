namespace Maui.PDFView
{
    internal interface IPdfView : IView
    {
        string Uri { get; set; }
        bool IsHorizontal { get; set; }
    }
}
