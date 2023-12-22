namespace Maui.PDFView
{
    public static class AppBuilderExtensions
    {
        public static MauiAppBuilder UseMauiPdfView(this MauiAppBuilder builder)
        {
            builder.ConfigureMauiHandlers((handlers) =>
            {
#if ANDROID
                handlers.AddHandler(typeof(PdfView), typeof(Platforms.Android.PdfViewHandler));
#elif IOS
                handlers.AddHandler(typeof(PdfView), typeof(Platforms.iOS.PdfViewHandler));
#elif MACCATALYST
                handlers.AddHandler(typeof(PdfView), typeof(Platforms.MacCatalyst.PdfViewHandler));
#elif WINDOWS
                handlers.AddHandler(typeof(PdfView), typeof(Platforms.Windows.PdfViewHandler));
#endif
            });
            return builder;
        }
    }
}
