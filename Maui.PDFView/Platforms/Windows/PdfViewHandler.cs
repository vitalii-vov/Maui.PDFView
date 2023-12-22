using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Maui.PDFView.Platforms.Windows
{
    internal class PdfViewHandler : ViewHandler<IPdfView, TextBlock>
    {
        private readonly static PropertyMapper<PdfView, PdfViewHandler> PropertyMapper = new(ViewMapper)
        {
        };

        public PdfViewHandler() : base(PropertyMapper, null)
        {
        }

        protected override TextBlock CreatePlatformView()
        {
            return new TextBlock
            {
                Text = "PdfView is not implemented on the Windows platform",
            };
        }
    }
}
