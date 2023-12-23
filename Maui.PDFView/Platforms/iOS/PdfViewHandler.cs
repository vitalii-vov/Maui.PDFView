using Foundation;
using Microsoft.Maui.Handlers;
using PdfKit;
using UIKit;

namespace Maui.PDFView.Platforms.iOS
{
    internal class PdfViewHandler : ViewHandler<IPdfView, PdfKit.PdfView>
    {
        private readonly static PropertyMapper<PdfView, PdfViewHandler> PropertyMapper = new(ViewMapper)
        {
            [nameof(IPdfView.Uri)] = MapUri,
            [nameof(IPdfView.IsHorizontal)] = MapIsHorizontal,
        };

        private string _fileName;

        public PdfViewHandler() : base(PropertyMapper, null)
        {
        }

        static void MapUri(PdfViewHandler handler, IPdfView pdfView)
        {
            handler._fileName = pdfView.Uri;
            handler.RenderPages();
        }

        static void MapIsHorizontal(PdfViewHandler handler, IPdfView pdfView)
        {
            handler.PlatformView.DisplayDirection = pdfView.IsHorizontal
                                        ? PdfDisplayDirection.Horizontal
                                        : PdfDisplayDirection.Vertical;
        }

        protected override PdfKit.PdfView CreatePlatformView()
        {
            return new PdfKit.PdfView();
        }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            RenderPages();
            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }

        void RenderPages()
        {
            if (_fileName == null)
                return;

            PlatformView.Document = new PdfDocument(NSData.FromFile(_fileName));

            PlatformView.AutosizesSubviews = true;
            PlatformView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleBottomMargin;
            PlatformView.DisplayMode = PdfDisplayMode.SinglePageContinuous;
            PlatformView.DisplaysPageBreaks = true;

            PlatformView.MaxScaleFactor = 4f;
            //PlatformView.MinScaleFactor = PlatformView.ScaleFactorForSizeToFit;
            PlatformView.MinScaleFactor = (nfloat)(UIScreen.MainScreen.Bounds.Height * 0.00075);

            PlatformView.AutoScales = true;
        }
    }
}
